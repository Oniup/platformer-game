using System.Numerics;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.Engine.Level
{
    public partial class Actor
    {
        /// <summary>
        /// Parameters for spawning an Actor within the scene and is created by the <see cref="CreateActorRegistry">CreateActorRegistry</see>
        /// </summary>
        public readonly struct SpawnInfo
        {
            public Vector2 Position { get; init; }
            public Scene? Scene { get; init; }
            public LDtkDefinition.Entity? Definition { get; init; }
            public EntityFields? Fields { get; init; }
        }

        public interface ICreateInfo
        {
            public string EntityIdentifier { get; }
            public bool GlobalActor { get; }
            public int ActorMetaTypeId { get; }

            /// <summary>
            /// Setup any additional required resources after the project’s sprite atlases have been loaded
            /// </summary>
            /// <param name="resources">to recall, but mainly to load new resources to registry</param>
            /// <param name="def">Entity definition to recall any required loaded resources for creation of addition ones</param>
            public void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def);

            /// <summary>
            /// Creates an instance of a specific actor
            /// </summary>
            /// <param name="resources">to recall resources required for actor to pass into their constructor</param>
            /// <param name="info">Spawn info specifying their position</param>
            /// <returns></returns>
            public Actor Instantiate(ResourceRegistry resources, SpawnInfo info);
        }

        public abstract class CreateInfo<T> : ICreateInfo where T : Actor
        {
            public virtual string EntityIdentifier => typeof(T).Name;
            public virtual bool GlobalActor => false;
            public int ActorMetaTypeId => typeof(T).GetHashCode();

            public virtual void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def) { }
            public abstract Actor Instantiate(ResourceRegistry resources, SpawnInfo info);
        }
    }

    public partial class TilemapLayer
    {
        public readonly new struct SpawnInfo
        {
            public Vector2 WorldPosition { get; init; }
            public Scene Scene { get; init; }
            public int TilesetId { get; init; }
            public List<int> CsvGrid { get; init; }
            public List<LDtkLevel.Tile> Tiles { get; init; }
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
            public void SetupRequiredResources(LDtkDefinition.Tileset tileset, ResourceRegistry resources);

            /// <summary>
            /// Creates an instance of a derived TilemapLayer type to populate the assigned scene.
            /// </summary>
            /// <param name="resources">Resource manager for fetching required resources</param>
            /// <param name="tileset">The tileset definition that supplies tile SpriteAtlas</param>
            /// <param name="def">The LDtk layer definition containing size, grid, and other settings</param>
            /// <param name="tiles">List of tiles that define graphics to show and where to draw it</param>
            /// <param name="worldPosition">World position</param>
            /// <returns>A fully‑initialized <see cref="TilemapLayer"/> ready for insertion into a scene</returns>
            public TilemapLayer Instantiate(ResourceRegistry resources, SpawnInfo info);
        }

        public new abstract class CreateInfo<T> : ICreateInfo where T : TilemapLayer
        {
            public virtual string LayerIdentifier => typeof(T).Name;
            public virtual bool GlobalActor => false;
            public int ActorTypeId => typeof(T).GetHashCode();

            public virtual void SetupRequiredResources(LDtkDefinition.Tileset tileset, ResourceRegistry resources) { }
            public abstract TilemapLayer Instantiate(ResourceRegistry resources, SpawnInfo info);
        }
    }
}