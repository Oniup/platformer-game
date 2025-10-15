using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame
{
    public class SpikeTilemapLayer : TilemapLayer
    {
        public SpikeTilemapLayer(SpriteAtlas atlas, SpawnInfo info)
            : base(atlas, CollisionLayer.Damage, CollisionLayer.All & ~CollisionLayer.Player, info, false)
        {
            float colliderHeightY = atlas.GridHeight / 2;
            InitializeCollisionBoxes(info.Scene, atlas.GridWidth, colliderHeightY, info.CsvGrid);

            foreach (BoxCollider collider in Colliders)
                collider.Offset += Vector2.UnitY * colliderHeightY;
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