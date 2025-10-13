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
        protected Resource(ResourceType type)
        {
            Type = type;
        }

        public ResourceType Type { get; init; }

        public abstract void Dispose();
    }
}