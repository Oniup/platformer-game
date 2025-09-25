using System.Numerics;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame.Engine.Level
{
    public class Animation
    {
        private string _name;
        private List<Point> _frames;
        private float _frameDuration;

        public Animation(string name, List<Point> frames, float frameDuration)
        {
            _name = name;
            _frames = frames;
            _frameDuration = frameDuration;

            CurrentFrameIndex = 0;
        }

        public int CurrentFrameIndex { get; set; }

        public string Name
        {
            get { return _name; }
        }

        public float FrameDuration
        {
            get { return _frameDuration; }
        }

        public Point CurrentFrame
        {
            get { return _frames[CurrentFrameIndex]; }
        }

        public void Restart()
        {
            CurrentFrameIndex = 0;
        }
    }

    // TODO: Replace Actor inheritence with Collision Shape Actor
    public abstract class CharacterActor : Actor
    {
        private SpriteAtlas _atlas;
        private List<Animation> _animations;
        private int _currentAnimIndex;
        private float _animTimer;

        protected CharacterActor(SpriteAtlas atlas, int id, Vector2 position, bool active = true)
            : base(id, position, active)
        {
            _atlas = atlas;
            _animations = new List<Animation>();
            _currentAnimIndex = 0;
            _animTimer = 0.0f;
        }
    }
}