using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using Raylib_cs;

namespace PlatformerGame
{
    public class GameManager : Actor
    {
        private Player _player = null!;
        private Vector2 _sceneTopLeft;
        private Vector2 _sceneBottomRight;

        public GameManager(Vector2 position) : base(position)
        {
            EventDispatcher.AddListener<NewCurrentSceneEvent>(this, OnNewCurrentSceneEvent);
        }

        public override void OnAwake()
        {
            _player = World.Find<Player>().First();
            SetSceneBoundingBox(World.CurrentScene);
            Console.WriteLine("Found player and set the scene bounding box");
        }

        public override void OnUpdate(float deltaTime)
        {
            CheckPlayerExitScene();

#if DEBUG
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
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
            public override Actor Instantiate(ResourceManager resources, Scene? scene, LDtkDefinition.Entity? def, Vector2 position)
            {
                return new GameManager(position);
            }
        }
    }
}