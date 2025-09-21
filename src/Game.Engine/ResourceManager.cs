/// <summary>
/// COS20007:       Custom Project
/// Name:           Ewan Robson
/// Student ID:     103992579
/// Date Created:   9-21-2025
/// Date Created:   9-21-2025
/// </summary>

using System.Diagnostics;

namespace Game.Engine
{
    public class ResourceManager
    {
        private Dictionary<int, Resource> _resources;
        private string _assetDirectory;

        public ResourceManager()
        {
            // Load all application life time resources
            _resources = new Dictionary<int, Resource>();

            // Set asset directory
            DirectoryInfo? dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir != null)
            {
                foreach (DirectoryInfo subDir in dir.EnumerateDirectories())
                {
                    if (subDir.Name == "Assets")
                    {
                        _assetDirectory = subDir.FullName;
                        return;
                    }
                }
                dir = dir.Parent;
            }
            throw new NullReferenceException("Failed to find asset directory");
        }

        public string AssetDirectory
        {
            get { return _assetDirectory; }
        }

        public void Load(LDtkProjectData projectData)
        {
            // TODO....
        }

        public void Load(Resource resource)
        {
            if (_resources.ContainsKey(resource.Id))
            {
                Debug.WriteLine("Cannot add the same resource twice");
                return;
            }
            _resources.Add(resource.Id, resource);
        }

        public void Unload(int id)
        {
            _resources.Remove(id);
        }

        public T Get<T>(int id) where T : Resource
        {
            Resource? res;
            if (!_resources.TryGetValue(id, out res))
            {
                Type type = typeof(T);
                throw new NullReferenceException(type.Name + " resource with ID " + id + " has not been loaded");
            }
            return (T)res;
        }

        public T Get<T>(string name) where T : Resource
        {
            Resource? res;
            if (!_resources.TryGetValue(name.GetHashCode(), out res))
            {
                Type type = typeof(T);
                throw new NullReferenceException(type.Name + " resource with name " + name + " has not been loaded");
            }
            return (T)res!;
        }

        public string GetAssetPath(string relPath)
        {
            return _assetDirectory + "/" + relPath;
        }
    }
}