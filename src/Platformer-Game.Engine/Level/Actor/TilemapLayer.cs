using System.Numerics;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.Engine.Level
{
    // TODO: Inherit from collidable actor and add custom collision detection
    public class TilemapLayer : Actor
    {
        private SpriteAtlas _atlas;
        private List<LDtkLevel.Tile> _tiles;

        public TilemapLayer(SpriteAtlas atlas, List<LDtkLevel.Tile> tiles, int id, Vector2 position, bool active = true)
            : base(id, position, active)
        {
            _atlas = atlas;
            _tiles = tiles;
        }

        public override void OnDraw()
        {
            foreach (LDtkLevel.Tile tile in _tiles)
            {
                _atlas.SetGrid(tile.AtlasPosition);
                _atlas.Draw(Position + (Vector2)tile.ScenePosition);
            }
        }
    }
}