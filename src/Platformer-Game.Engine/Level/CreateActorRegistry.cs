using System.Numerics;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.Engine.Level
{
    public class CreateActorRegistry
    {
        private Dictionary<int, Actor.ICreateInfo> _createInfos;
        private Dictionary<string, TilemapLayer.ICreateInfo> _layerCreateInfos;
        private ResourceManager _resources;
        private Project _project;

        public CreateActorRegistry(ResourceManager resources, Project project, Actor.ICreateInfo[] createInfos, TilemapLayer.ICreateInfo[] layerCreateInfos)
        {
            _createInfos = new Dictionary<int, Actor.ICreateInfo>();
            _layerCreateInfos = new Dictionary<string, TilemapLayer.ICreateInfo>();
            _resources = resources;
            _project = project;

            foreach (Actor.ICreateInfo createInfo in createInfos)
                Add(createInfo);
            foreach (TilemapLayer.ICreateInfo createInfo in layerCreateInfos)
                Add(createInfo);
        }

        public Actor Instantiate(LDtkLevel.Entity data, Scene? scene, out bool isGlobal)
        {
            LDtkDefinition.Entity? def = _project.GetEntityDefinition(data.DefUId) ?? throw new NullReferenceException($"Entity definition with UID {data.DefUId} doesn't exist");

            Actor.ICreateInfo? createInfo;
            if (!_createInfos.TryGetValue(data.DefUId, out createInfo))
                throw new NullReferenceException($"Create info assigned to {def.Identifier}, {def.UId} doesn't exist");

            isGlobal = createInfo.GlobalActor;
            return createInfo.Instantiate(_resources, scene, def, (Vector2)data.Position + WorldOffset(scene));
        }

        public Actor Instantiate<T>(Vector2 position, Scene? scene = null) where T : Actor
        {
            Type type = typeof(T);
            int queryId = type.GetHashCode();

            // Try get actor type id if the key is that
            {
                if (_createInfos.TryGetValue(queryId, out Actor.ICreateInfo? createInfo))
                {
                    return createInfo.Instantiate(_resources, scene, null, position);
                }
            }

            // Otherwise iterate through until found and provide entity definition
            foreach ((int id, Actor.ICreateInfo createInfo) in _createInfos)
            {
                if (createInfo.ActorTypeId == queryId)
                {
                    LDtkDefinition.Entity def = _project.GetEntityDefinition(id) ?? throw new NullReferenceException($"Type {type.Name} doesn't have a registered entity definition but has the create info?");
                    return createInfo.Instantiate(_resources, scene, def, position + WorldOffset(scene));
                }
            }
            throw new NullReferenceException($"{type.Name} Actor create info is not registered");
        }

        public TilemapLayer InstantiateTilemapLayer(LDtkLevel.Layer layer, Scene scene)
        {
            LDtkDefinition.Layer info = _project.GetLayerDefinition(layer.LayerDefUId) ?? throw new NullReferenceException($"Layer defintion with UID {layer.LayerDefUId} doesn't exist");
            if (info.TilesetDefUId == null)
                throw new NullReferenceException($"Layer {info.Identifier} doesn't have a tileset attached to it, cannot create a TilemapLayer");

            TilemapLayer.ICreateInfo? createInfo;
            _layerCreateInfos.TryGetValue(info.Identifier, out createInfo);
            if (createInfo == null)
                createInfo = new TilemapLayer.CreateInfo();

            LDtkDefinition.Tileset tileset = _project.GetTilesetDefinition((int)info.TilesetDefUId) ?? throw new NullReferenceException($"Layer {info.Identifier} is missing a tileset definition");
            Vector2 worldPosition = new Vector2(scene.WorldX, scene.WorldY) + new Vector2(layer.PxOffsetX, layer.PxOffsetY);
            return createInfo.Instantiate(_resources, tileset, info, layer.AutoLayerTiles, worldPosition);
        }

        private bool Add(Actor.ICreateInfo createInfo)
        {
            LDtkDefinition.Entity? def = _project.GetEntityDefinition(createInfo.EntityIdentifier);
            int key = def == null ? createInfo.ActorTypeId : def.UId;
            if (_createInfos.ContainsKey(key))
            {
                Console.WriteLine($"Cannot add duplicate {createInfo.EntityIdentifier} create infos");
                return false;
            }

            createInfo.SetupRequiredResources(def, _resources);
            _createInfos.Add(key, createInfo);
            return true;
        }

        private bool Add(TilemapLayer.ICreateInfo createInfo)
        {
            if (_project.GetLayerDefinition(createInfo.LayerIdentifier) == null)
                throw new NullReferenceException($"Tilemap Layers must be defined within the level editor");

            if (_layerCreateInfos.ContainsKey(createInfo.LayerIdentifier))
            {
                Console.WriteLine($"Cannot add duplicate {createInfo.LayerIdentifier} create infos");
                return false;
            }

            _layerCreateInfos.Add(createInfo.LayerIdentifier, createInfo);
            return true;
        }

        private Vector2 WorldOffset(Scene? scene)
        {
            return scene != null ? new Vector2(scene.WorldX, scene.WorldY) : Vector2.Zero;
        }
    }
}