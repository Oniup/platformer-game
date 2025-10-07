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
        private float _lastInputDirection;

        // Jump
        private readonly float _jumpImpulse = 3000.0f;
        private readonly float _jumpContinuedForce = 20500f;
        private readonly float[] _jumpDurations = [0.1f, 0.05f];
        private readonly float _coyoteTimerDuration = 0.08f; // Duration that Celeste uses 
        private bool _jumpUseImpulseForce = true;
        private float _jumpTimer;
        private float _coyoteTimer;
        private int _jumpCount;

        // Ground/Wall detection
        private bool _isOnGround;
        private bool _isTouchingWall;
        private CircleCollider _wallGrabCollider;

        // Hack for fixing a crash when sometimes it freezes the entire program if gravity is calculated on the first frame
        private bool _enableGravity;

        private MainFramebuffer _renderTarget;
        private Point _exitSceneTopLeftPt = Point.Zero;
        private Point _exitSceneBottomRightPt = Point.Zero;
        private bool _justExitedTheScene = false;

        public Player(SpriteAtlas sprite, AnimationSet animationSet, MainFramebuffer renderTarget, Vector2 position)
            : base(sprite, animationSet, CollisionLayer.Player, CollisionLayer.None, position)
        {
            _renderTarget = renderTarget;

            DisabledCollisionDisplacement = false;

            // Setting up collision shapes
            AddCircleCollider(Vector2.UnitY * 6.0f, 9.0f);
            AddCircleCollider(Vector2.UnitY * 9.4f, 6.0f, OnGroundTrigger);
            _wallGrabCollider = AddCircleCollider(Vector2.UnitY * 6.4f, 6.0f, OnTouchingWallTrigger);

            // Setting up listener for events
            EventDispatcher.AddListener<NewCurrentSceneEvent>(this, OnNewCurrentSceneEvent);
            EventDispatcher.AddListener<PlayerHitEvent>(this, OnPlayerHitEvent);

            MaxVelocityCap = new Vector2(200.0f, 700.0f);
        }

        private int _NumberOfJumps
        {
            get { return _jumpDurations.Length; }
        }

        public override void OnUpdate(float deltaTime)
        {
            float inputDirection = GetInputDirection(out bool jumpPressed);
            _wallGrabCollider.Offset = new Vector2(inputDirection * 5, _wallGrabCollider.Offset.Y);

            base.OnUpdate(deltaTime); // Collisions and animations handle
            HandleMovement(inputDirection, jumpPressed, deltaTime);

            if (_isTouchingWall)
                Console.WriteLine("Is touching wall");

            HandleAnimations(inputDirection);

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
            _isTouchingWall = false;
            _enableGravity = true;
        }

        public override void OnDestroy()
        {
            EventDispatcher.RemoveListener<NewCurrentSceneEvent>(this);
            EventDispatcher.RemoveListener<PlayerHitEvent>(this);
        }

        public override void OnLateUpdate(float deltaTime)
        {
            // TODO: Remove when implementing the camera controller
            // _renderTarget.CameraPosition = new Vector2(
            //     Position.X - _renderTarget.FramebufferWidth / 2,
            //     Position.Y - _renderTarget.FramebufferHeight / 2
            // );
        }

        private float GetInputDirection(out bool jumpPressed)
        {
            float direction = 0.0f;
            if (Raylib.IsKeyDown(KeyboardKey.A))
                direction -= 1.0f;
            if (Raylib.IsKeyDown(KeyboardKey.D))
                direction += 1.0f;

            jumpPressed = Raylib.IsKeyDown(KeyboardKey.Space);

            return direction;
        }

        private void HandleMovement(float inputDirection, bool jumpPressed, float deltaTime)
        {
            HandleJumping(jumpPressed, deltaTime);
            if (_enableGravity)
                ApplyGravityForce();

            HandleHorizontalMovement(inputDirection, deltaTime);

            ApplyForcesBody(deltaTime);
            _lastInputDirection = inputDirection;
        }

        private void HandleHorizontalMovement(float inputDirection, float deltaTime)
        {
            if (_isOnGround)
                Velocity = new Vector2(Velocity.X, 0.0f);

            if (inputDirection != 0.0f)
            {
                if (_lastInputDirection != inputDirection && inputDirection != 0.0f)
                    Velocity = new Vector2(0.0f, Velocity.Y);
                Velocity += Vector2.UnitX * inputDirection * _moveSpeed * deltaTime;
            }
            else
                Velocity -= new Vector2(Velocity.X * _groundedDrag * deltaTime, 0.0f);
        }

        private void HandleJumping(bool jumpPressed, float deltaTime)
        {
            if (jumpPressed && _jumpCount < _NumberOfJumps && _jumpTimer < _jumpDurations[_jumpCount])
            {
                ApplyJumpForces(deltaTime);
            }
            else if (!jumpPressed && !_jumpUseImpulseForce)
            {
                _jumpUseImpulseForce = true;
                _jumpCount++;
                _jumpTimer = 0.0f;
            }

            CoyoteTimeLeniency(jumpPressed, deltaTime);
            if (_isOnGround)
            {
                _jumpCount = 0;
                _coyoteTimer = 0.0f;
            }
        }

        private void ApplyJumpForces(float deltaTime)
        {
            if (_jumpUseImpulseForce)
            {
                Velocity = new Vector2(Velocity.X, 0.0f);
                ImpulseForce -= Vector2.UnitY * _jumpImpulse;
                _jumpUseImpulseForce = false;

                if (_jumpCount != 0)
                    PlayAnimation("Double Jump");
            }
            else
                ApplyForce -= Vector2.UnitY * _jumpContinuedForce;

            _isOnGround = false;
            _jumpTimer += deltaTime;
        }

        private void CoyoteTimeLeniency(bool jumpPressed, float deltaTime)
        {
            if (!_isOnGround)
            {
                if (_jumpCount == 0 && _coyoteTimer > _coyoteTimerDuration && !jumpPressed)
                    _jumpCount++;
                _coyoteTimer += deltaTime;
            }
        }

        private void HandleAnimations(float inputMoveDirection)
        {
            if (_isOnGround)
            {
                if (inputMoveDirection == 0.0f)
                    PlayAnimation("Idle");
                else
                    PlayAnimation("Running");
            }
            else
            {
                if (Velocity.Y > 0.0f)
                    PlayAnimation("Fall");
                else
                    PlayAnimation("Jump");
            }
            if (inputMoveDirection != 0.0f)
                FlipX = inputMoveDirection > 0.0f ? false : true;
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

        private void OnTouchingWallTrigger(CollidableActor actor, ShapeCollider collider)
        {
            if ((actor.CollisionLayer & CollisionLayer.Ground) != 0)
                _isTouchingWall = true;
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
                anims.Add(sprite, "Double Jump", 0, 6, AnimationMode.UninterruptableUntilComplete, "Idle");
                anims.Add(sprite, "Fall", 1, 1);
                anims.Add(sprite, "Hit", 2, 7, AnimationMode.UninterruptableUntilComplete,"Idle");
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