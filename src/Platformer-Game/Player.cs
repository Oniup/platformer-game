using System.Numerics;
using PlatformerGame.Engine.Event;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Utilities;
using PlatformerGame.Engine.Level.Collision;
using Raylib_cs;

namespace PlatformerGame
{
    public class Player : CharacterActor
    {
        private float _moveSpeed;
        private Vector2 _direction;
        private MainFramebuffer _renderTarget;

        private string[] _animNames;
        private int _currAnim;

        private Point _exitSceneTopLeftPt = Point.Zero;
        private Point _exitSceneBottomRightPt = Point.Zero;
        private bool _justExitedTheScene = false;

        public Player(SpriteAtlas sprite, AnimationSet animationSet, MainFramebuffer renderTarget, Vector2 position)
            : base(sprite, animationSet, CollisionLayer.Player, CollisionLayer.None, position)
        {
            _moveSpeed = 200.0f;
            _direction = Vector2.Zero;
            _renderTarget = renderTarget;

            _animNames = [
                "Double Jump",
                "Fall",
                "Hit",
                "Idle",
                "Jump",
                "Running",
                "Wall Slide",
            ];
            _currAnim = 0;

            PlayAnimation(_animNames[_currAnim]);

            DisabledCollisionDisplacement = false;
            AddCircleCollider(Vector2.UnitY * 6.0f, 9.0f);
            // AddBoxCollider(Vector2.Zero, 16, 16, false);

            EventDispatcher.AddListener<NewCurrentSceneEvent>(this, OnNewCurrentSceneEvent);
            World.ShowCollisionOutlines = true;
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            // Move
            _direction = Vector2.Zero;
            if (Raylib.IsKeyDown(KeyboardKey.A))
                _direction.X -= 1.0f;
            if (Raylib.IsKeyDown(KeyboardKey.D))
                _direction.X += 1.0f;
            if (Raylib.IsKeyDown(KeyboardKey.W))
                _direction.Y -= 1.0f;
            if (Raylib.IsKeyDown(KeyboardKey.S))
                _direction.Y += 1.0f;

            if (_direction != Vector2.Zero)
                Position += Vector2.Normalize(_direction) * _moveSpeed * deltaTime;

            // Switch animation
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                _currAnim = ++_currAnim % _animNames.Count();
                PlayAnimation(_animNames[_currAnim]);
            }

            // Set current scene to the new one that was just entered
            string? exitDir = ExitedScene();
            if (exitDir != null)
            {
                EventDispatcher.FireEvent(new SetNewCurrentSceneEvent(exitDir, SetNewCurrentSceneEvent.IdentifierType.NeighbouringDirection));
                _justExitedTheScene = true;
            }

#if DEBUG
            // Debug show collision outlines
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
                World.ShowCollisionOutlines = !World.ShowCollisionOutlines;
#endif
        }

        public override void OnLateUpdate(float deltaTime)
        {
            // TODO: Remove when implementing the camera controller
            _renderTarget.CameraPosition = new Vector2(
                Position.X - _renderTarget.FramebufferWidth / 2,
                Position.Y - _renderTarget.FramebufferHeight / 2
            );
        }

        private string? ExitedScene()
        {
            // Replace with collider
            Point topLeft = (Point)Position + new Point(4);
            Point botRight = (Point)Position + new Point(24, 29);

            string? dir = null;
            if (topLeft.Y < _exitSceneTopLeftPt.Y)
                dir = "n";
            if (botRight.Y > _exitSceneBottomRightPt.Y)
                dir = "s";
            if (botRight.X > _exitSceneBottomRightPt.X)
                dir = "e";
            if (topLeft.X < _exitSceneTopLeftPt.X)
                dir = "w";

            if (_justExitedTheScene)
            {
                if (dir != null)
                    dir = null;
                else
                    _justExitedTheScene = false;
            }
            return dir;
        }

        private void OnNewCurrentSceneEvent(IEvent evt, object? sender)
        {
            NewCurrentSceneEvent data = (NewCurrentSceneEvent)evt;

            _exitSceneTopLeftPt = new Point(data.Scene.WorldX, data.Scene.WorldY);
            _exitSceneBottomRightPt = _exitSceneTopLeftPt + new Point(data.Scene.Width, data.Scene.Height);
        }

        public class CreateInfo : CreateInfo<Player>
        {
            public override bool GlobalActor => true;

            public override void SetupRequiredResources(LDtkDefinition.Entity? def, ResourceManager resources)
            {
                SpriteAtlas sprite = resources.Get<SpriteAtlas>(def!.TilesetId);

                AnimationSet anims = new AnimationSet();
                anims.Add(sprite, "Double Jump", 0, 6);
                anims.Add(sprite, "Fall", 1, 1);
                anims.Add(sprite, "Hit", 2, 7);
                anims.Add(sprite, "Idle", 3, 11);
                anims.Add(sprite, "Jump", 4, 1);
                anims.Add(sprite, "Running", 5, 12);
                anims.Add(sprite, "Wall Slide", 6, 5);

                resources.Load("Player Animations", anims);
            }

            public override Actor Instantiate(ResourceManager resources, Scene? scene, LDtkDefinition.Entity? def, Vector2 position)
            {
                SpriteAtlas sprite = resources.Get<SpriteAtlas>(def!.TilesetId);
                AnimationSet animationSet = resources.Get<AnimationSet>("Player Animations");

                // TODO: Remove when implementing the camera controller
                MainFramebuffer renderTarget = resources.Get<MainFramebuffer>("Main Render Target");

                return new Player(sprite, animationSet, renderTarget, position);
            }
        }
    }
}