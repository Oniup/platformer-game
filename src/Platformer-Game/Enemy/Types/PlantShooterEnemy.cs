using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Utilities;
using Raylib_cs;

namespace PlatformerGame
{
    public class PlantShooterEnemy : Enemy
    {
        private readonly float _waitToNextShootDuration = 0.5f;
        private readonly Vector2 _projSpawnPointOffset = new Vector2(5, 1);

        public PlantShooterEnemy(SpriteAtlas atlas, AnimationSet animations, SoundEffect hitSound, EntityFields fields, Vector2 position)
            : base(atlas, animations, hitSound, position)
        {
            MoveDirection = fields.GetValue<float>("PointingDirection");
            CurrentState = new IdleState(this);

            var baseOffset = new Vector2(3, 7);
            SetupRequiredColliders(baseOffset, baseOffset - Vector2.UnitY * 16, atlas.GridSize - new Vector2(25, 15), false, false);
            SetupVisionCollider(fields.GetValue<float>("DetectRange"), atlas.GridHeight / 2, 2);
        }

        private Vector2 ProjectileSpawnPoint => Position + new Vector2(MoveDirection * _projSpawnPointOffset.X, _projSpawnPointOffset.Y);

#if DEBUG
        public override void OnDraw()
        {
            base.OnDraw();
            if (World.ShowCollisionOutlines)
                Raylib.DrawCircleV(ProjectileSpawnPoint, 3, Color.Red);
        }
#endif

        private class IdleState : IdleState<PlantShooterEnemy>
        {
            private DeltaTimer _waitToNextShootTimer;

            public IdleState(PlantShooterEnemy self)
                : base(self)
            {
                Self.PlayAnimation("Idle");
                _waitToNextShootTimer = new DeltaTimer(Self._waitToNextShootDuration, true);
            }

            public override void OnUpdate(float deltaTime)
            {
                base.OnUpdate(deltaTime);
                _waitToNextShootTimer.Update(deltaTime);
            }

            public override IState? SwitchState()
            {
                if (_waitToNextShootTimer.Finished && Self.IsSeeingPlayer())
                    return new ShootState(Self);

                return null;
            }
        }

        private class ShootState : State<PlantShooterEnemy>
        {
            public ShootState(PlantShooterEnemy self)
                : base(self)
            {
                Self.PlayAnimation("Shoot");

                ProjectileActor projectile = Self.World.Instantiate<PlantShooterProjectile>(Self.ProjectileSpawnPoint);
                projectile.Direction = new Vector2(Self.MoveDirection, 0.0f);
            }

            public override void OnUpdate(float deltaTime)
            {
            }

            public override IState? SwitchState()
            {
                if (Self.AnimationPaused)
                    return new IdleState(Self);
                return null;
            }
        }

        public class CreateInfo : CreateInfo<PlantShooterEnemy>
        {
            public override void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def)
            {
                var atlas = resources.Get<SpriteAtlas>((int)def?.TilesetId!);
                atlas.GridWidth = 44;
                atlas.GridHeight = 42;

                var anims = new AnimationSet();
                anims.Add(atlas, "Idle", 0, 10);
                anims.Add(atlas, "Shoot", 1, 7, AnimationOption.PauseOnComplete);
                anims.Add(atlas, "Hit", 2, 5, AnimationOption.ForceInterruptOnStart | AnimationOption.PauseOnComplete);

                resources.Load("Enemy PlantShooter Animations", anims);
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var atlas = resources.Get<SpriteAtlas>((int)info.Definition!.TilesetId!);
                var anims = resources.Get<AnimationSet>("Enemy PlantShooter Animations");
                var hitSound = resources.Get<SoundEffect>("Enemy Pig Hit Sound");

                return new PlantShooterEnemy(atlas, anims, hitSound, info.Fields!, info.Position);
            }
        }
    }

    public class PlantShooterProjectile(SpriteAtlas atlas, Vector2 position) 
        : ProjectileActor(atlas, Vector2.Zero, 180.0f, CollisionLayer.All & ~(CollisionLayer.Player | CollisionLayer.Ground), position)
    {
        protected override void OnTriggerEnter(CollidableActor actor, ShapeCollider collider)
        {
            if (!collider.IsTrigger)
            {
                if (actor.CollisionLayer.HasFlag(CollisionLayer.Player))
                    EventDispatcher.FireEvent(new PlayerHitEvent(), this);
                Destroy = true;
            }
        }

        public class CreateInfo : CreateInfo<PlantShooterProjectile>
        {
            public override void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def)
            {
                resources.Load("Enemy Projectiles", new SpriteAtlas(16, $"{resources.AssetDirectory}/Graphics/Enemies/Projectiles.png"));
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var atlas = resources.Get<SpriteAtlas>("Enemy Projectiles");
                return new PlantShooterProjectile(atlas, info.Position);
            }
        }
    }
}