namespace PlatformerGame.Engine.Resources
{
    public enum ResourceType
    {
        Sprite,
        SpriteAtlas,
        AnimationSet,
        RenderTarget,
        SoundEffect,
        MusicStream,
    }

    public abstract class Resource : IDisposable
    {
        public Resource(ResourceType type)
        {
            Type = type;
        }

        public ResourceType Type { get; init; }

        public abstract void Dispose();
    }
}