using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Level.UI;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.UI
{
    public class SelectPlayerCanvas : Canvas
    {
        public SelectPlayerCanvas(Vector2 position)
            : base(position)
        {
        }

        public class CreateInfo : CreateInfo<SelectPlayerCanvas>
        {
            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                throw new NotImplementedException();
            }
        }
    }
}