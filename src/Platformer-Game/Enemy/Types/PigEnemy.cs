using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame
{
    public class PigEnemy : Enemy
    {
        private readonly float _runSpeed = 200.0f;

        private readonly float _angryDetectRange;
        private readonly bool _noWalkState;

        public PigEnemy(SpriteAtlas atlas, AnimationSet animations, SoundEffect hitSound, EntityFields fields, Vector2 position)
            : base(atlas, animations, hitSound, position)
        {
            _angryDetectRange = fields.GetValue<float>("AngryDetectRange");
            _noWalkState = fields.GetValue<bool>("NoWalkState");

            DeathScore = 2;
            MoveDirection = fields.GetValue<float>("StartMoveDirection");
            CurrentState = fields.GetValue<bool>("StartWithWalkState") && !_noWalkState ? new WalkState(this) : new IdleState(this);
            WalkingSpeed = 75.0f;

            SetupRequiredColliders(Vector2.UnitY * 5, -Vector2.UnitY * 7, atlas.GridSize - new Vector2(10));
            SetupVisionCollider(fields.GetValue<float>("DetectRange"), atlas.GridHeight * 0.9f, 0.0f);
        }

        private class IdleState(PigEnemy self, bool shouldSwitchDirection = false) : IdleState<PigEnemy>(self, shouldSwitchDirection)
        {
            public override IState? SwitchState()
            {
                if (Self.IsSeeingPlayer())
                    return new AngryRunState(Self);

                if (SwitchToWalkState())
                    return new WalkState(Self);

                return null;
            }
        }

        private class WalkState(PigEnemy self) : WalkState<PigEnemy>(self)
        {
            public override IState? SwitchState()
            {
                if (Self.IsSeeingPlayer())
                    return new AngryRunState(Self);

                if (SwitchToIdleState(out bool idleShouldSwitchDirection))
                    return new IdleState(Self, idleShouldSwitchDirection);

                return null;
            }
        }

        private class AngryRunState : State<PigEnemy>
        {
            private Player _player;

            public AngryRunState(PigEnemy self) 
                : base(self)
            {
                Self.MaxVelocityCap = new Vector2(Self._runSpeed, Self.MaxVelocityCap.Y);
                Self.PlayAnimation("AngryRun");
                _player = (Player)Self.VisibleActor!;
            }

            public override void OnUpdate(float deltaTime)
            {
                Self.MoveDirection = Math.Sign(_player.Position.X - Self.Position.X);
                if (Self.MoveDirection != Math.Sign(Self.Velocity.X))
                    Self.Velocity += Vector2.UnitX * (Self.MoveDirection * Self._runSpeed * 2 * deltaTime);
                else
                    Self.Velocity += Vector2.UnitX * (Self.MoveDirection * Self._runSpeed * deltaTime);
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
                // Required enemy animations
                anims.Add(atlas, "Idle", 0, 9);
                anims.Add(atlas, "Walk", 2, 15);
                anims.Add(atlas, "Hit", 3, 5, AnimationOption.UninterruptibleUntilComplete | AnimationOption.PauseOnComplete);
                // Specialized animations
                anims.Add(atlas, "AngryRun", 1, 12);

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