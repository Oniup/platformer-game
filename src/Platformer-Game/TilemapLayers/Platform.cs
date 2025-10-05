using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Level.Collision;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame
{
    public class PlatformTilemapLayer : TilemapLayer
    {
        public PlatformTilemapLayer(SpriteAtlas atlas, CollisionLayer layer, CollisionLayer mask, List<LDtkLevel.Tile> tiles, Vector2 position)
            : base(atlas, layer, mask, tiles, position)
        {
            _cellCollider.Height = 5;
        }

        protected override bool ApplyDisplacement(CollisionShapeActor actor, ShapeCollider collider, Vector2 thisDisplacement, ref Vector2 displacement)
        {
            // Figure out how to do this
            displacement += thisDisplacement;
            return true;
        }

        public new class CreateInfo : CreateInfo<PlatformTilemapLayer>
        {
            public override string LayerIdentifier => "Platform";

            public override TilemapLayer Instantiate(ResourceManager resources, LDtkDefinition.Tileset tileset, LDtkDefinition.Layer def, List<LDtkLevel.Tile> tiles, Vector2 worldPosition)
            {
                SpriteAtlas atlas = resources.Get<SpriteAtlas>(tileset.UId);
                return new PlatformTilemapLayer(atlas, CollisionLayer.Ground, CollisionLayer.None, tiles, worldPosition);
            }
        }
    }
}