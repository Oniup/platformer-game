using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame
{
    public class TrunkEnemy : Enemy
    {
        public TrunkEnemy(SpriteAtlas atlas, AnimationSet animations, SoundEffect hitSound, EntityFields fields, Vector2 position)
            : base(atlas, animations, hitSound, position)
        {
            CurrentState = new NoState(this);
            SetupColliders(Vector2.UnitY * 5, -Vector2.UnitY * 7, atlas.GridSize - new Vector2(10), false, false);
        }

        public class CreateInfo : CreateInfo<TrunkEnemy>
        {
            public override void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def)
            {
                var atlas = resources.Get<SpriteAtlas>((int)def?.TilesetId!);
                atlas.GridWidth = 64;
                atlas.GridHeight = 32;

                var anims = new AnimationSet();
                anims.Add(atlas, "Idle", 0, 17);
                anims.Add(atlas, "Walk", 1, 13);
                anims.Add(atlas, "Shoot", 2, 10);
                anims.Add(atlas, "Hit", 2, 5, AnimationOption.UninterruptibleUntilComplete | AnimationOption.PauseOnComplete);

                resources.Load("Enemy Trunk Animations", anims);
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var atlas = resources.Get<SpriteAtlas>((int)info.Definition!.TilesetId!);
                var anims = resources.Get<AnimationSet>("Enemy Trunk Animations");
                var hitSound = resources.Get<SoundEffect>("Enemy Pig Hit Sound");

                return new TrunkEnemy(atlas, anims, hitSound, info.Fields!, info.Position);
            }
        }
    }
}