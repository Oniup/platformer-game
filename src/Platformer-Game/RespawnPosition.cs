using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using Raylib_cs;

namespace PlatformerGame
{
    public class RespawnPosition(Vector2 position) : Actor(position)
    {
#if DEBUG
        public override void OnDraw()
        {
            if (World.ShowCollisionOutlines)
                Raylib.DrawRectangleV(Position - new Vector2(8), new Vector2(16), Color.Orange);
        }
#endif

        public class CreateInfo : CreateInfo<RespawnPosition>
        {
            public override string EntityIdentifier => "PlayerRespawnPosition";

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                return new RespawnPosition(info.Position);
            }
        }
    }

    public class RespawnEffect : AnimatedEffectActor
    {
        private SoundEffect _sound;

        public RespawnEffect(SpriteAtlas atlas, AnimationSet animations, SoundEffect sound, Vector2 position) 
            : base(atlas, animations, position)
        {
            _sound = sound;
        }

        public void SetToDisappear()
        {
            PlayAnimation("Disappear");
        }

        public override void OnDestroy()
        {
            _sound.Play();
        }

        public class CreateInfo : CreateInfo<RespawnEffect>
        {
            public override void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def)
            {
                var atlas = new SpriteAtlas(96, $"{resources.AssetDirectory}/Graphics/Effects/Appear and Disappear(96x96).png");

                var anims = new AnimationSet();
                anims.Add(atlas, "Appear", 0, 7, AnimationOption.PauseOnComplete);
                anims.Add(atlas, "Disappear", 1, 7, AnimationOption.PauseOnComplete);

                var sound = new SoundEffect([
                    $"{resources.AssetDirectory}/Sounds/Player/Hit/Teleport/tele_146.wav",
                ]);
                sound.SetVolume(0.3f);
                sound.SetPitchVariation(0.4f);

                resources.Load("Respawn Effect", atlas);
                resources.Load("Respawn Effect Animations", anims);
                resources.Load("Respawn Effect Sound", sound);
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var atlas = resources.Get<SpriteAtlas>("Respawn Effect");
                var anims = resources.Get<AnimationSet>("Respawn Effect Animations");

                var sound = resources.Get<SoundEffect>("Respawn Effect Sound");
                return new RespawnEffect(atlas, anims, sound, info.Position);
            }
        }
    }
}