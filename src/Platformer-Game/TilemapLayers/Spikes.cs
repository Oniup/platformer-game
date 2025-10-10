using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Level.Collision;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame
{
    public class SpikeTilemapLayer : TilemapLayer
    {
        public SpikeTilemapLayer(SpriteAtlas atlas, CollisionLayer layer, CollisionLayer mask, Scene scene, List<int> csvGrid, List<LDtkLevel.Tile> tiles, Vector2 position)
            : base(atlas, layer, mask, scene, tiles, position)
        {
            float colliderHeightY = atlas.GridHeight / 2;
            InitializeCollisionBoxes(scene, atlas.GridWidth, colliderHeightY, csvGrid);

            foreach (TilemapBoxCollider collider in _colliders)
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

            public override TilemapLayer Instantiate(ResourceManager resources, Scene scene, int tilesetId, List<int> csvGrid, List<LDtkLevel.Tile> tiles, Vector2 worldPosition)
            {
                SpriteAtlas atlas = resources.Get<SpriteAtlas>(tilesetId);
                return new SpikeTilemapLayer(atlas, CollisionLayer.Damage, CollisionLayer.All & ~CollisionLayer.Player, scene, csvGrid, tiles, worldPosition);
            }
        }
    }
}