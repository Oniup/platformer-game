/// <summary>
/// COS20007:       Custom Project
/// Name:           Ewan Robson
/// Student ID:     103992579
/// Created:        9-21-2025
/// Last Edited:    9-25-2025
/// </summary>

namespace PlatformerGame.Engine
{
    public enum ResourceType
    {
        Sprite,
        SpriteAtlas,
        SoundEffect,
        MusicStream,
    }

    public abstract class Resource
    {
        public Resource(ResourceType type)
        {
            Type = type;
        }

        public ResourceType Type { get; init; }
    }
}