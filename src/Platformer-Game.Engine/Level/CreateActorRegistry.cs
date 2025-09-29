using System.Numerics;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.Engine.Level
{
    public class CreateActorRegistry
    {
        private Dictionary<int, Actor.ICreateInfo> _createInfos;
        private ResourceManager _resources;
        private Project _project;

        public CreateActorRegistry(ResourceManager resources, Project project, Actor.ICreateInfo[] createInfos)
        {
            _createInfos = new Dictionary<int, Actor.ICreateInfo>();
            _resources = resources;
            _project = project;
            foreach (Actor.ICreateInfo createInfo in createInfos)
                Add(createInfo);
        }

        public bool Add(Actor.ICreateInfo createInfo)
        {
            try
            {
                LDtkDefinition.Entity def = _project.GetEntityDefinition(createInfo.EntityIdentifier);
                if (_createInfos.ContainsKey(def.UId))
                {
                    Console.WriteLine($"Cannot add duplicate {createInfo.EntityIdentifier} create infos");
                    return false;
                }
                _createInfos.Add(def.UId, createInfo);
                return true;
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine($"Cannot add {createInfo.EntityIdentifier} create info: {e.Message}");
                return false;
            }
        }

        public Actor Instantiate(LDtkLevel.Entity data)
        {
            return Instantiate(data, out _);
        }

        public Actor Instantiate(LDtkLevel.Entity data, out bool isGlobal)
        {
            LDtkDefinition.Entity def = _project.GetEntityDefinition(data.DefUId);

            Actor.ICreateInfo? createInfo;
            if (!_createInfos.TryGetValue(data.DefUId, out createInfo))
                throw new NullReferenceException($"Create info assigned to {def.Identifier}, {def.UId} doesn't exist");

            isGlobal = createInfo.GlobalActor;
            return createInfo.Create(_resources, def, (Vector2)data.Position - PositionOffset(def));
        }

        public Actor Instantiate<T>(Vector2 position) where T : Actor
        {
            Type type = typeof(T);
            int queryId = type.GetHashCode();
            foreach ((int id, Actor.ICreateInfo createInfo) in _createInfos)
            {
                if (createInfo.ActorTypeId == queryId)
                {
                    LDtkDefinition.Entity def = _project.GetEntityDefinition(id);
                    return createInfo.Create(_resources, def, position - PositionOffset(def));
                }
            }
            throw new NullReferenceException($"{type.Name} Actor create info is not registered");
        }

        public TilemapLayer InstantiateTilemapLayer(LDtkLevel.Layer layer)
        {
            LDtkDefinition.Layer info = _project.GetLayerDefinition(layer.LayerDefUId);
            if (info.TilesetDefUId == null)
                throw new NullReferenceException($"Layer {info.Identifier} doesn't have a tileset attached to it, cannot create a TilemapLayer");

            LDtkDefinition.Tileset tileset = _project.GetTilesetDefinition((int)info.TilesetDefUId);
            SpriteAtlas atlas = _resources.Get<SpriteAtlas>(tileset.UId);
            Vector2 worldOffset = new Vector2(layer.PxOffsetX, layer.PxOffsetY);

            return new TilemapLayer(atlas, layer.AutoLayerTiles, layer.LayerDefUId, worldOffset);
        }

        private Vector2 PositionOffset(LDtkDefinition.Entity def)
        {
            return new Vector2(def.PivotX, def.PivotY) * new Vector2(def.Width, def.Height);
        }
    }
}