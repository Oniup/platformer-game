using System.Numerics;
using PlatformerGame.Engine.Level.Collision;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.Engine.Level
{
    public class TilemapLayer : CollidableActor
    {
        protected SpriteAtlas _atlas;
        protected List<LDtkLevel.Tile> _tiles;
        protected TilemapBoxCollider _cellCollider;

        public TilemapLayer(SpriteAtlas atlas, CollisionLayer layer, CollisionLayer mask, List<LDtkLevel.Tile> tiles, Vector2 position)
            : base(layer, mask, CollisionActorType.Tilemap, position)
        {
            _atlas = atlas;
            _tiles = tiles;
            _cellCollider = new TilemapBoxCollider()
            {
                Offset = Vector2.Zero,
                Width = atlas.GridWidth,
                Height = atlas.GridHeight,
            };
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

#if DEBUG
                if (World.ShowCollisionOutlines)
                {
                    _cellCollider.Offset = GetTileBoxColliderOffset(tile);
                    _cellCollider.DrawOutline(Position);
                }
#endif
            }
        }

        protected override bool IsColliding(CollidableActor actor, out Vector2 displacement)
        {
            displacement = Vector2.Zero;
            if (actor.CollisionType == CollisionActorType.Shapes)
                return CollidingWithShapes((CollisionShapeActor)actor, ref displacement);
            return false;
        }

        protected bool CollidingWithShapes(CollisionShapeActor actor, ref Vector2 displacement)
        {
            bool collisionDetected = false;

            foreach (LDtkLevel.Tile tile in _tiles)
            {
                _cellCollider.Offset = GetTileBoxColliderOffset(tile);

                Vector2 thisDisplacement = Vector2.Zero;
                foreach (ShapeCollider collider in actor.Colliders)
                {
                    if (_cellCollider.IsColliding(this, actor, collider, ref thisDisplacement)
                        && ApplyDisplacement(actor, collider, thisDisplacement, ref displacement))
                    {
                        collisionDetected = true;
                    }
                }
            }
            return collisionDetected;
        }

        protected virtual Vector2 GetTileBoxColliderOffset(LDtkLevel.Tile tile)
        {
            return tile.ScenePosition;
        }

        protected virtual bool ApplyDisplacement(CollisionShapeActor actor, ShapeCollider collider, Vector2 thisDisplacement, ref Vector2 displacement)
        {
            displacement += thisDisplacement;
            return true;
        }

        public new interface ICreateInfo
        {
            public string LayerIdentifier { get; }
            public int ActorTypeId { get; }

            /// <summary>
            /// Setup any additional resources that the layer needs after the project’s sprite atlases have been loaded.
            /// </summary>
            /// <param name="tileset">The tileset definition referenced by the LDtk layer</param>
            /// <param name="resources">Resource manager to recall any previously loaded resources</param>
            public void SetupRequiredResources(LDtkDefinition.Tileset tileset, ResourceManager resources);

            /// <summary>
            /// Creates an instance of a derived TilemapLayer type to populate the assigned scene.
            /// </summary>
            /// <param name="resources">Resource manager for fetching required resources</param>
            /// <param name="tileset">The tileset definition that supplies tile SpriteAtlas</param>
            /// <param name="def">The LDtk layer definition containing size, grid, and other settings</param>
            /// <param name="tiles">List of tiles that define graphics to show and where to draw it</param>
            /// <param name="worldPosition">World position</param>
            /// <returns>A fully‑initialized <see cref="TilemapLayer"/> ready for insertion into a scene</returns>
            public TilemapLayer Instantiate(ResourceManager resources, LDtkDefinition.Tileset tileset, LDtkDefinition.Layer def, List<LDtkLevel.Tile> tiles, Vector2 worldPosition);
        }

        public new abstract class CreateInfo<T> : ICreateInfo
        {
            public virtual string LayerIdentifier => typeof(T).Name;
            public virtual bool GlobalActor => false;
            public int ActorTypeId => typeof(T).GetHashCode();

            public virtual void SetupRequiredResources(LDtkDefinition.Tileset tileset, ResourceManager resources) { }
            public abstract TilemapLayer Instantiate(ResourceManager resources, LDtkDefinition.Tileset tileset, LDtkDefinition.Layer def, List<LDtkLevel.Tile> tiles, Vector2 worldPosition);
        }

        public class CreateInfo : CreateInfo<TilemapLayer>
        {
            public override TilemapLayer Instantiate(ResourceManager resources, LDtkDefinition.Tileset tileset, LDtkDefinition.Layer def, List<LDtkLevel.Tile> tiles, Vector2 worldPosition)
            {
                SpriteAtlas atlas = resources.Get<SpriteAtlas>(tileset.UId);
                return new TilemapLayer(atlas, CollisionLayer.Ground, CollisionLayer.None, tiles, worldPosition);
            }
        }
    }

    public class TilemapBoxCollider : BoxCollider
    {
        public override Vector2 CornerOffset => Vector2.Zero;

        protected override bool CollideWithCircle(Vector2 position, Vector2 otherPosition, CircleCollider collider, ref Vector2 displacement)
        {
            Vector2 circleCenter = otherPosition + collider.Offset;
            Vector2 boxTopLeft = position + Offset;
            Vector2 boxBottomRight = position + Offset + new Vector2(Width, Height);
            if (CircleVsBox(circleCenter, collider.Radius, boxTopLeft, boxBottomRight, ref displacement))
            {
                displacement = -displacement;
                return true;
            }
            return false;
        }

        protected override bool CollidateWithBox(Vector2 position, Vector2 otherPosition, BoxCollider collider, ref Vector2 displacement)
        {
            Vector2 tileTopLeft = position + Offset;
            Vector2 tileBottomRight = position + Offset + new Vector2(Width, Height);
            Vector2 topLeft = otherPosition + collider.Offset - collider.CornerOffset;
            Vector2 bottomRight = otherPosition + collider.Offset + collider.CornerOffset;
            return BoxVsBox(tileTopLeft, tileBottomRight, topLeft, bottomRight, ref displacement);
        }
    }
}