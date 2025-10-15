namespace PlatformerGame.Engine.Resources
{
    public enum ResourceType
    {
        Sprite,
        SpriteAtlas,
        CanvasSprite,
        AnimationSet,
        Framebuffer,
        MainFramebuffer,
        SoundEffect,
        MusicStream,
    }

    public abstract class Resource : IDisposable
    {
        public ResourceType Type { get; init; }

        protected Resource(ResourceType type)
        {
            Type = type;
        }

        public abstract void Dispose();
    }
}