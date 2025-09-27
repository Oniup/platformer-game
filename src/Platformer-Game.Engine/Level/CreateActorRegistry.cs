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
            return createInfo.Create(_resources, def, (Vector2)data.Position);
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
                    return createInfo.Create(_resources, def, position);
                }
            }
            throw new NullReferenceException($"{type.Name} Actor create info is not registered");
        }

        // public TilemapLayer CreateTilemapLayer(ResourceManager resources, Project project, LDtkLevel.Layer data)
        // {
        //     throw new NotImplementedException();
        // }
    }
}