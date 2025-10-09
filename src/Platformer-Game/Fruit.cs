using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Level.Collision;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame
{
    public class Fruit : CharacterActor
    {
        private readonly float _delayNextBoingDuration;
        private float _delayNextBoingTimer;

        public Fruit(SpriteAtlas sprite, AnimationSet animations, Vector2 position)
            : base(sprite, animations, CollisionLayer.Collectable, CollisionLayer.All & ~CollisionLayer.Player, position) // Only check for player collision
        {
            Random random = new Random();

            // Setup animations
            _delayNextBoingDuration = Math.Clamp(random.NextSingle() * 2.0f, 0.5f, 2.0f);
            _delayNextBoingTimer = 0.0f;
            PlayAnimation(random.Next(0, 7).ToString());
            PauseAnimation();

            // Setup collisions
            AddCircleCollider(Vector2.Zero, 12.0f, OnTrigger);
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (AnimationPaused)
            {
                if (_delayNextBoingTimer > _delayNextBoingDuration)
                {
                    ResumeAnimation();
                    _delayNextBoingTimer = 0.0f;
                }
                else
                    _delayNextBoingTimer += deltaTime;
            }
        }

        public override void OnDestroy()
        {
            World.Instantiate<FruitCollected>(Position, World.CurrentScene);
        }

        private void OnTrigger(CollidableActor collision, ShapeCollider collider)
        {
            // Fire event to add 1 score
            Destroy = true;
        }

        public class CreateInfo : CreateInfo<Fruit>
        {
            public override void SetupRequiredResources(LDtkDefinition.Entity? def, ResourceManager resources)
            {
                SpriteAtlas atlas = resources.Get<SpriteAtlas>((int)def!.TilesetId!);

                AnimationSet anims = new AnimationSet();
                anims.Add(atlas, "0", 0, 17, AnimationMode.PauseOnComplete);
                anims.Add(atlas, "1", 1, 17, AnimationMode.PauseOnComplete);
                anims.Add(atlas, "2", 2, 17, AnimationMode.PauseOnComplete);
                anims.Add(atlas, "3", 3, 17, AnimationMode.PauseOnComplete);
                anims.Add(atlas, "4", 4, 17, AnimationMode.PauseOnComplete);
                anims.Add(atlas, "5", 5, 17, AnimationMode.PauseOnComplete);
                anims.Add(atlas, "6", 6, 17, AnimationMode.PauseOnComplete);
                anims.Add(atlas, "7", 7, 17, AnimationMode.PauseOnComplete);

                resources.Load("Fruit Animations", anims);
            }

            public override Actor Instantiate(ResourceManager resources, Scene? scene, LDtkDefinition.Entity? def, Vector2 position)
            {
                SpriteAtlas sprite = resources.Get<SpriteAtlas>((int)def!.TilesetId!);
                AnimationSet anims = resources.Get<AnimationSet>("Fruit Animations");
                return new Fruit(sprite, anims, position);
            }
        }
    }

    public class FruitCollected(SpriteAtlas atlas, AnimationSet animations, Vector2 position) 
        : AnimatedEffectActor(atlas, animations, position)
    {
        public class CreateInfo : CreateInfo<FruitCollected>
        {
            public override void SetupRequiredResources(LDtkDefinition.Entity? def, ResourceManager resources)
            {
                string asset = resources.AssetDirectory + "/Graphics/Effects/Fruit Collected.png";
                SpriteAtlas atlas = new SpriteAtlas(32, asset);
                AnimationSet anims = new AnimationSet();

                anims.Add(atlas, "Pop", 0, 6, AnimationMode.PauseOnComplete);

                resources.Load("Fruit Collected", atlas);
                resources.Load("Fruit Collected Animations", anims);
            }

            public override Actor Instantiate(ResourceManager resources, Scene? scene, LDtkDefinition.Entity? def, Vector2 position)
            {
                SpriteAtlas sprite = resources.Get<SpriteAtlas>("Fruit Collected");
                AnimationSet anims = resources.Get<AnimationSet>("Fruit Collected Animations");
                return new FruitCollected(sprite, anims, position);
            }
        }
    }
}