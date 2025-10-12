using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Utilities;
using Raylib_cs;

namespace PlatformerGame
{
    public class Player : CharacterActor
    {
        // Movement
        private readonly float _moveSpeed = 600.0f;
        private readonly float _groundedDrag = 12.0f;
        private float _lastInputDirection = 0.0f;

        // Jump
        private readonly float _jumpImpulse = 3000.0f;
        private readonly float _jumpContinuedForce = 20500f;
        private readonly float[] _jumpDurations = [0.1f, 0.05f];
        private readonly float _coyoteTimerDuration = 0.08f; // Duration that Celeste uses 
        private bool _jumpUseImpulseForce = true;
        private float _jumpTimer = 0.0f;
        private float _coyoteTimer = 0.0f;
        private int _jumpCount = 0;

        // Wall slide
        private readonly float _wallJumpOffsetImpulse = 2000.0f;
        private readonly float _wallSlideGravityAmplifer = 0.1f;

        // Conditions
        private bool _isOnGround = false;
        private bool _isTouchingWall = false;
        private bool _prevIsTouchingWall = false;
        private bool _isInHitState = false;
        private CircleCollider _wallSlideCollider;

        // Hack for fixing a crash when sometimes it freezes the entire program if gravity is calculated on the first frame
        private bool _enableGravity = false;

        private int NumberOfJumps => _jumpDurations.Length;
        private bool IsWallSliding => !_isOnGround && _isTouchingWall;

        public Player(SpriteAtlas sprite, AnimationSet animationSet, Vector2 position)
            : base(sprite, animationSet, CollisionLayer.Player, CollisionLayer.None, position)
        {
            DisabledCollisionDisplacement = false;

            // Setting up collision shapes
            AddCircleCollider(Vector2.UnitY * 7.0f, 8.0f);
            AddCircleCollider(Vector2.UnitY * 9.4f, 6.0f, OnGroundTrigger);
            _wallSlideCollider = AddCircleCollider(Vector2.UnitY * 6.4f, 3.0f, OnTouchingWallTrigger);

            // Setting up listener for events
            EventDispatcher.AddListener<PlayerHitEvent>(this, OnPlayerHitEvent);

            MaxVelocityCap = new Vector2(200.0f, 500.0f);
        }

        public override void OnAwake()
        {
            Position = GetRespawnPointPosition();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (!_isInHitState)
                MovementController(deltaTime);
            else
                HitState(deltaTime);

            UpdateAnimation(deltaTime);

            _prevIsTouchingWall = _isTouchingWall;
            _isOnGround = false;
            _isTouchingWall = false;
            _enableGravity = true;
        }

        public override void OnDispose()
        {
            EventDispatcher.RemoveListener<PlayerHitEvent>(this);
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

        private void HitState(float deltaTime)
        {
            // Wait until the hit animation has completed before teleporting to respawn position
            if (AnimationPaused)
            {
                Position = GetRespawnPointPosition();
                ResetAllForces();

                _isInHitState = false;
                DisabledCollision = false;
                ResumeAnimation();
            }

            ApplyGravityForce();
            ApplyForcesBody(deltaTime);
        }

        private void MovementController(float deltaTime)
        {
            float inputDirection = GetInputDirection(out bool jumpPressed);
            _wallSlideCollider.Offset = new Vector2(inputDirection * 6f, _wallSlideCollider.Offset.Y);

            CalculateCollisions();
            HandleMovement(inputDirection, jumpPressed, deltaTime);
            HandleAnimations(inputDirection);
        }

        private void HandleMovement(float inputDirection, bool jumpPressed, float deltaTime)
        {
            HandleJumping(inputDirection, jumpPressed, deltaTime);
            HandleGravity();
            HandleHorizontalMovement(inputDirection, deltaTime);

            ApplyForcesBody(deltaTime);
            _lastInputDirection = inputDirection;
        }

        public void HandleGravity()
        {
            if (_enableGravity && !_isOnGround)
            {
                if (IsWallSliding)
                {
                    if (_prevIsTouchingWall == false)
                        Velocity = new Vector2(Velocity.X, Velocity.Y * 0.4f);
                    ApplyGravityForce(_wallSlideGravityAmplifer);
                }
                else
                    ApplyGravityForce();
            }
        }

        private void HandleHorizontalMovement(float inputDirection, float deltaTime)
        {
            // if (_isOnGround)
            //     Velocity = new Vector2(Velocity.X, 0.0f);

            if (inputDirection != 0.0f && !IsWallSliding)
            {
                if (_lastInputDirection != inputDirection && inputDirection != 0.0f)
                    Velocity = new Vector2(0.0f, Velocity.Y);
                Velocity += Vector2.UnitX * inputDirection * _moveSpeed * deltaTime;
            }
            else
                Velocity -= new Vector2(Velocity.X * _groundedDrag * deltaTime, 0.0f);
        }

        private void HandleJumping(float inputDirection, bool jumpPressed, float deltaTime)
        {
            if (jumpPressed && _jumpCount < NumberOfJumps && _jumpTimer < _jumpDurations[_jumpCount])
            {
                ApplyJumpForces(inputDirection, deltaTime);
            }
            else if (!jumpPressed && !_jumpUseImpulseForce)
            {
                _jumpUseImpulseForce = true;
                _jumpCount++;
                _jumpTimer = 0.0f;
            }

            CoyoteTimeLeniency(jumpPressed, deltaTime);
            if (_isOnGround || IsWallSliding)
            {
                _jumpCount = 0;
                _coyoteTimer = 0.0f;
            }
        }

        private void ApplyJumpForces(float inputDirection, float deltaTime)
        {
            if (_jumpUseImpulseForce)
            {
                Velocity = new Vector2(Velocity.X, 0.0f);
                ApplyImpulse -= Vector2.UnitY * _jumpImpulse;
                _jumpUseImpulseForce = false;

                if (IsWallSliding)
                    ApplyImpulse += Vector2.UnitX * -inputDirection * _wallJumpOffsetImpulse;

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
            if (!_isOnGround || !IsWallSliding)
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
                if (_isTouchingWall)
                    PlayAnimation("Wall Slide");
                else
                {
                    if (Velocity.Y > 0.0f)
                        PlayAnimation("Fall");
                    else
                        PlayAnimation("Jump");
                }
            }
            if (inputMoveDirection != 0.0f)
                FlipX = inputMoveDirection <= 0.0f;
        }

        private void OnGroundTrigger(CollidableActor actor, ShapeCollider collider)
        {
            if ((actor.CollisionLayer & CollisionLayer.Ground) != 0)
            {
                if ((actor.CollisionLayer & CollisionLayer.Platform) != 0)
                {
                    PlatformTilemapLayer tilemap = (PlatformTilemapLayer)actor;
                    if (!tilemap.IsRegistered(this) && Velocity.Y >= 0.0f)
                        _isOnGround = true;
                    else
                        _isOnGround = false;
                }
                else
                    _isOnGround = true;
            }
        }

        private void OnTouchingWallTrigger(CollidableActor actor, ShapeCollider collider)
        {
            if ((actor.CollisionLayer & CollisionLayer.Ground) != 0)
            {
                if ((actor.CollisionLayer & CollisionLayer.Platform) != 0)
                {
                    // Skip platform tilemap if already inside
                    PlatformTilemapLayer tilemap = (PlatformTilemapLayer)actor;
                    if (tilemap.IsRegistered(this))
                        return;
                }
                _isTouchingWall = true;
            }
        }

        private void OnPlayerHitEvent(Event _, object? sender)
        {
            PlayAnimation("Hit");
            _isInHitState = true;
            DisabledCollision = true;

            // Bounce upwards on hit
            ResetForces();
            Velocity = new Vector2(Velocity.X, -_jumpImpulse * 0.05f);

            World.Instantiate<RespawnEffect>(GetRespawnPointPosition());
        }

        private Vector2 GetRespawnPointPosition()
        {
            List<RespawnPosition> respawnPosition = World.Find<RespawnPosition>(World.CurrentScene, 1);
            if (respawnPosition.Count == 0)
                throw new NullReferenceException("Must define a respawn position in each scene");
            return respawnPosition.First().Position;
        }

        public class CreateInfo : CreateInfo<Player>
        {
            public override bool GlobalActor => true;

            public override void SetupRequiredResources(LDtkDefinition.Entity? def, ResourceManager resources)
            {
                string path = resources.AssetDirectory + "/Graphics/Player/";
                SpriteAtlas sprite = null!;
                for (int i = 0; i < 4; i++)
                {
                    string name = "Player" + (i + 1);
                    sprite = new SpriteAtlas(32, path + name + ".png");
                    resources.Load(name, sprite);
                }

                AnimationSet anims = new AnimationSet();
                anims.Add(sprite, "Double Jump", 0, 6, AnimationMode.UninterruptableUntilComplete, "Idle");
                anims.Add(sprite, "Fall", 1, 1);
                anims.Add(sprite, "Hit", 2, 7, AnimationMode.PauseOnComplete, "Idle");
                anims.Add(sprite, "Idle", 3, 11);
                anims.Add(sprite, "Jump", 4, 1);
                anims.Add(sprite, "Running", 5, 12);
                anims.Add(sprite, "Wall Slide", 6, 5);

                resources.Load("Player Animations", anims);
            }

            public override Actor Instantiate(ResourceManager resources, Scene? scene, LDtkDefinition.Entity? def, Vector2 position)
            {
                // TODO: Let user decide what player skin they want to use
                SpriteAtlas sprite = resources.Get<SpriteAtlas>("Player1");
                AnimationSet animationSet = resources.Get<AnimationSet>("Player Animations");

                return new Player(sprite, animationSet, position);
            }
        }
    }
}