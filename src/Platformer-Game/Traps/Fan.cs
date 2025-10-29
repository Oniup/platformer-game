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
        public float _pushForce;
        public float _maxSpeed;
        public bool _isOn;

        public Fan(float pushRange, float pushForce, float maxSpeed, bool isOn, SpriteAtlas atlas, AnimationSet animations, Vector2 position)
            : base(atlas, animations, CollisionLayer.Trap, CollisionLayer.All & ~CollisionLayer.Player, position)
        {
            _pushForce = pushForce;
            _maxSpeed = -maxSpeed;
            _isOn = isOn;

            AddBoxCollider(Vector2.Zero, 20, 8, OnHitFan);
            var centerPoint = -Vector2.UnitY * pushRange / 2;
            AddBoxCollider(centerPoint, 28, pushRange - 14, OnTriggerEnter);
        }

        private void OnTriggerEnter(CollidableActor other, ShapeCollider collider)
        {
            if (_isOn && other is Player player)
            {
                if (player.HasFanPushedAlready)
                    return;

                float multiplier = 1.0f;
                if (player.Velocity.Y > 100.0f)
                    multiplier = 4.0f;

                player.ApplyForce -= Vector2.UnitY * _pushForce * multiplier;
                if (player.Velocity.Y < _maxSpeed)
                    player.Velocity = new Vector2(player.Velocity.X, _maxSpeed);

                // player.HasFanPushedAlready = true;
            }
        }

        private void OnHitFan(CollidableActor other, ShapeCollider collider)
        {
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
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var atlas = resources.Get<SpriteAtlas>((int)info.Definition!.TilesetId!);
                var anims = resources.Get<AnimationSet>("Fan Animations");

                var pushDir = info.Fields!.GetValue<Vector2>("PushRange") + info.Scene!.WorldOffset;
                var pushForce = info.Fields.GetValue<float>("PushForce");
                var maxActorSpeed = info.Fields.GetValue<float>("MaxActorSpeed");
                var isOn = info.Fields.GetValue<bool>("IsOn");

                float pushRange = MathF.Abs(Vector2.Distance(pushDir, info.Position));
                return new Fan(pushRange, pushForce, maxActorSpeed, isOn, atlas, anims, info.Position);
            }
        }
    }
}