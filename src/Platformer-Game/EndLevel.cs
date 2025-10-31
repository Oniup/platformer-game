using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame
{
    public class EndLevel : CharacterActor
    {
        private float _impulseForce = 3000.0f;

        public EndLevel(SpriteAtlas atlas, AnimationSet animations, Vector2 position)
            : base(atlas, animations, CollisionLayer.Ground, CollisionLayer.All & ~CollisionLayer.Player, position)
        {
            AddBoxCollider(Vector2.UnitY * 10f, 46, 44);
            AddBoxCollider(Vector2.UnitY * -10f, 44, 13, OnPlayerEnter);
        }

        private void OnPlayerEnter(CollidableActor other, ShapeCollider collider)
        {
            var player = (CharacterActor)other;
            player.ApplyImpulse -= Vector2.UnitY * _impulseForce;
            player.Velocity = Vector2.Zero;
            DisabledCollision = true;

            EventDispatcher.FireEvent(new LevelComplete());
        }

        public class CreateInfo : CreateInfo<EndLevel>
        {
            public override void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def)
            {
                var atlas = resources.Get<SpriteAtlas>((int)def!.TilesetId!);
                var anims = new AnimationSet();

                anims.Add(atlas, "Idle", 0, 1);
                anims.Add(atlas, "Trigger", 1, 8, AnimationOption.PauseOnComplete);

                resources.Load("End Level Animations", anims);
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var atlas = resources.Get<SpriteAtlas>((int)info.Definition!.TilesetId!);
                var anims = resources.Get<AnimationSet>("End Level Animations");
                return new EndLevel(atlas, anims, info.Position);
            }
        }
    }
}