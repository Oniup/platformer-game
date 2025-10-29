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

        /// <summary>
        /// World position
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// A reference to the world instance, in which is initialized after the constructor
        /// </summary>
        public World World { get; set; } = null!;

        /// <summary>
        /// Set to true if you want to destroy the actor
        /// </summary>
        public bool Destroy { get; set; }

        /// <summary>
        /// If the actor is created during the scene creation, this method is called after all actors have been loaded.
        /// Otherwise called after the constructor 
        /// </summary>
        public virtual void OnAwake()
        {
        }

        /// <summary>
        /// Called once per frame to update the actor’s state.
        /// </summary>
        /// <param name="deltaTime">Time, in seconds, since the previous frame.</param>
        public virtual void OnUpdate(float deltaTime)
        {
        }

        /// <summary>
        /// Called before all <c>OnUpdate</c> useful for pre‑processing.
        /// </summary>
        /// <param name="deltaTime">Time, in seconds, since the previous frame.</param>
        public virtual void OnBeforeUpdate(float deltaTime)
        {
        }

        /// <summary>
        /// Called when the engine is ready to render the actor.
        /// </summary>
        public virtual void OnDraw()
        {
        }

        /// <summary>
        /// Called when the actor is being removed from the scene or destroyed.
        /// </summary>
        public virtual void OnDestroy()
        {
        }

        /// <summary>
        /// Called when the actor is being removed from the scene or being destroyed and when clearing the scene to load another
        /// </summary>
        public virtual void OnDispose()
        {
        }

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
            public int ActorTypeId { get; }

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

        public abstract class CreateInfo<T> : ICreateInfo
        {
            public virtual string EntityIdentifier => typeof(T).Name;
            public virtual bool GlobalActor => false;
            public int ActorTypeId => typeof(T).GetHashCode();

            public virtual void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def) { }
            public abstract Actor Instantiate(ResourceRegistry resources, SpawnInfo info);
        }
    }
}