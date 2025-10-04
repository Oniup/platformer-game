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
        /// Set to true if you want to destroy the actor
        /// </summary>
        public bool Destroy { get; set; }

        /// <summary>
        /// Called once per frame to update the actor’s state.
        /// </summary>
        /// <param name="deltaTime">Time, in seconds, since the previous frame.</param>
        public virtual void OnUpdate(float deltaTime)
        {
        }

        /// <summary>
        /// Called at a fixed interval (independent of frame rate which can be defined by ApplicationCreateInfo.FixedUpdateTimeInterval) for physics‑related updates.
        /// </summary>
        /// <param name="fixedDeltaTime">Time, in seconds, between fixed‑update calls.</param>
        public virtual void OnFixedUpdate(float fixedDeltaTime)
        {
        }

        /// <summary>
        /// Called after all <c>OnUpdate</c> (and <c>OnFixedUpdate</c> when that is called), useful for post‑processing.
        /// </summary>
        /// <param name="deltaTime">Time, in seconds, since the previous frame.</param>
        public virtual void OnLateUpdate(float deltaTime)
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

        public interface ICreateInfo
        {
            public string EntityIdentifier { get; }
            public bool GlobalActor { get; }
            public int ActorTypeId { get; }

            /// <summary>
            /// Setup any additional required resources after the project’s sprite atlases have been loaded
            /// </summary>
            /// <param name="def">Entity definition to recall any required loaded resources for creation of addition ones</param>
            /// <param name="resources">Resource manager to recall any previously loaded resources</param>
            public void SetupRequiredResources(LDtkDefinition.Entity? def, ResourceManager resources);

            /// <summary>
            /// Creates an instance of a derived actor type to either populate the assigned scene or globally updating across all scenes.
            /// </summary>
            /// <param name="resources">Resource manager to recall required resources</param>
            /// <param name="scene">Object that will own the instance to be created. If null, then the object is global</param>
            /// <param name="def">Entity defintion used to recall required resources. If null, there is no project definition</param>
            /// <param name="position">World position</param>
            /// <returns>An instance of a dirived actor type</returns>
            public Actor Instantiate(ResourceManager resources, Scene? scene, LDtkDefinition.Entity? def, Vector2 position);
        }

        public abstract class CreateInfo<T> : ICreateInfo
        {
            public virtual string EntityIdentifier => typeof(T).Name;
            public virtual bool GlobalActor => false;
            public int ActorTypeId => typeof(T).GetHashCode();

            public virtual void SetupRequiredResources(LDtkDefinition.Entity? def, ResourceManager resources) { }
            public abstract Actor Instantiate(ResourceManager resources, Scene? scene, LDtkDefinition.Entity? def, Vector2 position);
        }
    }
}