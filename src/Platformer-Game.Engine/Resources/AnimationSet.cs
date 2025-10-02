using System.Numerics;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.Engine.Utilities
{
    public interface IAnimatable
    {
        public void PlayAnimation(string name, int startingFrame = 0);
        public void PauseAnimation();
        public void ResumeAnimation();
    }

    internal class AnimationController
    {
        private AnimationSet _animationSet;
        private Animation _currentAnimation;
        private int _frameIndex;
        private bool _paused;
        private float _frameTimer;

        public AnimationController(AnimationSet animationSet)
        {
            _animationSet = animationSet;
            _currentAnimation = animationSet.Default;
            _paused = false;
            _frameTimer = 0.0f;
            _frameIndex = 0;
        }

        public bool Paused
        {
            get { return _paused; }
        }

        public void Play(string name, int startingFrame)
        {
            _currentAnimation = _animationSet.Get(name);
            _frameIndex = startingFrame;
        }

        public void Pause()
        {
            _paused = true;
            _frameTimer = 0.0f;
        }

        public void Resume()
        {
            _paused = false;
        }

        public void UpdateAnimation(float deltaTime)
        {
            if (_paused)
                return;

            while (_frameTimer > _currentAnimation.FrameDuration)
            {
                _frameTimer -= _currentAnimation.FrameDuration;
                int previousFrame = _frameIndex;

                _currentAnimation.NextFrame(ref _frameIndex);
                if (_currentAnimation.PauseOnComplete && _frameIndex == 0 && previousFrame != 0)
                    Pause();
            }
            _frameTimer += deltaTime;
        }

        public void DrawFrame(SpriteAtlas atlas, Vector2 position)
        {
            atlas.GridPosition = _currentAnimation.GetFramePoint(_frameIndex);
            atlas.Draw(position - new Vector2(atlas.GridWidth * 0.5f, atlas.GridHeight * 0.5f));
        }
    }

    internal class Animation
    {
        private string _name;
        private List<Point> _frames;
        private float _frameDuration;
        private bool _pauseOnComplete;

        public Animation(string name, List<Point> frames, bool pauseOnComplete, float frameDuration)
        {
            _name = name;
            _frames = frames;
            _frameDuration = frameDuration;
            _pauseOnComplete = pauseOnComplete;
        }

        public string Name
        {
            get { return _name; }
        }

        public float FrameDuration
        {
            get { return _frameDuration; }
        }

        public bool PauseOnComplete
        {
            get { return _pauseOnComplete; }
        }

        public void NextFrame(ref int frameIndex)
        {
            frameIndex = ++frameIndex % _frames.Count();
        }

        public Point GetFramePoint(int frameIndex)
        {
            return _frames[frameIndex];
        }
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

        internal Animation Default
        {
            get { return _animations.First(); }
        }

        internal Animation Get(string name)
        {
            foreach (Animation animation in _animations)
            {
                if (animation.Name == name)
                    return animation;
            }
            throw new NullReferenceException($"Failed to fetch {name} animation");
        }

        public void Add(SpriteAtlas atlas, string name, int row, int frameCount, bool pauseOnComplete = false, float duration = DefaultFrameTime)
        {
            Point begin = new Point(0, row * atlas.GridHeight);
            Point end = new Point(frameCount * atlas.GridWidth, row * atlas.GridHeight);
            while (end.X >= atlas.Width)
            {
                end.Y += atlas.GridWidth;
                end.X -= atlas.Width;
            }
            Add(atlas, name, begin, end, pauseOnComplete, duration, frameCount);
        }

        public void Add(SpriteAtlas atlas, string name, Point begin, Point end, bool pauseOnComplete = false, float duration = DefaultFrameTime, int frameCount = 0)
        {
#if DEBUG
            if (begin.Y > end.Y || (begin.X > end.X && begin.Y == end.Y))
                throw new ArgumentOutOfRangeException($"Begin point {begin} must be less than or equal to end point {end} when creating an animation");
            if (begin.X > atlas.Width || end.X > atlas.Width || begin.Y > atlas.Height || end.Y > atlas.Height)
                throw new ArgumentOutOfRangeException($"One of the points {begin} or {end} exceeds the sprite atlas size {new Point(atlas.Width, atlas.Height)}");
#endif
            List<Point> frames = new(frameCount);
            Point frame = begin;
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
            Add(name, frames, pauseOnComplete, duration);
        }

        public void Add(string name, List<Point> frames, bool pauseOnComplete, float frameDuration = DefaultFrameTime)
        {
#if DEBUG
            if (Contains(name))
                throw new ArgumentException("An animation with the same name as \"{" + name +
                    "}\" already exists. Cannot have multiple animations with the same name in the same set");
#endif
            _animations.Add(new Animation(name, frames, pauseOnComplete, frameDuration));
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
    }
}