using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame
{
    public class Fruit : CharacterActor
    {
        private readonly float _delayNextBoingDuration;
        private float _delayNextBoingTimer;

        public Fruit(SpriteAtlas sprite, AnimationSet animations, Vector2 position)
            : base(sprite, animations, CollisionLayer.Collectable, CollisionLayer.All & ~CollisionLayer.Player, position) // Only check for player collision
        {
            var random = new Random();

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
            EventDispatcher.FireEvent(new AddScoreEvent(1));
        }

        private void OnTrigger(CollidableActor collision, ShapeCollider collider)
        {
            Destroy = true;
        }

        public class CreateInfo : CreateInfo<Fruit>
        {
            public override void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def)
            {
                var atlas = resources.Get<SpriteAtlas>((int)def!.TilesetId!);
                var anims = new AnimationSet();

                anims.Add(atlas, "0", 0, 17, AnimationOption.PauseOnComplete);
                anims.Add(atlas, "1", 1, 17, AnimationOption.PauseOnComplete);
                anims.Add(atlas, "2", 2, 17, AnimationOption.PauseOnComplete);
                anims.Add(atlas, "3", 3, 17, AnimationOption.PauseOnComplete);
                anims.Add(atlas, "4", 4, 17, AnimationOption.PauseOnComplete);
                anims.Add(atlas, "5", 5, 17, AnimationOption.PauseOnComplete);
                anims.Add(atlas, "6", 6, 17, AnimationOption.PauseOnComplete);
                anims.Add(atlas, "7", 7, 17, AnimationOption.PauseOnComplete);

                resources.Load("Fruit Animations", anims);
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var sprite = resources.Get<SpriteAtlas>((int)info.Definition!.TilesetId!);
                var anims = resources.Get<AnimationSet>("Fruit Animations");
                return new Fruit(sprite, anims, info.Position);
            }
        }
    }

    public class FruitCollected(SpriteAtlas atlas, AnimationSet animations, Vector2 position) : AnimatedEffectActor(atlas, animations, position)
    {
        public class CreateInfo : CreateInfo<FruitCollected>
        {
            public override void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def)
            {
                var atlas = new SpriteAtlas(32, $"{resources.AssetDirectory}/Graphics/Effects/Fruit Collected.png");

                var anims = new AnimationSet();
                anims.Add(atlas, "Pop", 0, 6, AnimationOption.PauseOnComplete);

                var sound = new SoundEffect([
                    $"{resources.AssetDirectory}/Sounds/FruitCollected/menu_031.wav",
                ], 10);
                sound.SetPitchVariation(0.5f);
                sound.SetVolume(0.8f);

                resources.Load("Fruit Collected", atlas);
                resources.Load("Fruit Collected Animations", anims);
                resources.Load("Fruit Collected Sound", sound);
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var sprite = resources.Get<SpriteAtlas>("Fruit Collected");
                var anims = resources.Get<AnimationSet>("Fruit Collected Animations");

                resources.Get<SoundEffect>("Fruit Collected Sound").Play();

                return new FruitCollected(sprite, anims, info.Position);
            }
        }
    }
}