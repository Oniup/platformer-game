using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Utilities;
using Raylib_cs;

namespace PlatformerGame.Traps
{
    public class Fan : PawnActor
    {
        private SoundEffect _hitSound;
        public float _pushForce;
        public float _maxSpeed;
        public bool _isOn;

        private List<CharacterActor> _pushed;
        private List<Fan> _otherFans = null!;

#if DEBUG
        private bool _debugShowPush;
#endif

        public Fan(SpriteAtlas atlas, AnimationSet animations, SoundEffect hitSound, EntityFields fields, Vector2 pushDir, Scene scene, Vector2 position)
            : base(atlas, animations, CollisionLayer.Damage | CollisionLayer.Trap, CollisionLayer.All & ~(CollisionLayer.Player | CollisionLayer.Enemy), position)
        {
            _hitSound = hitSound;
            _pushForce = fields.GetValue<float>("PushForce");
            _maxSpeed = -fields.GetValue<float>("MaxActorSpeed");
            _isOn = fields.GetValue<bool>("IsOn");
            _pushed = new List<CharacterActor>();

            float pushRange = MathF.Abs(Vector2.Distance(pushDir, Position));

            AddBoxCollider(Vector2.Zero, 20, 8, OnHitFan);
            var centerPoint = -Vector2.UnitY * pushRange / 2;
            AddBoxCollider(centerPoint, 28, pushRange - 14, OnTriggerEnter);
        }

        public override void OnAwake(Scene? scene)
        {
            _otherFans = World.Find<Fan>(scene);
            if (!_otherFans.Remove(this))
                throw new NullReferenceException("HUh?");
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            _pushed.Clear();
        }

        public override void OnDraw()
        {
            base.OnDraw();

#if DEBUG
            if (World.ShowCollisionOutlines && _debugShowPush)
                Raylib.DrawRectangleV(Position, new Vector2(5), Color.Red);
            _debugShowPush = false;
#endif
        }

        private void OnTriggerEnter(CollidableActor actor, ShapeCollider collider)
        {
            if (!collider.IsTrigger && _isOn && actor is CharacterActor character)
            {
                // Check if other fans have pushed the character, to prevent close fans from applying double the force that is intended
                foreach (Fan other in _otherFans)
                {
                    foreach (CharacterActor pushed in other._pushed)
                    {
                        if (ReferenceEquals(pushed, character))
                            return;
                    }
                }

                _pushed.Add(character);
#if DEBUG
                _debugShowPush = true;
#endif

                float multiplier = 1.0f;
                if (character.Velocity.Y > 100.0f)
                    multiplier = 4.0f;

                character.ApplyForce -= Vector2.UnitY * _pushForce * multiplier;
            }
        }

        private void OnHitFan(CollidableActor actor, ShapeCollider collider)
        {
            if (collider.IsTrigger)
                return;

            _hitSound.Play();
            if (actor.CollisionLayer.HasFlag(CollisionLayer.Player))
                EventDispatcher.FireEvent(new PlayerHitEvent(), this);

            else if (actor.CollisionLayer.HasFlag(CollisionLayer.Enemy))
            {
                var enemy = (Enemy)actor;
                enemy.SetToDeathState();
            }
        }

        public class CreateInfo : CreateInfo<Fan>
        {
            public override void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def)
            {
                var atlas = resources.Get<SpriteAtlas>((int)def!.TilesetId!);
                // Correct grid size to the grid size of the sprite atlas
                atlas.GridWidth = 24;
                atlas.GridHeight = 8;

                var anims = new AnimationSet();
                anims.Add(atlas, "On", 0, 3);
                anims.Add(atlas, "Off", 0, 1);

                resources.Load("Fan Animations", anims);

                var hitSound = new SoundEffect([
                    $"{resources.AssetDirectory}/Sounds/Traps/Fan/metal_015.wav",
                    $"{resources.AssetDirectory}/Sounds/Traps/Fan/metal_016.wav",
                ]);
                hitSound.SetVolume(0.3f);
                hitSound.SetPitchVariation(0.8f);
                resources.Load("Fan Hit Sound", hitSound);
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var atlas = resources.Get<SpriteAtlas>((int)info.Definition!.TilesetId!);
                var anims = resources.Get<AnimationSet>("Fan Animations");
                var hitSound = resources.Get<SoundEffect>("Fan Hit Sound");

                var pushDir = info.Fields!.GetValue<Vector2>("PushRange") + info.Scene!.WorldOffset;
                return new Fan(atlas, anims, hitSound, info.Fields, pushDir, info.Scene, info.Position);
            }
        }
    }
}