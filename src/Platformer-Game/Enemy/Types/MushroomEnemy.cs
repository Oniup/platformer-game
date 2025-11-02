using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame
{
    public class MushroomEnemy : Enemy
    {
        public MushroomEnemy(SpriteAtlas atlas, AnimationSet animations, SoundEffect hitSound, EntityFields fields, Vector2 position)
            : base(atlas, animations, hitSound, position)
        {
            MoveDirection = fields.GetValue<float>("StartMoveDirection");
            CurrentState = fields.GetValue<bool>("StartWithWalkState") ? new MushIdleState(this) : new MushWalkState(this);
            SetupColliders(Vector2.UnitY * 9, Vector2.Zero, atlas.GridSize - new Vector2(10, 20));
        }

        private class MushIdleState(Enemy enemy, bool shouldSwitchDirection = false) : IdleState(enemy, shouldSwitchDirection)
        {
            public override IState? SwitchState()
            {
                if (SwitchToWalkState())
                    return new MushWalkState(Self);

                return null;
            }
        }

        private class MushWalkState(Enemy enemy) : WalkState(enemy)
        {
            public override IState? SwitchState()
            {
                if (SwitchToIdleState(out bool idleShouldSwitchDirection))
                    return new MushIdleState(Self, idleShouldSwitchDirection);

                return null;
            }
        }

        public class CreateInfo : CreateInfo<MushroomEnemy>
        {
            public override void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def)
            {
                var atlas = resources.Get<SpriteAtlas>((int)def?.TilesetId!);
                var anims = new AnimationSet();
                anims.Add(atlas, "Idle", 0, 13);
                anims.Add(atlas, "Walk", 1, 13);
                anims.Add(atlas, "Hit", 2, 5, AnimationOption.UninterruptibleUntilComplete | AnimationOption.PauseOnComplete);

                resources.Load("Enemy Mushroom Animations", anims);
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var atlas = resources.Get<SpriteAtlas>((int)info.Definition!.TilesetId!);
                var anims = resources.Get<AnimationSet>("Enemy Mushroom Animations");
                var hitSound = resources.Get<SoundEffect>("Enemy Pig Hit Sound");

                return new MushroomEnemy(atlas, anims, hitSound, info.Fields!, info.Position);
            }
        }
    }
}