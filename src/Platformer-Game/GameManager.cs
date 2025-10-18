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
        private enum LevelStatus
        {
            NotComplete,
            TransitionToComplete,
            Complete,
        }

        private Player _player = null!;

        // UI canvases
        private PauseCanvas _pauseCanvas = null!;
        private RuntimeCanvas _runtimeCanvas = null!;

        // Scene bounding box for checking player exiting scene
        private Vector2 _sceneTopLeft;
        private Vector2 _sceneBottomRight;

        // Run metrics
        private int _totalFruitCount;
        private int _fruitsCollectedCount;
        private int _hitCount;
        private float _timer = 0.0f;

        // On level complete
        private readonly float _levelCompleteDelayDurations = 1.0f;
        private float _levelCompleteTimer;
        private LevelStatus _levelStatus = LevelStatus.NotComplete;

        public GameManager(Vector2 position)
            : base(position)
        {
            EventDispatcher.AddListener<NewCurrentSceneEvent>(this, OnNewCurrentSceneEvent);
            EventDispatcher.AddListener<AddScoreEvent>(this, OnAddingScoreEvent);
            EventDispatcher.AddListener<PlayerHitEvent>(this, OnPlayerHitEvent);
            EventDispatcher.AddListener<LevelComplete>(this, OnLevelComplete);
        }

        public override void OnAwake()
        {
            _player = World.Find<Player>().First();
            _pauseCanvas = World.Find<PauseCanvas>().First();
            _runtimeCanvas = World.Find<RuntimeCanvas>().First();
            _totalFruitCount = World.FindAllCount<Fruit>();

            Console.WriteLine($"{World.CurrentScene.WorldOffset}");
        }

        public override void OnDispose()
        {
            EventDispatcher.RemoveListener<NewCurrentSceneEvent>(this);
            EventDispatcher.RemoveListener<AddScoreEvent>(this);
            EventDispatcher.RemoveListener<PlayerHitEvent>(this);
            EventDispatcher.RemoveListener<LevelComplete>(this);
        }

        public override void OnUpdate(float deltaTime)
        {
            switch (_levelStatus)
            {
                case LevelStatus.NotComplete:
                    NotCompleteRuntime(deltaTime);
                    break;
                case LevelStatus.TransitionToComplete:
                    TransitionToComplete(deltaTime);
                    break;
                case LevelStatus.Complete:
                    break;
            }
        }

        private void NotCompleteRuntime(float deltaTime)
        {
            ProcessInputs(out bool pausedPressed);
            if (pausedPressed)
            {
                World.Paused = !World.Paused;
                _pauseCanvas.Showing = World.Paused;
            }

            if (World.Paused)
                return;
            CheckPlayerExitScene();
            _runtimeCanvas.SetTime(MathF.Round(_timer, 2));
            _timer += deltaTime;
        }

        private void TransitionToComplete(float deltaTime)
        {
            if (_levelCompleteTimer > _levelCompleteDelayDurations)
            {
                // Show complete scores through UI
                Console.WriteLine($"Time: {_timer}");
                Console.WriteLine($"Score: {_fruitsCollectedCount}/{_totalFruitCount}");
                Console.WriteLine($"Hits: {_hitCount}");
                _levelStatus = LevelStatus.Complete;
            }
            _levelCompleteTimer += deltaTime;
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
            _sceneTopLeft = data.Scene.WorldOffset;
            _sceneBottomRight = _sceneTopLeft + data.Scene.Size;
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

        private void OnLevelComplete(Event eventData, object? sender)
        {
            _levelStatus = LevelStatus.TransitionToComplete;
            _levelCompleteTimer = 0.0f;
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

        public class CreateInfo : CreateInfo<GameManager>
        {
            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                return new GameManager(info.Position);
            }
        }
    }
}