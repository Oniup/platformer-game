using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame.Traps
{
    public class Trampoline : CharacterActor
    {
        private float _bounceForce;

        public Trampoline(float bounceForce, SpriteAtlas atlas, AnimationSet animations, Vector2 position) 
            : base(atlas, animations, CollisionLayer.Trap, CollisionLayer.All & ~CollisionLayer.Player, position)
        {
            _bounceForce = bounceForce;
            AddBoxCollider(Vector2.UnitY * 10.0f, 18, 14, OnTriggerEnter);
        }

        private void OnTriggerEnter(CollidableActor other, ShapeCollider collider)
        {
            if (other is CharacterActor character && CurrentAnimation != "Bounce")
            {
                character.Velocity = new Vector2(character.Velocity.X, 0.0f);
                character.ApplyImpulse -= Vector2.UnitY * _bounceForce;
                PlayAnimation("Bounce");

                if (character is Player player)
                    player.ResetDoubleJump();
            }
        }

        public class CreateInfo : CreateInfo<Trampoline>
        {
            public override void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def)
            {
                var atlas = resources.Get<SpriteAtlas>((int)def!.TilesetId!);
                var anims = new AnimationSet();

                anims.Add(atlas, "Idle", 1, 1);
                anims.Add(atlas, "Bounce", 0, 8, AnimationOption.UninterruptibleUntilComplete, "Idle");
                resources.Load("Trampoline Animations", anims);
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var atlas = resources.Get<SpriteAtlas>((int)info.Definition!.TilesetId!);
                var anims = resources.Get<AnimationSet>("Trampoline Animations");

                var bounceForce = info.Fields!.GetValue<float>("BounceForce");

                return new Trampoline(bounceForce, atlas, anims, info.Position);
            }
        }
    }
}