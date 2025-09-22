/// <summary>
/// COS20007:       Custom Project
/// Name:           Ewan Robson
/// Student ID:     103992579
/// Created:        9-21-2025
/// Last Edited:    9-22-2025
/// </summary>

using System.Diagnostics;

namespace Game.Engine
{
    public class ResourceManager
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

        public bool Load(Resource resource)
        {
            if (_resources.ContainsKey(resource.Id))
            {
                Debug.WriteLine("Cannot add the same resource twice");
                return false;
            }
            _resources.Add(resource.Id, resource);
            return true;
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
            try
            {
                T res = Get<T>(name.GetHashCode());
                return res;
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("Failed to load resource with Name " + name);
            }
        }

        public void LoadProject(LDtkProjectData projectData)
        {
            // TODO....
        }

        public string GetAssetPath(string relPath)
        {
            return _assetDirectory + "/" + relPath;
        }
    }
}