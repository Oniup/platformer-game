using System.Numerics;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.Engine.Level
{
    public class CreateActorRegistry
    {
        private Dictionary<int, Actor.ICreateInfo> _createInfos;

        public CreateActorRegistry(Project project, Actor.ICreateInfo[] createInfos)
        {
            _createInfos = new Dictionary<int, Actor.ICreateInfo>();
            ValidateCreateInfos(project, createInfos);
        }

        public bool Add(Project project, Actor.ICreateInfo createInfo)
        {
            try
            {
                LDtkDefinition.Entity def = project.GetEntityDefinition(createInfo.EntityIdentifier);
                _createInfos.Add(def.UId, createInfo);
                return true;
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine($"Cannot add {createInfo.EntityIdentifier} create info: {e.Message}");
                return false;
            }
        }

        public Actor Instantiate(ResourceManager resources, Project project, LDtkLevel.Entity data)
        {
            LDtkDefinition.Entity def = project.GetEntityDefinition(data.DefUId);

            Actor.ICreateInfo? createInfo;
            if (!_createInfos.TryGetValue(data.DefUId, out createInfo))
                throw new NullReferenceException($"Create info assigned to {def.Identifier}, {def.UId} doesn't exist");

            return createInfo.Create(resources, def, (Vector2)data.Position);
        }

        public Actor Instantiate<T>(ResourceManager resources, Project project, Vector2 position) where T : Actor
        {
            Type type = typeof(T);
            int queryId = type.GetHashCode();
            foreach ((int id, Actor.ICreateInfo createInfo) in _createInfos)
            {
                if (createInfo.ActorTypeId == queryId)
                {
                    LDtkDefinition.Entity def = project.GetEntityDefinition(id);
                    return createInfo.Create(resources, def, position);
                }
            }
            throw new NullReferenceException($"{type.Name} Actor create info is not registered");
        }

        // public TilemapLayer CreateTilemapLayer(ResourceManager resources, Project project, LDtkLevel.Layer data)
        // {
        //     throw new NotImplementedException();
        // }

        private void ValidateCreateInfos(Project project, Actor.ICreateInfo[] createInfos)
        {
            foreach (Actor.ICreateInfo createInfo in createInfos)
                Add(project, createInfo);
        }
    }
}