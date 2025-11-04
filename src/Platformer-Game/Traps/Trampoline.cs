using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame.Traps
{
    public class Trampoline : PawnActor
    {
        private SoundEffect _sound;
        private float _bounceForce;

        public Trampoline(float bounceForce, SpriteAtlas atlas, AnimationSet animations, SoundEffect sound, Vector2 position) 
            : base(atlas, animations, CollisionLayer.Trap, CollisionLayer.All & ~(CollisionLayer.Player | CollisionLayer.Enemy), position)
        {
            _bounceForce = bounceForce;
            _sound = sound;
            AddBoxCollider(Vector2.UnitY * 10.0f, 18, 14, OnTriggerEnter);
        }

        private void OnTriggerEnter(CollidableActor other, ShapeCollider collider)
        {
            if (!collider.IsTrigger && other is CharacterActor character && CurrentAnimation != "Bounce")
            {
                character.Velocity = new Vector2(character.Velocity.X, 0.0f);
                character.ApplyImpulse -= Vector2.UnitY * _bounceForce;
                PlayAnimation("Bounce");
                _sound.Play();

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
                var sound = new SoundEffect([
                    $"{resources.AssetDirectory}/Sounds/Traps/Trampoline/tele_022.wav",
                ], 5);
                sound.SetPitchVariation(0.6f);
                sound.SetVolume(0.5f);

                anims.Add(atlas, "Idle", 1, 1);
                anims.Add(atlas, "Bounce", 0, 8, AnimationOption.UninterruptibleUntilComplete, "Idle");
                resources.Load("Trampoline Animations", anims);
                resources.Load("Trampoline Sound", sound);
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var atlas = resources.Get<SpriteAtlas>((int)info.Definition!.TilesetId!);
                var anims = resources.Get<AnimationSet>("Trampoline Animations");
                var sound = resources.Get<SoundEffect>("Trampoline Sound");

                var bounceForce = info.Fields!.GetValue<float>("BounceForce");

                return new Trampoline(bounceForce, atlas, anims, sound, info.Position);
            }
        }
    }
}