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
        private readonly float _jumpContinuedForce = 20500.0f;
        private readonly float[] _jumpDurations = [0.1f, 0.05f];
        private readonly float _coyoteTimerDuration = 0.1f;
        private bool _jumpUseImpulseForce = true;
        private float _jumpRequiredHangVelocity = 50f;
        private float _jumpHangGravityMultiplier = 0.8f;
        private float _jumpTimer = 0.0f;
        private float _coyoteTimer = 0.0f;
        private int _jumpCount = 0;

        // Wall slide
        private readonly float _wallJumpOffsetImpulse = 2000.0f;
        private readonly float _wallColliderOffset = 6.0f;
        private readonly float _wallSlideGravityAmplifier = 0.1f;
        private readonly float _wallJumpCoyoteTimeDuration = 0.2f;
        private float _wallSlideJumpXDirection;
        private float _wallJumpCoyoteTimer = 0.0f;

        // Conditions
        private bool _isOnGround;
        private bool _notMoving = true;
        private bool _isTouchingWall;
        private bool _prevIsTouchingWall;
        private bool _prevIsOnGround;
        private bool _isInHitState;
        private CircleCollider _wallSlideCollider;

        private SoundEffect[] _sounds;

        // Hack for fixing a crash when sometimes it freezes the entire program if gravity is calculated on the first frame
        private bool _enableGravity;

        // Level complete animation
        private readonly float _levelCompleteDurationBeforeDestroy = 0.3f;
        private bool _isLevelComplete;
        private float _levelCompleteTimer;


        private int NumberOfJumps => _jumpDurations.Length;
        private bool IsWallSliding => !_isOnGround && _isTouchingWall;
        private bool CanWallJump => !_isOnGround && _wallJumpCoyoteTimer > 0.0f;

        public Player(SpriteAtlas sprite, AnimationSet animationSet, SoundEffect[] sounds, Vector2 position)
            : base(sprite, animationSet, CollisionLayer.Player, CollisionLayer.None, position)
        {
            DisabledCollisionDisplacement = false;

            // Setting up collision shapes
            AddCircleCollider(Vector2.UnitY * 7.0f, 8.0f);
            AddCircleCollider(Vector2.UnitY * 9.4f, 6.0f, OnGroundTrigger);
            _wallSlideCollider = AddCircleCollider(Vector2.UnitY * 6.4f, 3.0f, OnTouchingWallTrigger);

            // Setting up listener for events
            EventDispatcher.AddListener<PlayerHitEvent>(this, OnPlayerHitEvent);
            EventDispatcher.AddListener<LevelComplete>(this, OnLevelComplete);

            _jumpCount = _jumpDurations.Length;
            _sounds = sounds;
        }

        public bool IsInHitState => _isInHitState;

        public override void OnDispose()
        {
            EventDispatcher.RemoveListener<PlayerHitEvent>(this);
            EventDispatcher.RemoveListener<LevelComplete>(this);
        }

        public override void OnAwake(Scene? scene)
        {
            Position = GetRespawnPointPosition();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (World.Paused)
                return;

            if (!_isInHitState)
                MovementController(deltaTime);
            else
                HitState(deltaTime);
            UpdateAnimation(deltaTime);

            foreach (SoundEffect sound in _sounds)
                sound.UpdateTimer(deltaTime);

            _prevIsTouchingWall = _isTouchingWall;
            _prevIsOnGround = _isOnGround;
            _isOnGround = false;
            _isTouchingWall = false;
            _enableGravity = true;
            if (_isLevelComplete)
            {
                if (_levelCompleteTimer > _levelCompleteDurationBeforeDestroy)
                    Destroy = true;
                _levelCompleteTimer += deltaTime;
            }
        }

        public override void OnDestroy()
        {
            RespawnEffect actor = World.Instantiate<RespawnEffect>(Position);
            actor.SetToDisappear();
        }

        public void ResetDoubleJump()
        {
            if (_jumpCount > 0)
                _jumpCount = 1;
        }

        private float GetInputDirection(out bool jumpPressed)
        {
            // Disable controls on level complete
            if (_isLevelComplete)
            {
                jumpPressed = false;
                return 0.0f;
            }

            float direction = 0.0f;
            if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))
                direction -= 1.0f;
            if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right))
                direction += 1.0f;

            int gamepadId = 0;
            float gamepadDirection = 0.0f;
            bool gamepadJumpPressed = false;
            if (Raylib.IsGamepadAvailable(gamepadId))
            {
                float deadZone = 0.5f;
                gamepadDirection = Raylib.GetGamepadAxisMovement(gamepadId, GamepadAxis.LeftX);
                if (gamepadDirection > deadZone)
                    gamepadDirection = 1.0f;
                else if (gamepadDirection < -deadZone)
                    gamepadDirection = -1.0f;
                else
                    gamepadDirection = 0.0f;
                gamepadJumpPressed = Raylib.IsGamepadButtonDown(gamepadId, GamepadButton.RightFaceDown);
            }

            direction = gamepadDirection != 0.0f ? gamepadDirection : direction;
            jumpPressed = gamepadJumpPressed ? gamepadJumpPressed : Raylib.IsKeyDown(KeyboardKey.Space) || Raylib.IsKeyDown(KeyboardKey.Up) || Raylib.IsKeyDown(KeyboardKey.W);
            return Math.Clamp(direction, -1.0f, 1.0f);
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
            ApplyForcesToBody(deltaTime);
        }

        private void MovementController(float deltaTime)
        {
            float inputDirection = GetInputDirection(out bool jumpPressed);
            float wallOffset = inputDirection * _wallColliderOffset;
            _wallSlideCollider.Offset = new Vector2(inputDirection * _wallColliderOffset, _wallSlideCollider.Offset.Y);

            HandleMovement(inputDirection, jumpPressed, deltaTime);
            HandleAnimations(inputDirection);
        }

        private void HandleMovement(float inputDirection, bool jumpPressed, float deltaTime)
        {
            HandleJumping(jumpPressed, deltaTime);
            HandleGravity();
            HandleHorizontalMovement(inputDirection, deltaTime);

            ApplyForcesToBody(deltaTime);

            _lastInputDirection = inputDirection;
            _wallJumpCoyoteTimer -= deltaTime;
        }

        private void HandleGravity()
        {
            if (!_enableGravity || _isOnGround)
                return;

            if (IsWallSliding)
            {
                if (_prevIsTouchingWall == false)
                    Velocity = new Vector2(Velocity.X, Velocity.Y * 0.4f);
                ApplyGravityForce(_wallSlideGravityAmplifier);
                return;
            }

            float gravityMultiplier = DefaultGravityFallMultiplier;
            if (MathF.Abs(Velocity.Y) < _jumpRequiredHangVelocity)
                gravityMultiplier = _jumpHangGravityMultiplier;

            ApplyGravityForce(gravityMultiplier);
        }

        private void HandleHorizontalMovement(float inputDirection, float deltaTime)
        {
            if (inputDirection != 0.0f && !IsWallSliding)
            {
                float moveSpeed = _moveSpeed;

                if (_lastInputDirection != inputDirection && inputDirection != 0.0f && !_notMoving)
                    Velocity = new Vector2(0.0f, Velocity.Y);

                Velocity += Vector2.UnitX * (inputDirection * moveSpeed * deltaTime);
                _notMoving = false;
            }
            else if (Velocity.X != 0.0f)
            {
                if (MathF.Abs(Velocity.X) > 10f)
                    Velocity -= new Vector2(Velocity.X * _groundedDrag * deltaTime, 0.0f);
                else
                {
                    Velocity = new Vector2(0.0f, Velocity.Y);
                    _notMoving = true;
                }
            }

        }

        private void HandleJumping(bool jumpPressed, float deltaTime)
        {
            if (jumpPressed && _jumpCount < NumberOfJumps && _jumpTimer < _jumpDurations[_jumpCount])
            {
                ApplyJumpForces(deltaTime);
                // _isOnGround = false;
                // _prevIsOnGround = true;
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

        private void ApplyJumpForces(float deltaTime)
        {
            // Begin jump
            if (_jumpUseImpulseForce)
            {
                Velocity = new Vector2(Velocity.X, 0.0f);
                ApplyImpulse -= Vector2.UnitY * _jumpImpulse;
                _jumpUseImpulseForce = false;

                if (CanWallJump)
                    ApplyImpulse += Vector2.UnitX * (_wallSlideJumpXDirection * _wallJumpOffsetImpulse);

                if (_jumpCount != 0)
                    PlayAnimation("Double Jump");
            }
            else // Variable jump height
                ApplyForce -= Vector2.UnitY * _jumpContinuedForce;

            // Variable jump height duration before force stops being applied
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


        protected override void ApplyCollisionDisplacement(CollidableActor collidable, Vector2 displacement)
        {
            // Skip stopping velocity if moving upwards and is a platform
            if (collidable.CollisionLayer.HasFlag(CollisionLayer.Platform))
            {
                var tilemap = (PlatformTilemapLayer)collidable;
                if (tilemap.IsRegistered(this) || Velocity.Y < 0.0f)
                    return;
            }

            base.ApplyCollisionDisplacement(collidable, displacement);
        }  

        private void OnGroundTrigger(CollidableActor actor, ShapeCollider collider)
        {
            if (!collider.IsTrigger && actor.CollisionLayer.HasFlag(CollisionLayer.Ground))
            {
                if ((actor.CollisionLayer & CollisionLayer.Platform) != 0)
                {
                    var tilemap = (PlatformTilemapLayer)actor;
                    if (!tilemap.IsRegistered(this) && Velocity.Y >= 0.0f)
                    {
                        // For some reason I have to have this condition like this, otherwise the player just floats up
                        // through the platform and I don't know why.
                    }
                    else
                    {
                        _isOnGround = false;
                        return;
                    }
                }

                if (!_prevIsOnGround)
                {
                    if (Velocity.Y > MaxVelocityCap.Y - 10)
                        _sounds[(int)SfxIndex.HardLanding].Play();
                    else
                        _sounds[(int)SfxIndex.Landing].Play();
                }
                _isOnGround = true;
            }
        }

        private void OnTouchingWallTrigger(CollidableActor actor, ShapeCollider collider)
        {
            if (!collider.IsTrigger && actor.CollisionLayer.HasFlag(CollisionLayer.Ground))
            {
                // Cannot grab onto platforms
                if (actor.CollisionLayer.HasFlag(CollisionLayer.Platform))
                    return;

                _wallJumpCoyoteTimer = _wallJumpCoyoteTimeDuration;
                _wallSlideJumpXDirection = -Math.Clamp(_wallSlideCollider.Offset.X, -1.0f, 1.0f);

                if (!_prevIsTouchingWall)
                    _sounds[(int)SfxIndex.Landing].Play();
                _isTouchingWall = true;
            }
        }

        private void OnPlayerHitEvent(Event _, object? sender)
        {
            PlayAnimation("Hit");
            _sounds[(int)SfxIndex.Hit].Play();
            _isInHitState = true;
            DisabledCollision = true;

            // Bounce upwards on hit
            ResetForces();
            Velocity = new Vector2(Velocity.X, -_jumpImpulse * 0.05f);

            World.Instantiate<RespawnEffect>(GetRespawnPointPosition());
        }

        private void OnLevelComplete(Event eventData, object? sender)
        {
            _isLevelComplete = true;
        }

        private Vector2 GetRespawnPointPosition()
        {
            List<RespawnPosition> respawnPosition = World.Find<RespawnPosition>(World.CurrentScene, 1);
            if (respawnPosition.Count == 0)
                throw new NullReferenceException("Must define a respawn position in each scene");
            return respawnPosition.First().Position;
        }

        private enum SfxIndex : int
        {
            Hit = 0,
            Landing,
            HardLanding,
        }

        public class CreateInfo : CreateInfo<Player>
        {
            public override bool GlobalActor => true;

            public override void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def)
            {
                SpriteAtlas atlas = SetupSpriteAtlases(resources);
                SetupAnimations(resources, atlas);
                SetupSoundEffects(resources);
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                // TODO: Let user decide what player skin they want to use
                SaveData data = SaveData.Read();
                var sprite = resources.Get<SpriteAtlas>(data.SelectedSkin);
                var animationSet = resources.Get<AnimationSet>("Player Animations");

                SoundEffect[] soundEffects = [
                    resources.Get<SoundEffect>("Player Hit"),
                    resources.Get<SoundEffect>("Player Landing"),
                    resources.Get<SoundEffect>("Player Hard Landing"),
                ];

                return new Player(sprite, animationSet, soundEffects, info.Position);
            }

            private static void SetupAnimations(ResourceRegistry resources, SpriteAtlas atlas)
            {
                var anims = new AnimationSet();
                anims.Add(atlas, "Fall", 1, 1);
                anims.Add(atlas, "Idle", 3, 11);
                anims.Add(atlas, "Jump", 4, 1);
                anims.Add(atlas, "Running", 5, 12);
                anims.Add(atlas, "Wall Slide", 6, 5, AnimationOption.ForceInterruptOnStart);

                anims.Add(atlas, "Double Jump", 0, 6, AnimationOption.UninterruptibleUntilComplete, "Idle");
                anims.Add(atlas, "Hit", 2, 7, AnimationOption.PauseOnComplete | AnimationOption.ForceInterruptOnStart, "Idle");

                resources.Load("Player Animations", anims);
            }

            private static SpriteAtlas SetupSpriteAtlases(ResourceRegistry resources)
            {
                string[] spriteNames = [
                    "Ninja Frog",
                    "Pink Man",
                    "Virtual Guy",
                    "Mask Dude",
                ];

                SpriteAtlas atlas = null!;
                foreach (string spriteName in spriteNames)
                {
                    atlas = new SpriteAtlas(32, $"{resources.AssetDirectory}/Graphics/Player/{spriteName}.png");
                    resources.Load(spriteName, atlas);
                }
                return atlas;
            }

            private static void SetupSoundEffects(ResourceRegistry resources)
            {
                string path = $"{resources.AssetDirectory}/Sounds/Player";

                var hit = new SoundEffect([
                    $"{path}/Hit/hit-deep_147.wav",
                    $"{path}/Hit/hit-deep_148.wav",
                ]);

                var landing = new SoundEffect([
                    $"{path}/Landing/hit_040.wav",
                    $"{path}/Landing/hit_081.wav",
                ]);
                landing.SetVolume(0.2f);
                landing.SetPitchVariation(0.6f);

                var hardLanding = new SoundEffect([
                    $"{path}/Landing/hit_040.wav",
                    $"{path}/Landing/hit_056.wav",
                ]);
                hardLanding.SetVolume(0.6f);
                hardLanding.SetPitchVariation(0.6f);

                resources.Load("Player Hit", hit);
                resources.Load("Player Landing", landing);
                resources.Load("Player Hard Landing", hardLanding);
            }
        }
    }
}
