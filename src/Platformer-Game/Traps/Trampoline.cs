using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame.Traps
{
    public class Trampoline : CharacterActor
    {
        private float _bounceForce = 16000.0f;

        public Trampoline(SpriteAtlas atlas, AnimationSet animations, CollisionLayer layer, CollisionLayer mask, Vector2 position) : base(atlas, animations, layer, mask, position)
        {
            AddBoxCollider(Vector2.UnitY * 10.0f, 26, 14, OnTriggerEnter);
        }

        private void OnTriggerEnter(CollidableActor other, ShapeCollider collider)
        {
            if (other is CharacterActor character && CurrentAnimation != "Bounce")
            {
                character.Velocity = new Vector2(character.Velocity.X, 0.0f);
                character.ApplyImpulse -= Vector2.UnitY * _bounceForce;
                PlayAnimation("Bounce");
            }
        }

        public class CreateInfo : CreateInfo<Trampoline>
        {
            public override void SetupRequiredResources(LDtkDefinition.Entity? def, ResourceManager resources)
            {
                SpriteAtlas atlas = resources.Get<SpriteAtlas>((int)def!.TilesetId!);

                AnimationSet anims = new AnimationSet();
                anims.Add(atlas, "Idle", 1, 1);
                anims.Add(atlas, "Bounce", 0, 8, AnimationMode.UninterruptableUntilComplete, "Idle");

                resources.Load("Trampoline Animations", anims);
            }

            public override Actor Instantiate(ResourceManager resources, Scene? scene, LDtkDefinition.Entity? def, Vector2 position)
            {
                SpriteAtlas atlas = resources.Get<SpriteAtlas>((int)def!.TilesetId!);
                AnimationSet anims = resources.Get<AnimationSet>("Trampoline Animations");
                return new Trampoline(atlas, anims, CollisionLayer.Trap | CollisionLayer.Ground, CollisionLayer.None, position);
            }
        }
    }
}