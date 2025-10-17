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
        private RuntimeCanvas _runtimeCanvas = null!;
        private Vector2 _sceneTopLeft;
        private Vector2 _sceneBottomRight;
        private int _fruitsCollectedCount;
        private int _totalFruitCount;
        private int _hitCount;
        private float _timer = 0.0f;

        public GameManager(Vector2 position)
            : base(position)
        {
            EventDispatcher.AddListener<NewCurrentSceneEvent>(this, OnNewCurrentSceneEvent);
            EventDispatcher.AddListener<AddScoreEvent>(this, OnAddingScoreEvent);
            EventDispatcher.AddListener<PlayerHitEvent>(this, OnPlayerHitEvent);
        }

        public override void OnDispose()
        {
            EventDispatcher.RemoveListener<NewCurrentSceneEvent>(this);
            EventDispatcher.RemoveListener<AddScoreEvent>(this);
            EventDispatcher.RemoveListener<PlayerHitEvent>(this);
        }

        public override void OnAwake()
        {
            _player = World.Find<Player>().First();
            _pauseCanvas = World.Find<PauseCanvas>().First();
            _runtimeCanvas = World.Find<RuntimeCanvas>().First();
            _totalFruitCount = World.FindAllCount<Fruit>();
        }

        public override void OnUpdate(float deltaTime)
        {
            // Update UI
            base.OnUpdate(deltaTime);

            ProcessInputs(out bool pausedPressed);
            if (pausedPressed)
            {
                World.Paused = !World.Paused;
                _pauseCanvas.Showing = World.Paused;
            }

            CheckPlayerExitScene();

            if (!World.Paused)
            {
                _runtimeCanvas.SetTime(MathF.Round(_timer, 2));
                _timer += deltaTime;
            }
        }

        private static void ProcessInputs(out bool pausedPressed)
        {
            pausedPressed = Raylib.IsKeyPressed(KeyboardKey.Escape);
            if (!pausedPressed && Raylib.IsGamepadAvailable(0))
                pausedPressed = Raylib.IsGamepadButtonPressed(0, GamepadButton.MiddleRight) || Raylib.IsGamepadButtonPressed(0, GamepadButton.RightFaceRight);
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
            var data = (NewCurrentSceneEvent)eventData;
            SetSceneBoundingBox(data.Scene);
            Position = data.Scene.WorldOffset + data.Scene.Size;
        }

        private void OnAddingScoreEvent(Event eventData, object? sender)
        {
            var data = (AddScoreEvent)eventData;
            data.Handled = true;
            _fruitsCollectedCount += data.Score;
            _runtimeCanvas.SetScore(_fruitsCollectedCount);
        }

        private void OnPlayerHitEvent(Event eventData, object? sender)
        {
            _hitCount++;
            _runtimeCanvas.SetHit(_hitCount);
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