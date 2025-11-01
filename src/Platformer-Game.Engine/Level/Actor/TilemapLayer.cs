using System.Numerics;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Utilities;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.Engine.Level
{
    public partial class TilemapLayer : CollidableActor
    {
        private SpriteAtlas _atlas;
        protected List<LDtkLevel.Tile> _tiles;

        public TilemapLayer(SpriteAtlas atlas, CollisionLayer layer, CollisionLayer mask, SpawnInfo info, bool initializeBoxColliders = true)
            : base(layer, mask, info.WorldPosition)
        {
            _atlas = atlas;
            _tiles = info.Tiles;
            if (initializeBoxColliders)
                InitializeCollisionBoxes(info.Scene, _atlas.GridWidth, _atlas.GridHeight, info.CsvGrid);
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
                _atlas.Draw(Position + tile.ScenePosition, false, false);
            }

#if DEBUG
            // Draw collision boxes
            base.OnDraw();
#endif
        }

        protected override bool IsColliding(CollidableActor actor, out Vector2 displacement)
        {
            displacement = Vector2.Zero;
            bool collisionDetected = false;
            foreach (BoxCollider boxCollider in Colliders)
            {
                Vector2 thisDisplacement = Vector2.Zero;
                foreach (ShapeCollider collider in actor.Colliders)
                {
                    if (boxCollider.IsColliding(this, actor, collider, ref thisDisplacement) && ApplyDisplacement(actor, collider, thisDisplacement, ref displacement))
                        collisionDetected = true;
                }
            }
            return collisionDetected;
        }

        protected virtual bool ApplyDisplacement(CollidableActor actor, ShapeCollider collider, Vector2 thisDisplacement, ref Vector2 displacement)
        {
            displacement += thisDisplacement;
            return true;
        }

        /// <summary>
        /// Creates the collision boxes for the tilemap. It optimizes by joining the relative cells that require a box
        /// collider into one. Only does this for the x axis
        /// </summary>
        /// <param name="scene">Number of cells required</param>
        /// <param name="colliderWidth"></param>
        /// <param name="colliderHeight"></param>
        /// <param name="csvGrid">Used to determine the neighboring tiles to the current tile</param>
        protected void InitializeCollisionBoxes(Scene scene, float colliderWidth, float colliderHeight, List<int> csvGrid)
        {
            if (_tiles.Count == 0 || csvGrid.Count == 0)
                return;

            int width = scene.Width / _atlas.GridWidth;
            int height = scene.Height / _atlas.GridHeight;

            float startX = 0.0f;
            float size = 0.0f;
            bool prevFilled = false;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool filled = csvGrid[x + y * width] > 0;
                    if (filled)
                    {
                        size += colliderWidth;
                        if (!prevFilled)
                            startX = x;
                        prevFilled = true;
                    }
                    else
                        AddTileCollider(startX, y, colliderHeight, ref size, ref prevFilled);
                }
                AddTileCollider(startX, y, colliderHeight, ref size, ref prevFilled);
            }
        }

        private void AddTileCollider(float startX, float y, float colliderHeight, ref float size, ref bool prevFilled)
        {
            if (!prevFilled)
                return;

            Colliders.Add(new TilemapBoxCollider
            {
                Offset = new Vector2(startX, y) * _atlas.GridSize,
                Size = new Vector2(size, colliderHeight),
            });

            size = 0.0f;
            prevFilled = false;
        }

        public class CreateInfo : CreateInfo<TilemapLayer>
        {
            public override TilemapLayer Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var atlas = resources.Get<SpriteAtlas>(info.TilesetId);
                return new TilemapLayer(atlas, CollisionLayer.Ground, CollisionLayer.None, info);
            }
        }
    }
}