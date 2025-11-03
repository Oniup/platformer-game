using System.Numerics;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.Engine.Level
{
    public abstract partial class Actor
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
        public virtual void OnAwake(Scene? scene)
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
    }
}