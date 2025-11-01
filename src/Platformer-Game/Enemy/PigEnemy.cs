using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame
{
    public class PigEnemy : Enemy
    {
        public PigEnemy(SpriteAtlas atlas, AnimationSet animations, SoundEffect hitSound, float detectRange, Vector2 position)
            : base(atlas, animations, hitSound, position)
        {
            CurrentState = new IdleState(this);

            float colliderWidth = atlas.GridWidth - 10;
            AddBoxCollider(Vector2.UnitY * 5, colliderWidth, atlas.GridHeight - 10);
            AddBoxCollider(-Vector2.UnitY * 7, colliderWidth, 6, OnHeadHitTrigger);
            AddBoxCollider(Vector2.UnitY * 5, detectRange, atlas.GridHeight * 1.5f, OnVisionEnterTrigger);
        }

        private void OnVisionEnterTrigger(CollidableActor other, ShapeCollider collider)
        {
        }

        protected class IdleState : State<PigEnemy>
        {
            public IdleState(PigEnemy self) 
                : base(self)
            {
                Self.PlayAnimation("Idle");
            }

            public override void OnUpdate(float deltaTime)
            {
            }

            public override IState? SwitchState()
            {
                return null;
            }
        }

        public class CreateInfo : CreateInfo<PigEnemy>
        {
            public override void SetupRequiredResources(ResourceRegistry resources, LDtkDefinition.Entity? def)
            {
                var atlas = resources.Get<SpriteAtlas>((int)def!.TilesetId!);
                atlas.GridWidth = 36;
                atlas.GridHeight = 30;

                var anims = new AnimationSet();
                anims.Add(atlas, "Idle", 0, 9);
                anims.Add(atlas, "AngryWalk", 1, 12);
                anims.Add(atlas, "Walk", 2, 15);
                anims.Add(atlas, "Hit", 3, 5, AnimationOption.UninterruptibleUntilComplete | AnimationOption.PauseOnComplete);
                resources.Load("Enemy Pig Animations", anims);

                var hitSound = new SoundEffect([
                    $"{resources.AssetDirectory}/Sounds/Enemy/Hit/CCS_01.wav",
                    $"{resources.AssetDirectory}/Sounds/Enemy/Hit/CCS_03.wav",
                ], 2);
                hitSound.SetPitchVariation(0.5f);
                hitSound.SetVolume(0.5f);
                resources.Load("Enemy Pig Hit Sound", hitSound);
            }

            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var atlas = resources.Get<SpriteAtlas>((int)info.Definition!.TilesetId!);
                var anims = resources.Get<AnimationSet>("Enemy Pig Animations");
                var hitSound = resources.Get<SoundEffect>("Enemy Pig Hit Sound");

                float detectRange = info.Fields!.GetValue<float>("DetectRange");
                return new PigEnemy(atlas, anims, hitSound, detectRange, info.Position);
            }
        }
    }
}