using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame
{
    public class PigEnemy : Enemy
    {
        private readonly float _groundedDrag = 12.0f;

        private readonly float _angryDetectRange;
        private readonly float _idleWaitTime;
        private readonly float _walkSpeed;
        private readonly float _runForce;

        public PigEnemy(SpriteAtlas atlas, AnimationSet animations, SoundEffect hitSound, EntityFields fields, Vector2 position)
            : base(atlas, animations, hitSound, position)
        {
            _angryDetectRange = fields.GetValue<float>("AngryDetectRange");
            _idleWaitTime = fields.GetValue<float>("IdleWaitTime");
            _walkSpeed = fields.GetValue<float>("WalkSpeed");
            _runForce = fields.GetValue<float>("RunSpeed");

            DeathScore = fields.GetValue<int>("DeathScore");
            MoveDirection = fields.GetValue<float>("StartMoveDirection");
            CurrentState = fields.GetValue<bool>("StartWithWalkState") ? new WalkState(this) : new IdleState(this);

            float colliderWidth = atlas.GridWidth - 10;
            AddBoxCollider(Vector2.UnitY * 5, colliderWidth, atlas.GridHeight - 10, OnPlayerHit, true);
            AddBoxCollider(-Vector2.UnitY * 7, colliderWidth, 6, OnHeadHitTrigger);

            VisionCollider = AddBoxCollider(Vector2.Zero, fields.GetValue<float>("DetectRange"), atlas.GridHeight * 0.9f, OnVisionEnterTrigger);

            IsOnGroundCollider = AddBoxCollider(Vector2.UnitY * (atlas.GridHeight / 2), 5, 5, OnIsGroundInFrontTrigger);
            IsWallInFrontCollider = AddBoxCollider(Vector2.Zero, 5, 5, OnIsWallRightInFrontTrigger);
            CheckColliderOffset = atlas.GridWidth / 2;
        }

        protected class IdleState : State<PigEnemy>
        {
            private float _waitTimer;
            private bool _switchDirectionWhenStateChange;

            public IdleState(PigEnemy self, bool shouldSwitchDirection = false)
                : base(self)
            {
                _switchDirectionWhenStateChange = !shouldSwitchDirection;
                if (shouldSwitchDirection)
                    Self.MoveDirection = -Self.MoveDirection;
                Self.PlayAnimation("Idle");
            }

            public override void OnUpdate(float deltaTime)
            {
                _waitTimer += deltaTime;

                // Slow down to a stop
                if (Self.Velocity.X != 0)
                {
                    if (MathF.Abs(Self.Velocity.X) > 10f)
                        Self.Velocity -= new Vector2(Self.Velocity.X * Self._groundedDrag * deltaTime, 0.0f);
                    else
                        Self.Velocity = new Vector2(0.0f, Self.Velocity.Y);
                }
            }

            public override IState? SwitchState()
            {
                if (_waitTimer >= Self._idleWaitTime)
                {
                    if (_switchDirectionWhenStateChange)
                        Self.MoveDirection = -Self.MoveDirection;
                    return new WalkState(Self);
                }

                return null;
            }
        }

        protected class WalkState : State<PigEnemy>
        {
            public WalkState(PigEnemy self)
                : base(self)
            {
                Self.MaxVelocityCap = new Vector2(Self._walkSpeed, Self.MaxVelocityCap.Y);
                Self.PlayAnimation("Walk");
            }

            public override void OnUpdate(float deltaTime)
            {
                Self.Velocity += Vector2.UnitX * (Self.MoveDirection * Self._walkSpeed * deltaTime);
            }

            public override IState? SwitchState()
            {
                if (Self.IsSeeingPlayer())
                    return new AngryRunState(Self);

                if (Self.IsWallInFront)
                    return new IdleState(Self, true);
                if (!Self.IsGroundInFront)
                    return new IdleState(Self);

                return null;
            }
        }

        protected class AngryRunState : State<PigEnemy>
        {
            private Player _player;

            public AngryRunState(PigEnemy self) 
                : base(self)
            {
                Self.MaxVelocityCap = new Vector2(Self._runForce, Self.MaxVelocityCap.Y);
                Self.PlayAnimation("AngryRun");
                _player = (Player)Self.VisibleActor!;
            }

            public override void OnUpdate(float deltaTime)
            {
                Self.MoveDirection = Math.Sign(_player.Position.X - Self.Position.X);
                if (Self.MoveDirection != Math.Sign(Self.Velocity.X))
                    Self.Velocity += Vector2.UnitX * (Self.MoveDirection * Self._runForce * 2 * deltaTime);
                else
                    Self.Velocity += Vector2.UnitX * (Self.MoveDirection * Self._runForce * deltaTime);
            }

            public override IState? SwitchState()
            {
                float distance = Vector2.Distance(_player.Position, Self.Position);
                if (distance > Self._angryDetectRange)
                    return new IdleState(Self);

                return null;
            }
        }

        public class CreateInfo : CreateInfo<PigEnemy>
        {
            public override void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def)
            {
                var atlas = resources.Get<SpriteAtlas>((int)def!.TilesetId!);
                atlas.GridWidth = 36;
                atlas.GridHeight = 30;

                var anims = new AnimationSet();
                anims.Add(atlas, "Idle", 0, 9);
                anims.Add(atlas, "AngryRun", 1, 12);
                anims.Add(atlas, "Walk", 2, 15);
                anims.Add(atlas, "Hit", 3, 5, AnimationOption.UninterruptibleUntilComplete | AnimationOption.PauseOnComplete);
                resources.Load("Enemy Pig Animations", anims);

                var hitSound = new SoundEffect([
                    $"{resources.AssetDirectory}/Sounds/Enemy/Hit/CCS_01.wav",
                    $"{resources.AssetDirectory}/Sounds/Enemy/Hit/CCS_03.wav",
                ], 2);
                hitSound.SetPitchVariation(0.5f);
                hitSound.SetVolume(0.5f);
                resources.Load("Enemy Pig Hit Sound", hitSound);
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var atlas = resources.Get<SpriteAtlas>((int)info.Definition!.TilesetId!);
                var anims = resources.Get<AnimationSet>("Enemy Pig Animations");
                var hitSound = resources.Get<SoundEffect>("Enemy Pig Hit Sound");

                return new PigEnemy(atlas, anims, hitSound, info.Fields!, info.Position);
            }
        }
    }
}