using System.Numerics;
using PlatformerGame.Engine.Events;
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
        // Movement
        private readonly float _moveSpeed = 600.0f;
        private readonly float _groundedDrag = 12.0f;
        private readonly Vector2 _maxPxPerSecond = new Vector2(200.0f, 700.0f);
        private Vector2 _lastDirection;

        // Jump
        private readonly float _jumpImpulse = 3800.0f;
        private readonly float _jumpContinuedForce = 10000f;
        private readonly float[] _jumpDurations = [0.2f, 0.1f];
        private bool _jumpUseImpulseForce = true;
        private float _jumpTimer;
        private int _jumpCount;

        // Ground/Wall detection
        private bool _isOnGround;
        private float _gravityAmplifierWhenFalling = 1.5f;

        // Hack for fixing a crash when sometimes it freezes the entire program if gravity is calculated on the first frame
        private bool _enableGravity;

        private string[] _animNames;
        private int _currAnim;

        private MainFramebuffer _renderTarget;
        private Point _exitSceneTopLeftPt = Point.Zero;
        private Point _exitSceneBottomRightPt = Point.Zero;
        private bool _justExitedTheScene = false;

        public Player(SpriteAtlas sprite, AnimationSet animationSet, MainFramebuffer renderTarget, Vector2 position)
            : base(sprite, animationSet, CollisionLayer.Player, CollisionLayer.None, position)
        {
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

            PlayAnimation(_animNames[_currAnim]);

            DisabledCollisionDisplacement = false;
            AddCircleCollider(Vector2.UnitY * 6.0f, 9.0f);
            AddCircleCollider(Vector2.UnitY * 9.4f, 6f, OnGroundTrigger);

            EventDispatcher.AddListener<NewCurrentSceneEvent>(this, OnNewCurrentSceneEvent);
            EventDispatcher.AddListener<PlayerHitEvent>(this, OnPlayerHitEvent);
        }

        private int _NumberOfJumps
        {
            get { return _jumpDurations.Length; }
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            HandleMovement(deltaTime);

            // Switch animation
            if (Raylib.IsKeyPressed(KeyboardKey.K))
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

            _isOnGround = false;
            _enableGravity = true;
        }

        public override void OnLateUpdate(float deltaTime)
        {
            // TODO: Remove when implementing the camera controller
            // _renderTarget.CameraPosition = new Vector2(
            //     Position.X - _renderTarget.FramebufferWidth / 2,
            //     Position.Y - _renderTarget.FramebufferHeight / 2
            // );
        }

        private Vector2 GetInputDirection(out bool jumpPressed)
        {
            Vector2 direction = Vector2.Zero;
            if (Raylib.IsKeyDown(KeyboardKey.A))
                direction.X -= 1.0f;
            if (Raylib.IsKeyDown(KeyboardKey.D))
                direction.X += 1.0f;

            jumpPressed = Raylib.IsKeyDown(KeyboardKey.Space);

            return direction;
        }

        private void HandleMovement(float deltaTime)
        {
            Vector2 moveDirection = GetInputDirection(out bool jumpPressed);
            HandleJumping(deltaTime, jumpPressed);

            if (!_isOnGround)
            {
                float gravityAmplifier = Velocity.Y > 0.0f ? _gravityAmplifierWhenFalling : 1.0f;
                if (_enableGravity)
                    ApplyForce += Vector2.UnitY * (World.GravityScale * gravityAmplifier * Mass);

                if (!jumpPressed && _jumpCount == 0)
                    _jumpCount++;
            }
            else
            {
                Velocity = new Vector2(Velocity.X, 0.0f);
                _jumpCount = 0;
            }

            if (moveDirection != Vector2.Zero)
            {
                if (_lastDirection.X != moveDirection.X && moveDirection.X != 0.0f)
                    Velocity = new Vector2(0.0f, Velocity.Y);
                Velocity += Vector2.UnitX * moveDirection.X * _moveSpeed * deltaTime;
            }
            else
                Velocity -= new Vector2(Velocity.X * _groundedDrag * deltaTime, 0.0f);

            Velocity = Vector2.Clamp(Velocity, -_maxPxPerSecond, _maxPxPerSecond); // Cap speeds
            ApplyMovementForces(deltaTime);

            _lastDirection = moveDirection;
        }

        private void HandleJumping(float deltaTime, bool jumpPressed)
        {
            if (jumpPressed && _jumpCount < _NumberOfJumps &&  _jumpTimer < _jumpDurations[_jumpCount])
            {
                if (_jumpUseImpulseForce)
                {
                    Velocity = new Vector2(Velocity.X, 0.0f);
                    ImpulseForce -= Vector2.UnitY * _jumpImpulse;
                    _jumpUseImpulseForce = false;
                }
                else
                    ApplyForce -= Vector2.UnitY * _jumpContinuedForce;

                _isOnGround = false;
                _jumpTimer += deltaTime;
            }
            else if (!jumpPressed && !_jumpUseImpulseForce)
            {
                _jumpUseImpulseForce = true;
                _jumpCount++;
                _jumpTimer = 0.0f;
            }
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

        private void OnGroundTrigger(CollidableActor actor, ShapeCollider collider)
        {
            if ((actor.CollisionLayer & CollisionLayer.Ground) != 0)
                _isOnGround = true;
        }

        private void OnNewCurrentSceneEvent(Event evt, object? sender)
        {
            NewCurrentSceneEvent newSceneEvent = (NewCurrentSceneEvent)evt;

            _exitSceneTopLeftPt = new Point(newSceneEvent.Scene.WorldX, newSceneEvent.Scene.WorldY);
            _exitSceneBottomRightPt = _exitSceneTopLeftPt + new Point(newSceneEvent.Scene.Width, newSceneEvent.Scene.Height);
        }

        private void OnPlayerHitEvent(Event evt, object? sender)
        {
            PlayAnimation("Hit");

            List<RespawnPosition> pos = World.Find<RespawnPosition>(World.CurrentScene, 1);
            if (pos.Count == 0)
                throw new NullReferenceException("Must define a respawn position in each scene");
            Position = pos.First().Position;
        }

        public class CreateInfo : CreateInfo<Player>
        {
            public override bool GlobalActor => true;

            public override void SetupRequiredResources(LDtkDefinition.Entity? def, ResourceManager resources)
            {
                SpriteAtlas sprite = resources.Get<SpriteAtlas>((int)def!.TilesetId!);

                AnimationSet anims = new AnimationSet();
                anims.Add(sprite, "Double Jump", 0, 6);
                anims.Add(sprite, "Fall", 1, 1);
                anims.Add(sprite, "Hit", 2, 7, true);
                anims.Add(sprite, "Idle", 3, 11);
                anims.Add(sprite, "Jump", 4, 1);
                anims.Add(sprite, "Running", 5, 12);
                anims.Add(sprite, "Wall Slide", 6, 5);

                resources.Load("Player Animations", anims);
            }

            public override Actor Instantiate(ResourceManager resources, Scene? scene, LDtkDefinition.Entity? def, Vector2 position)
            {
                SpriteAtlas sprite = resources.Get<SpriteAtlas>((int)def!.TilesetId!);
                AnimationSet animationSet = resources.Get<AnimationSet>("Player Animations");

                // TODO: Remove when implementing the camera controller
                MainFramebuffer renderTarget = resources.Get<MainFramebuffer>("Main Render Target");

                return new Player(sprite, animationSet, renderTarget, position);
            }
        }
    }
}