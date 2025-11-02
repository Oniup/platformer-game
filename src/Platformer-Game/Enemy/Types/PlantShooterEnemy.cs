using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame
{
    public class PlantShooterEnemy : Enemy
    {
        public PlantShooterEnemy(SpriteAtlas atlas, AnimationSet animations, SoundEffect hitSound, EntityFields fields, Vector2 position)
            : base(atlas, animations, hitSound, position)
        {
            CurrentState = new NoState(this);
            var baseOffset = new Vector2(3, 7);
            SetupColliders(baseOffset, baseOffset - Vector2.UnitY * 16, atlas.GridSize - new Vector2(25, 15), false, false);
        }

        public class CreateInfo : CreateInfo<PlantShooterEnemy>
        {
            public override void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def)
            {
                var atlas = resources.Get<SpriteAtlas>((int)def?.TilesetId!);
                atlas.GridWidth = 44;
                atlas.GridHeight = 42;

                var anims = new AnimationSet();
                anims.Add(atlas, "Idle", 0, 10);
                anims.Add(atlas, "Shoot", 1, 7);
                anims.Add(atlas, "Hit", 2, 5, AnimationOption.UninterruptibleUntilComplete | AnimationOption.PauseOnComplete);

                resources.Load("Enemy PlantShooter Animations", anims);
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var atlas = resources.Get<SpriteAtlas>((int)info.Definition!.TilesetId!);
                var anims = resources.Get<AnimationSet>("Enemy PlantShooter Animations");
                var hitSound = resources.Get<SoundEffect>("Enemy Pig Hit Sound");

                return new PlantShooterEnemy(atlas, anims, hitSound, info.Fields!, info.Position);
            }
        }
    }
}