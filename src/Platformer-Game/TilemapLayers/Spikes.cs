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
        public SpikeTilemapLayer(SpriteAtlas atlas, CollisionLayer layer, CollisionLayer mask, List<LDtkLevel.Tile> tiles, Vector2 position)
            : base(atlas, layer, mask, tiles, position)
        {
            _cellCollider.Height /= 2;
        }

        public override void OnUpdate(float deltaTime)
        {
            bool collision = CalculateCollisions();
            if (collision)
                EventDispatcher.FireEvent(new PlayerHitEvent(), this);
        }

        protected override Vector2 GetTileBoxColliderOffset(LDtkLevel.Tile tile)
        {
            return (Vector2)tile.ScenePosition + Vector2.UnitY * _cellCollider.Height;
        }

        public new class CreateInfo : CreateInfo<SpikeTilemapLayer>
        {
            public override string LayerIdentifier => "Spikes";

            public override TilemapLayer Instantiate(ResourceManager resources, LDtkDefinition.Tileset tileset, LDtkDefinition.Layer def, List<LDtkLevel.Tile> tiles, Vector2 worldPosition)
            {
                SpriteAtlas atlas = resources.Get<SpriteAtlas>(tileset.UId);
                return new SpikeTilemapLayer(atlas, CollisionLayer.Damage, CollisionLayer.All & ~CollisionLayer.Player, tiles, worldPosition);
            }
        }
    }
}