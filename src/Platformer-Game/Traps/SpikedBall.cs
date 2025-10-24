using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame.Traps
{
    public class SpikedBall : CollidableSpriteActor
    {
        private float _radius;
        private float _angle;
        private float _rotationSpeed;
        private Vector2 _center;
        private Chain _chain;

        public SpikedBall(Sprite sprite, SpriteAtlas chainAtlas, float radius, float rotationSpeed, float startingAngle, Scene scene, Vector2 position)
            : base(sprite, CollisionLayer.Trap | CollisionLayer.Damage, CollisionLayer.All & ~CollisionLayer.Player, position)
        {
            _radius = radius;
            _angle = startingAngle;
            _rotationSpeed = rotationSpeed;
            _center = position;
            _chain = new Chain(chainAtlas, Chain.Type.Metal, _center, Vector2.Zero);

            AddCircleCollider(Vector2.Zero, 10, OnPlayerEnter);
        }

        public override void OnUpdate(float deltaTime)
        {
            if (World.Paused)
                return;

            CalculateCollisions();

            _angle += _rotationSpeed * deltaTime;
            Position = new Vector2
            {
                X = _center.X + MathF.Cos(_angle) * _radius,
                Y = _center.Y + MathF.Sin(_angle) * _radius,
            };
            _chain.EndPosition = Position;
        }

        public override void OnDraw()
        {
            _chain.Draw();
            base.OnDraw();
        }

        private void OnPlayerEnter(CollidableActor other, ShapeCollider collider)
        {
            EventDispatcher.FireEvent(new PlayerHitEvent(), this);
        }

        public class CreateInfo : CreateInfo<SpikedBall>
        {
            public override void SetupRequiredResources(ResourceManager resources, LDtkDefinition.Entity? def)
            {
                resources.Load("Spiked Ball", new Sprite($"{resources.AssetDirectory}/Graphics/Traps/Spiked Ball.png"));
                resources.Load("Chains", new SpriteAtlas(8, $"{resources.AssetDirectory}/Graphics/Traps/Chains (8x8).png"));
            }

            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                var sprite = resources.Get<Sprite>("Spiked Ball");
                var chainsAtlas = resources.Get<SpriteAtlas>("Chains");

                var radiusPoint = info.Fields!.GetValue<Vector2>("Radius") + info.Scene!.WorldOffset;
                var rotationSpeed = info.Fields.GetValue<float>("RotationSpeed");

                float radius = MathF.Abs(Vector2.Distance(info.Position, radiusPoint));

                Vector2 radiusDir = radiusPoint - info.Position;
                float startingAngle = MathF.Atan2(radiusDir.Y, radiusDir.X);

                return new SpikedBall(sprite, chainsAtlas, radius, rotationSpeed, startingAngle, info.Scene, info.Position);
            }
        }
    }
}