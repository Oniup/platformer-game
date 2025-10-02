using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Level.Collision;
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
            Random random = new Random();

            // Setup animations
            _delayNextBoingDuration = Math.Clamp(random.NextSingle() * 2.0f, 0.5f, 2.0f);
            _delayNextBoingTimer = 0.0f;
            PlayAnimation(random.Next(0, 7).ToString());
            PauseAnimation();

            // Setup collisions
            AddCircleCollider(Vector2.Zero, 12.0f, false);
            // AddBoxCollider(Vector2.Zero, 16, 16, false);
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

            // If player is colliding
            if (CollisionHitInfos.Count > 0)
            {
                // Fire event to add 1 to the score ...
                // Destroy = true;
            }
        }

        public class CreateInfo : CreateInfo<Fruit>
        {
            public override string EntityIdentifier => "Collectable";

            public override void SetupRequiredResources(LDtkDefinition.Entity? def, ResourceManager resources)
            {
                SpriteAtlas atlas = resources.Get<SpriteAtlas>(def!.TilesetId);

                AnimationSet anims = new AnimationSet();
                anims.Add(atlas, "0", 0, 17, true);
                anims.Add(atlas, "1", 1, 17, true);
                anims.Add(atlas, "2", 2, 17, true);
                anims.Add(atlas, "3", 3, 17, true);
                anims.Add(atlas, "4", 4, 17, true);
                anims.Add(atlas, "5", 5, 17, true);
                anims.Add(atlas, "6", 6, 17, true);
                anims.Add(atlas, "7", 7, 17, true);

                resources.Load("Fruit Animations", anims);
            }

            public override Actor Instantiate(ResourceManager resources, Scene? scene, LDtkDefinition.Entity? def, Vector2 position)
            {
                SpriteAtlas sprite = resources.Get<SpriteAtlas>(def!.TilesetId);
                AnimationSet anims = resources.Get<AnimationSet>("Fruit Animations");
                return new Fruit(sprite, anims, position);
            }
        }
    }
}