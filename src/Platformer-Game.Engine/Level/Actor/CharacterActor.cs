using System.Numerics;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Utilities;

namespace PlatformerGame.Engine.Level
{
    // TODO: Replace Actor inheritence with Collision Shape Actor
    public abstract class CharacterActor : Actor
    {
        const float DefaultFrameTime = 0.0461f; // 25 frames per second

        private SpriteAtlas _atlas;
        private List<Animation> _animations;
        private int _current;
        private float _frameTimer;
        private bool _paused;

        public CharacterActor(SpriteAtlas atlas, int id, Vector2 position, bool active = true)
            : base(id, position, active)
        {
            _atlas = atlas;
            _animations = new List<Animation>();
            _current = 0;
            _frameTimer = 0.0f;
            _paused = false;
        }

        public bool Paused
        {
            get { return _paused; }
        }

        public override void OnUpdate(float deltaTime)
        {
            if (_paused)
                return;

            Animation anim = _animations[_current];
            while (_frameTimer > anim.FrameDuration)
            {
                _frameTimer -= anim.FrameDuration;
                int previousFrame = anim.FrameIndex;
                anim.NextFrame();
                if (anim.PauseOnComplete && anim.FrameIndex == 0 && previousFrame != 0)
                    PauseAnimation();
            }
            _frameTimer += deltaTime;
        }

        public void PlayAnimation(string name, int startingFrame = 0)
        {
            for (int i = 0; i < _animations.Count; ++i)
            {
                if (_animations[i].Name == name)
                {
                    _animations[i].FrameIndex = startingFrame;
                    _current = i;
                    return;
                }
            }
            throw new NullReferenceException($"Failed to fetch {name} animation");
        }

        public void PauseAnimation()
        {
            _paused = true;
            _frameTimer = 0.0f;
        }

        public void ResumeAnimation()
        {
            _paused = false;
        }

        public void AddAnimation(string name, int row, int frameCount, bool pauseOnComplete = false, float duration = DefaultFrameTime)
        {
            Point begin = new Point(0, row * _atlas.GridHeight);
            Point end = new Point(frameCount * _atlas.GridWidth, row * _atlas.GridHeight);
            while (end.X >= _atlas.Width)
            {
                end.Y += _atlas.GridWidth;
                end.X -= _atlas.Width;
            }
            AddAnimation(name, begin, end, pauseOnComplete, duration, frameCount);
        }

        public void AddAnimation(string name, Point begin, Point end, bool pauseOnComplete = false, float duration = DefaultFrameTime, int frameCount = 0)
        {
#if DEBUG
            if (begin.Y > end.Y || (begin.X > end.X && begin.Y == end.Y))
                throw new ArgumentOutOfRangeException($"Begin point {begin} must be less than or equal to end point {end} when creating an animation");
            if (begin.X > _atlas.Width || end.X > _atlas.Width || begin.Y > _atlas.Height || end.Y > _atlas.Height)
                throw new ArgumentOutOfRangeException($"One of the points {begin} or {end} exceeds the sprite atlas size {new Point(_atlas.Width, _atlas.Height)}");
#endif
            List<Point> frames = new(frameCount);
            Point frame = begin;
            while (frame != end)
            {
                frames.Add(frame);
                frame.X += _atlas.GridWidth;
                if (frame.X >= _atlas.Width)
                {
                    frame.X = 0;
                    frame.Y += _atlas.GridHeight;
                }
            }
            // Make sure to still add the last frame
            frames.Add(frame);
            AddAnimation(name, frames, pauseOnComplete, duration);
        }

        public void AddAnimation(string name, List<Point> frames, bool pauseOnComplete, float frameDuration = DefaultFrameTime)
        {
#if DEBUG
            if (ContainsAnimation(name))
                throw new ArgumentException("An animation with the same name as \"{" + name +
                    "}\" already exists. Cannot have multiple animations with the same name in the same set");
#endif
            _animations.Add(new Animation(name, frames, pauseOnComplete, frameDuration));
        }

        public bool ContainsAnimation(string name)
        {
            foreach (Animation anim in _animations)
            {
                if (anim.Name == name)
                    return true;
            }
            return false;
        }

        public override void OnDraw()
        {
            _atlas.GridPosition = _animations[_current].GetFramePoint();
            _atlas.Draw(Position);
        }

        private class Animation
        {
            private string _name;
            private List<Point> _frames;
            private int _frameIndex;
            private float _frameDuration;
            private bool _pauseOnComplete;

            public Animation(string name, List<Point> frames, bool pauseOnComplete, float frameDuration)
            {
                _name = name;
                _frames = frames;
                _frameDuration = frameDuration;
                _pauseOnComplete = pauseOnComplete;
            }

            public int FrameIndex
            {
                get { return _frameIndex; }
                set
                {
                    if (value > _frames.Count)
                    {
                        Console.Error.WriteLine($"Frame {value} exceeds the frame count {_frames.Count}");
                        _frameIndex = 0;
                        return;
                    }
                    _frameIndex = value;
                }
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

            public void Reset()
            {
                _frameIndex = 0;
            }

            public void NextFrame()
            {
                _frameIndex = ++_frameIndex % _frames.Count();
            }

            public Point GetFramePoint()
            {
                return _frames[_frameIndex];
            }
        }
    }
}