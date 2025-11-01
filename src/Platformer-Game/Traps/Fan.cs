using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame.Traps
{
    public class Fan : CharacterActor
    {
        private SoundEffect _hitSound;
        public float _pushForce;
        public float _maxSpeed;
        public bool _isOn;

        public Fan(SpriteAtlas atlas, AnimationSet animations, SoundEffect hitSound, EntityFields fields, Vector2 pushDir, Vector2 position)
            : base(atlas, animations, CollisionLayer.Damage | CollisionLayer.Trap, CollisionLayer.All & ~CollisionLayer.Player, position)
        {
            _hitSound = hitSound;
            _pushForce = fields.GetValue<float>("PushForce");
            _maxSpeed = -fields.GetValue<float>("MaxActorSpeed");
            _isOn = fields.GetValue<bool>("IsOn");

            float pushRange = MathF.Abs(Vector2.Distance(pushDir, Position));

            AddBoxCollider(Vector2.Zero, 20, 8, OnHitFan);
            var centerPoint = -Vector2.UnitY * pushRange / 2;
            AddBoxCollider(centerPoint, 28, pushRange - 14, OnTriggerEnter);
        }

        private void OnTriggerEnter(CollidableActor other, ShapeCollider collider)
        {
            if (_isOn && other is Player player)
            {
                float multiplier = 1.0f;
                if (player.Velocity.Y > 100.0f)
                    multiplier = 4.0f;

                player.ApplyForce -= Vector2.UnitY * _pushForce * multiplier;
                if (player.Velocity.Y < _maxSpeed)
                    player.Velocity = new Vector2(player.Velocity.X, _maxSpeed);
            }
        }

        private void OnHitFan(CollidableActor other, ShapeCollider collider)
        {
            _hitSound.Play();
            EventDispatcher.FireEvent(new PlayerHitEvent(), this);
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
                return new Fan(atlas, anims, hitSound, info.Fields, pushDir, info.Position);
            }
        }
    }
}