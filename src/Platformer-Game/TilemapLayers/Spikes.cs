using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame
{
    public class SpikeTilemapLayer : TilemapLayer
    {
        public SpikeTilemapLayer(SpriteAtlas atlas, SpawnInfo info)
            : base(atlas, CollisionLayer.Damage, CollisionLayer.All & ~CollisionLayer.Player, info, false)
        {
            int width = info.Scene.Width / SpriteAtlas.GridWidth;
            int height = info.Scene.Height / SpriteAtlas.GridHeight;
            Vector2 tileHalfSize = atlas.GridSize / 2;

            float liniencySize = 2f;
            float liniencyOffset = 1f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 size;
                    var offset = Vector2.Zero;

                    var id = (SpikeCsvId)info.CsvGrid[x + y * width];
                    switch (id)
                    {
                        case SpikeCsvId.Bottom:
                            size.X = atlas.GridWidth;
                            size.Y = tileHalfSize.Y;
                            offset.Y = tileHalfSize.Y + liniencySize;
                            offset.X = liniencyOffset;
                            break;
                        case SpikeCsvId.Left:
                            size.Y = atlas.GridHeight;
                            size.X = tileHalfSize.X;
                            offset.Y = liniencyOffset;
                            break;
                        case SpikeCsvId.Right:
                            size.Y = atlas.GridHeight;
                            size.X = tileHalfSize.X;
                            offset.X = tileHalfSize.X + liniencySize;
                            offset.Y = liniencyOffset;
                            break;
                        case SpikeCsvId.Top:
                            size.X = atlas.GridWidth;
                            size.Y = tileHalfSize.Y;
                            offset.X = liniencyOffset;
                            break;
                        default:
                            continue;
                    }
                    Colliders.Add(new TilemapBoxCollider
                    {
                        Offset = new Vector2(x, y) * atlas.GridSize + offset,
                        Width = size.X - liniencySize,
                        Height = size.Y - liniencySize,
                    });
                }
            }
        }

        private enum SpikeCsvId : int
        {
            None,
            Bottom = 1,
            Left,
            Right,
            Top,
        }

        public override void OnUpdate(float deltaTime)
        {
            bool collision = CalculateCollisions();
            if (collision)
                EventDispatcher.FireEvent(new PlayerHitEvent(), this);
        }

        public new class CreateInfo : CreateInfo<SpikeTilemapLayer>
        {
            public override string LayerIdentifier => "Spikes";

            public override TilemapLayer Instantiate(ResourceManager resources, SpawnInfo info)
            {
                var atlas = resources.Get<SpriteAtlas>(info.TilesetId);
                return new SpikeTilemapLayer(atlas, info);
            }
        }
    }
}