using System.Numerics;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.Engine.Level
{
    public abstract class Actor
    {
        protected Actor(Vector2 position)
        {
            Position = position;
        }

        public Vector2 Position { get; set; }
        public bool Destroy { get; set; }

        public virtual void OnDraw()
        {
        }

        public virtual void OnUpdate(float deltaTime)
        {
        }

        public virtual void OnFixedUpdate(float fixedDeltaTime)
        {
        }

        public virtual void OnLateUpdate(float deltaTime)
        {
        }

        public virtual void OnDestroy()
        {
        }

        public interface ICreateInfo
        {
            public string EntityIdentifier { get; }
            public bool GlobalActor { get; }
            public int ActorTypeId { get; }

            public void SetupRequiredResources(ResourceManager resources);
            public Actor Instantiate(ResourceManager resources, Scene? scene, LDtkDefinition.Entity? def, Vector2 position);
        }

        public abstract class CreateInfo<T> : ICreateInfo
        {
            public virtual string EntityIdentifier => typeof(T).Name;
            public virtual bool GlobalActor => false;
            public int ActorTypeId => typeof(T).GetHashCode();

            public virtual void SetupRequiredResources(ResourceManager resources) { }
            public abstract Actor Instantiate(ResourceManager resources, Scene? scene, LDtkDefinition.Entity? def, Vector2 position);
        }
    }
}