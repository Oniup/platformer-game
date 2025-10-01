using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame
{
    public class Fruit : CharacterActor
    {
        private readonly float _delayNextBoingDuration;
        private float _delayNextBoingTimer;

        public Fruit(SpriteAtlas sprite, int id, Vector2 position, bool active = true)
            : base(sprite, id, position, active)
        {
            Random random = new Random();
            AddAnimation("Idle", random.Next(0, 7), 17, true);

            // Max 2 second delay between boings
            _delayNextBoingDuration = Math.Clamp(random.NextSingle() * 2.0f, 0.5f, 2.0f);
            _delayNextBoingTimer = 0.0f;
            PauseAnimation();
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

        public class CreateInfo : CreateInfo<Fruit>
        {
            public override string EntityIdentifier => "Collectable";

            public override Actor Instantiate(ResourceManager resources, Scene? scene, LDtkDefinition.Entity? def, Vector2 position)
            {
                SpriteAtlas sprite = resources.Get<SpriteAtlas>(def!.TilesetId);
                return new Fruit(sprite, def.UId, position);
            }
        }
    }
}