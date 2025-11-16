using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Utilities;
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
        private LevelCompleteCanvas _levelCompleteCanvas = null!;

        // Scene bounding box for checking player exiting scene
        private Vector2 _sceneTopLeft;
        private Vector2 _sceneBottomRight;

        // Run metrics
        private int _fruitsCollectedCount;
        private int _hitCount;
        private float _timer = 0.0f;

        // On level complete
        private DeltaTimer _levelCompleteTimer = new DeltaTimer(1.0f);
        private LevelStatus _levelStatus = LevelStatus.NotComplete;

        public GameManager(Vector2 position)
            : base(position)
        {
            EventDispatcher.AddListener<NewCurrentSceneEvent>(this, OnNewCurrentSceneEvent);
            EventDispatcher.AddListener<AddScoreEvent>(this, OnAddingScoreEvent);
            EventDispatcher.AddListener<PlayerHitEvent>(this, OnPlayerHitEvent);
            EventDispatcher.AddListener<LevelComplete>(this, OnLevelComplete);
        }

        public override void OnDispose()
        {
            EventDispatcher.RemoveListener<NewCurrentSceneEvent>(this);
            EventDispatcher.RemoveListener<AddScoreEvent>(this);
            EventDispatcher.RemoveListener<PlayerHitEvent>(this);
            EventDispatcher.RemoveListener<LevelComplete>(this);
        }

        public override void OnAwake(Scene? scene)
        {
            _player = World.Find<Player>().First();

            // Canvases
            _pauseCanvas = World.Find<PauseCanvas>().First();
            _runtimeCanvas = World.Find<RuntimeCanvas>().First();
            _levelCompleteCanvas = World.Find<LevelCompleteCanvas>().First();
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
            if (_levelCompleteTimer.Finished)
            {
                _levelStatus = LevelStatus.Complete;
                _runtimeCanvas.Showing = false;
                _levelCompleteCanvas.Showing = true;

                // Submit score
                SaveData saveData = SaveData.Read();
                SaveData.LevelScore score = saveData.GetLevelScore(World.LevelName);
                var run = new SaveData.LevelScore.Run
                {
                    Score = _fruitsCollectedCount,
                    Time = _timer,
                    Hits = _hitCount,
                };
                if (score.SetBestRun(run))
                    SaveData.Write(saveData);

                // Show score through level complete UI
                _levelCompleteCanvas.RegisterRun(_fruitsCollectedCount, score.TotalRequiredScore, _timer, score.Required3StarTime, _hitCount, score.GetRunScoreRatio(run));
            }
            _levelCompleteTimer.Update(deltaTime);
        }

        private void CheckPlayerExitScene()
        {
            if (_player.IsInHitState)
                return;

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
                    exitDir, SetNewCurrentSceneEvent.IdentifierType.NeighboringDirection), this);
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
            _levelCompleteTimer.Start();
        }

        private void ProcessInputs(out bool pausedPressed)
        {
            pausedPressed = Raylib.IsKeyPressed(KeyboardKey.Escape);
            if (!pausedPressed && Raylib.IsGamepadAvailable(0))
                pausedPressed = Raylib.IsGamepadButtonPressed(0, GamepadButton.MiddleRight);
        }

        public class CreateInfo : CreateInfo<GameManager>
        {
            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                return new GameManager(info.Position);
            }
        }
    }
}