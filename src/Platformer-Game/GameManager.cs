using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.UI;
using Raylib_cs;

namespace PlatformerGame
{
    public class GameManager : Actor
    {
        private Player _player = null!;
        private PauseCanvas _pauseCanvas = null!;
        private Vector2 _sceneTopLeft;
        private Vector2 _sceneBottomRight;

        public GameManager(Vector2 position) : base(position)
        {
            EventDispatcher.AddListener<NewCurrentSceneEvent>(this, OnNewCurrentSceneEvent);
        }

        public override void OnAwake()
        {
            _player = World.Find<Player>().First();
            _pauseCanvas = World.Find<PauseCanvas>().First();
            SetSceneBoundingBox(World.CurrentScene);
        }

        public override void OnUpdate(float deltaTime)
        {
            CheckPlayerExitScene();

            ProcessInputs(out bool pausedPressed);
            if (pausedPressed)
            {
                World.Paused = !World.Paused;
                _pauseCanvas.Showing = World.Paused;
            }
        }

        private static void ProcessInputs(out bool pausedPressed)
        {
            pausedPressed = Raylib.IsKeyPressed(KeyboardKey.Escape);
            if (!pausedPressed && Raylib.IsGamepadAvailable(0))
                pausedPressed = Raylib.IsGamepadButtonPressed(0, GamepadButton.MiddleRight);
#if DEBUG
            if (Raylib.IsKeyPressed(KeyboardKey.F1))
                World.ShowCollisionOutlines = !World.ShowCollisionOutlines;
#endif
        }

        private void SetSceneBoundingBox(Scene scene)
        {
            _sceneTopLeft = scene.WorldOffset;
            _sceneBottomRight = _sceneTopLeft + scene.Size;
        }

        private void CheckPlayerExitScene()
        {
            string? exitDir = null;
            if (_player.Position.X > _sceneBottomRight.X)
                exitDir = "e";
            else if (_player.Position.X < _sceneTopLeft.X)
                exitDir = "w";
            else if (_player.Position.Y < _sceneTopLeft.Y)
                exitDir = "n";
            else if (_player.Position.Y > _sceneBottomRight.Y)
                exitDir = "s";

            if (exitDir != null)
            {
                EventDispatcher.FireEvent(new SetNewCurrentSceneEvent(
                    exitDir, SetNewCurrentSceneEvent.IdentifierType.NeighbouringDirection), this);
            }
        }

        private void OnNewCurrentSceneEvent(Event eventData, object? sender)
        {
            NewCurrentSceneEvent data = (NewCurrentSceneEvent)eventData;
            SetSceneBoundingBox(data.Scene);
        }

        public class CreateInfo : CreateInfo<GameManager>
        {
            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                return new GameManager(info.Position);
            }
        }
    }
}