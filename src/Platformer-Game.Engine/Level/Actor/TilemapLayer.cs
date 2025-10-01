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

        public TilemapLayer(SpriteAtlas atlas, List<LDtkLevel.Tile> tiles, Vector2 position)
            : base(position)
        {
            _atlas = atlas;
            _tiles = tiles;
        }

        protected List<LDtkLevel.Tile> Tiles
        {
            get { return _tiles; }
        }

        protected SpriteAtlas SpriteAtlas
        {
            get { return _atlas; }
        }

        public override void OnDraw()
        {
            foreach (LDtkLevel.Tile tile in _tiles)
            {
                _atlas.SetGrid(tile.AtlasPosition);
                _atlas.Draw(Position + (Vector2)tile.ScenePosition);
            }
        }

        public new interface ICreateInfo
        {
            public string LayerIdentifier { get; }
            public bool GlobalActor { get; }
            public int ActorTypeId { get; }

            public TilemapLayer Instantiate(ResourceManager resources, LDtkDefinition.Tileset tileset, LDtkDefinition.Layer def, List<LDtkLevel.Tile> tiles, Vector2 worldPosition);
        }

        public new abstract class CreateInfo<T> : ICreateInfo
        {
            public virtual string LayerIdentifier => typeof(T).Name;
            public virtual bool GlobalActor => false;
            public int ActorTypeId => typeof(T).GetHashCode();

            public abstract TilemapLayer Instantiate(ResourceManager resources, LDtkDefinition.Tileset tileset, LDtkDefinition.Layer def, List<LDtkLevel.Tile> tiles, Vector2 worldPosition);
        }

        public class CreateInfo : CreateInfo<TilemapLayer>
        {
            public override TilemapLayer Instantiate(ResourceManager resources, LDtkDefinition.Tileset tileset, LDtkDefinition.Layer def, List<LDtkLevel.Tile> tiles, Vector2 worldPosition)
            {
                SpriteAtlas atlas = resources.Get<SpriteAtlas>(tileset.UId);
                return new TilemapLayer(atlas, tiles, worldPosition);
            }
        }
    }
}