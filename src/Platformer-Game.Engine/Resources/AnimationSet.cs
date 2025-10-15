using System.Numerics;

namespace PlatformerGame.Engine.Resources
{
    [Flags]
    public enum AnimationOption
    {
        Loop                            = 1 << 1,
        PauseOnComplete                 = 1 << 2,
        UninterruptableUntilComplete    = 1 << 3,
        ForceInterruptOnStart           = 1 << 4,
    }

    public class AnimationSet : Resource
    {
        public const float DefaultFrameTime = 0.0461f; // 25 frames per second

        private List<Animation> _animations;

        public AnimationSet()
            : base(ResourceType.AnimationSet)
        {
            _animations = new List<Animation>();
        }

        public Animation Default
        {
            get { return _animations.First(); }
        }

        public Animation Get(string name)
        {
            foreach (Animation animation in _animations)
            {
                if (animation.Name == name)
                    return animation;
            }
            throw new NullReferenceException($"Failed to fetch {name} animation");
        }

        public void Add(SpriteAtlas atlas, string name, int row, int frameCount, AnimationOption options = AnimationOption.Loop, string? playAfter = null, float duration = DefaultFrameTime)
        {
            Vector2 begin = new Vector2(0, row * atlas.GridHeight);
            Vector2 end = new Vector2(frameCount * atlas.GridWidth, row * atlas.GridHeight);
            while (end.X >= atlas.Width)
            {
                end.Y += atlas.GridWidth;
                end.X -= atlas.Width;
            }
            Add(atlas, name, begin, end, options, playAfter, duration, frameCount);
        }

        public void Add(SpriteAtlas atlas, string name, Vector2 begin, Vector2 end, AnimationOption options = AnimationOption.Loop, string? playAfter = null, float duration = DefaultFrameTime, int frameCount = 0)
        {
#if DEBUG
            if (begin.Y > end.Y || (begin.X > end.X && begin.Y == end.Y))
                throw new ArgumentOutOfRangeException($"Begin point {begin} must be less than or equal to end point {end} when creating an animation");
            if (begin.X > atlas.Width || end.X > atlas.Width || begin.Y > atlas.Height || end.Y > atlas.Height)
                throw new ArgumentOutOfRangeException($"One of the points {begin} or {end} exceeds the sprite atlas size {new Vector2(atlas.Width, atlas.Height)}");
#endif
            List<Vector2> frames = new(frameCount);
            Vector2 frame = begin;
            while (frame != end)
            {
                frames.Add(frame);
                frame.X += atlas.GridWidth;
                if (frame.X >= atlas.Width)
                {
                    frame.X = 0;
                    frame.Y += atlas.GridHeight;
                }
            }
            Add(name, frames, options, playAfter, duration);
        }

        public void Add(string name, List<Vector2> frames, AnimationOption options = AnimationOption.Loop, string? playAfter = null, float frameDuration = DefaultFrameTime)
        {
#if DEBUG
            if (Contains(name))
                throw new ArgumentException($"An animation with the same name as \"{name}\" already exists. Cannot have multiple animations with the same name in the same set");
            if (playAfter != null && playAfter == name)
                throw new ArgumentException($"Cannot have the playAfter parameter \"{playAfter}\" be the same as the animation name \"{name}\"");
#endif
            _animations.Add(new Animation(name, playAfter, frames, options, frameDuration));
        }

        public bool Contains(string name)
        {
            foreach (Animation anim in _animations)
            {
                if (anim.Name == name)
                    return true;
            }
            return false;
        }

        public override void Dispose()
        {
        }

        public class Animation
        {
            private string _name;
            private string? _playAfter;
            private List<Vector2> _frames;
            private float _frameDuration;
            private AnimationOption _options;

            public Animation(string name, string? playAfter, List<Vector2> frames, AnimationOption options, float frameDuration)
            {
                _name = name;
                _playAfter = playAfter;
                _frames = frames;
                _frameDuration = frameDuration;
                _options = options;
            }

            public string Name
            {
                get { return _name; }
            }

            public float FrameDuration
            {
                get { return _frameDuration; }
            }

            public int FrameCount
            {
                get { return _frames.Count; }
            }

            public AnimationOption Options
            {
                get { return _options; }
            }

            public string? PlayAfter
            {
                get { return _playAfter; }
            }

            public void NextFrame(ref int frameIndex)
            {
                frameIndex = ++frameIndex % _frames.Count();
            }

            public Vector2 GetFramePoint(int frameIndex)
            {
                return _frames[frameIndex];
            }
        }
    }
}