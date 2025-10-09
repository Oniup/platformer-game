using System.Numerics;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.Engine.Level
{
    public class AnimatedEffectActor(SpriteAtlas atlas, AnimationSet animations, Vector2 position) 
        : AnimatedActor(atlas, animations, position)
    {
        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (AnimationPaused)
                Destroy = true;
        }
    }
}