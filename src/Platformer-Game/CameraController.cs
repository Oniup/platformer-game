using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame
{
    public class CameraController : Actor
    {
        private MainFramebuffer _renderTarget;
        private Player _player = null!;

        public CameraController(MainFramebuffer renderTarget, Vector2 position)
            : base(position)
        {
            _renderTarget = renderTarget;

            SetPosition(World.CurrentScene);

            EventDispatcher.AddListener<NewCurrentSceneEvent>(this, OnNewCurrentSceneEvent);
        }

        public override void OnAwake()
        {
            _player = World.Find<Player>().First();
        }

        public override void OnLateUpdate(float deltaTime)
        {
            _renderTarget.CameraPosition = new Vector2(
                Position.X - _renderTarget.FramebufferWidth * 0.5f,
                Position.Y - _renderTarget.FramebufferHeight * 0.5f
            );
        }

        private void OnNewCurrentSceneEvent(Event eventData, object? sender)
        {
            NewCurrentSceneEvent data = (NewCurrentSceneEvent)eventData;
            SetPosition(data.Scene);
        }

        private void SetPosition(Scene scene)
        {
            Position = new Vector2(
                scene.WorldX + scene.Width * 0.5f,
                scene.WorldY + scene.Height * 0.5f
            );
        }

        public class CreateInfo : CreateInfo<CameraController>
        {
            public override bool GlobalActor => false;

            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                MainFramebuffer renderTarget = resources.Get<MainFramebuffer>("Main Render Target");
                return new CameraController(renderTarget, info.Position);
            }
        }
    }
}