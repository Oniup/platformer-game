using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using Raylib_cs;

namespace PlatformerGame
{
    public class RespawnPosition : Actor
    {
        public RespawnPosition(Vector2 position)
            : base(position)
        {
        }

#if DEBUG
        public override void OnDraw()
        {
            if (World.ShowCollisionOutlines)
                Raylib.DrawRectangleV(Position - new Vector2(8), new Vector2(16), Color.Orange);
        }
#endif

        public class CreateInfo : CreateInfo<RespawnPosition>
        {
            public override Actor Instantiate(ResourceManager resources, Scene? scene, LDtkDefinition.Entity? def, Vector2 position)
            {
                return new RespawnPosition(position);
            }
        }
    }
}