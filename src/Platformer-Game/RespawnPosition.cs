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

            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                return new RespawnPosition(info.Position);
            }
        }
    }

    public class RespawnEffect(SpriteAtlas atlas, AnimationSet animations, Vector2 position) : AnimatedEffectActor(atlas, animations, position)
    {
        public class CreateInfo : CreateInfo<RespawnEffect>
        {
            public override void SetupRequiredResources(LDtkDefinition.Entity? def, ResourceManager resources)
            {
                string asset = resources.AssetDirectory + "/Graphics/Effects/Appearing (96x96).png";
                SpriteAtlas atlas = new SpriteAtlas(96, asset);
                AnimationSet anims = new AnimationSet();

                anims.Add(atlas, "Init", 0, 7, AnimationMode.PauseOnComplete);

                resources.Load("Respawn Effect", atlas);
                resources.Load("Respawn Effect Animations", anims);
            }

            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                SpriteAtlas atlas = resources.Get<SpriteAtlas>("Respawn Effect");
                AnimationSet anims = resources.Get<AnimationSet>("Respawn Effect Animations");
                return new RespawnEffect(atlas, anims, info.Position);
            }
        }
    }
}