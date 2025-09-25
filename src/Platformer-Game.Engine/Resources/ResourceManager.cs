using System.Diagnostics;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.Engine.Resources
{
    public class ResourceManager : IDisposable
    {
        private Dictionary<int, Resource> _resources;
        private string _assetDirectory;

        public ResourceManager(string assetDirectory)
        {
            _resources = new Dictionary<int, Resource>();
            _assetDirectory = assetDirectory;
        }

        public string AssetDirectory
        {
            get { return _assetDirectory; }
        }

        public bool Load(int id, Resource resource)
        {
            if (_resources.ContainsKey(id))
            {
                Debug.WriteLine("Cannot add the same resource twice");
                return false;
            }
            _resources.Add(id, resource);
            return true;
        }

        public bool Load(string name, Resource resource)
        {
            int id = name.GetHashCode();
            if (_resources.ContainsKey(id))
            {
                Debug.WriteLine("Cannot add the same resource twice");
                return false;
            }
            _resources.Add(id, resource);
            return true;
        }

        public T Get<T>(int id) where T : Resource
        {
            Resource? res;
            if (!_resources.TryGetValue(id, out res))
            {
                Type type = typeof(T);
                throw new NullReferenceException("Resource " + id + " of type " + type.Name + " has not been loaded");
            }
            return (T)res;
        }

        public T Get<T>(string name) where T : Resource
        {
            Resource? res;
            int id = name.GetHashCode();
            if (!_resources.TryGetValue(id, out res))
            {
                Type type = typeof(T);
                throw new NullReferenceException("Resource " + name + " of type " + type.Name + " has not been loaded");
            }
            return (T)res;
        }

        public void LoadProject(Project projectData)
        {
            foreach (LDtkDefinition.Tileset tileset in projectData.Header.Defs.Tilesets)
            {
                if (tileset.Identifier == "Internal_Icons")
                    continue;
                string path = projectData.RootDirectory + tileset.RelPath;
                Load(tileset.UId, new SpriteAtlas(tileset.TileGridSize, path));
                Console.WriteLine("Loaded Resource " + tileset.Identifier);
            }
        }

        public string GetAssetPath(string relPath)
        {
            return _assetDirectory + "/" + relPath;
        }

        public void Dispose()
        {
            foreach ((int id, Resource res) in _resources)
                res.Dispose();
            _resources.Clear();
        }

#if DEBUG
        public void PrintAllResources()
        {
            foreach ((int id, Resource res) in _resources)
                Console.WriteLine($"Resource ID: {id}, Type: {res.Type}");
        }
#endif
    }
}