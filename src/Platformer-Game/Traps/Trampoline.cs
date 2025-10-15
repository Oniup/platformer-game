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
            : base(atlas, animations, CollisionLayer.Trap | CollisionLayer.Ground, CollisionLayer.None, position)
        {
            _bounceForce = bounceForce;
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
            public override void SetupRequiredResources(ResourceManager resources, LDtkDefinition.Entity? def)
            {
                SpriteAtlas atlas = resources.Get<SpriteAtlas>((int)def!.TilesetId!);

                AnimationSet anims = new AnimationSet();
                anims.Add(atlas, "Idle", 1, 1);
                anims.Add(atlas, "Bounce", 0, 8, AnimationOption.UninterruptableUntilComplete, "Idle");

                resources.Load("Trampoline Animations", anims);
            }

            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                if (info.Fields == null)
                    throw new NullReferenceException("Entity fields is required for Trampoline to initialize");

                SpriteAtlas atlas = resources.Get<SpriteAtlas>((int)info.Definition!.TilesetId!);
                AnimationSet anims = resources.Get<AnimationSet>("Trampoline Animations");

                float bounceForce = info.Fields.GetValue<float>("BounceForce");
                return new Trampoline(bounceForce, atlas, anims, info.Position);
                throw new NullReferenceException("Field BounceForce is required for instantiating Trampoline");
            }
        }
    }
}