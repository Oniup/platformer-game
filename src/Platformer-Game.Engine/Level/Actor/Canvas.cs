using System.Numerics;
using PlatformerGame.Engine.Events;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Utilities;
using Raylib_cs;

namespace PlatformerGame.Engine.Level.UI
{
    /// <summary>
    /// Composite design pattern for creating element groups that contains individual elements that when combined
    /// create one element group
    /// </summary>
    public abstract class Canvas : Actor
    {
        private OrderedDictionary<string, ElementGroup> _elements;
        private bool _selected;
        private SoundEffect? _nextButtonSound;
        private SoundEffect? _selectButtonSound;

        protected ElementGroup? HoveringElement { get; set; }
        public bool Showing { get; set; }
        public bool UpdateOnlyHovered { get; init; }

        public Canvas(Vector2 position)
            : this(null, null, position)
        {
        }

        public Canvas(SoundEffect? nextButtonSound, SoundEffect? selectButtonSound, Vector2 position)
            : base(position)
        {
            _elements = new OrderedDictionary<string, ElementGroup>();
            _nextButtonSound = nextButtonSound;
            _selectButtonSound = selectButtonSound;

            Showing = false;
            UpdateOnlyHovered = false;

            EventDispatcher.AddListener<NewCurrentSceneEvent>(this, OnNewCurrentSceneEvent);
        }

        public virtual Vector2 Center
        {
            get
            {
                (int winWidth, int winHeight) = Window.GetResolutionSize(WindowResolution.nHD);
                return new Vector2
                {
                    X = winWidth / 2,
                    Y = winHeight / 2,
                };
            }
        }

        public override void OnDispose()
        {
            EventDispatcher.RemoveListener<NewCurrentSceneEvent>(this);
        }

        public override void OnUpdate(float deltaTime)
        {
            if (!Showing)
                return;

            HandleHoveringElement();
            UpdateAnimatedElements(deltaTime);
        }

        public override void OnBeforeUpdate(float deltaTime)
        {
            if (_selected)
            {
                if (HoveringElement!.OnPress == null)
                    throw new NullReferenceException("OnPress should not be null for selectable elements");
                HoveringElement.OnPress();
                _selected = false;
            }
        }

        public override void OnDraw()
        {
            if (!Showing)
                return;

            foreach ((_, ElementGroup element) in _elements)
                element.Draw(Position, HoveringElement == element);
        }

        public virtual Vector2 PositionOffsetFromScene()
        {
            return Vector2.Zero;
        }

        public ElementGroup AddElement(string name, ElementGroup element)
        {
            _elements.Add(name, element);
            return element;
        }

        private void HandleHoveringElement()
        {
            if (HoveringElement == null)
                return;

            GetInput(out NextElementDirection direction, out bool select);
            if (select)
            {
                _selected = true;
                _selectButtonSound?.Play();
                return;
            }
            if (direction != NextElementDirection.None)
            {
                foreach ((NextElementDirection dir, string name) in HoveringElement.Next)
                {
                    if (dir == direction)
                    {
                        HoveringElement = _elements[name];
                        if (!HoveringElement.IsSelectable)
                            throw new InvalidOperationException($"Cannot select '{name}' element that as its not selectable (Set the OnPress event to make it selectable)");

                        _nextButtonSound?.Play();
                        break;
                    }
                }
            }
        }

        private void UpdateAnimatedElements(float deltaTime)
        {
            if (UpdateOnlyHovered)
            {
                if (HoveringElement == null)
                    return;

                foreach (Element element in HoveringElement.Elements)
                {
                    if (element is AnimatedElement animated)
                        animated.UpdateAnimation(deltaTime);
                }
            }
            else
            {
                foreach ((string _, ElementGroup group) in _elements)
                {
                    foreach (Element element in group.Elements)
                    {
                        if (element is AnimatedElement animated)
                            animated.UpdateAnimation(deltaTime);
                    }
                }
            }
        }

        private void OnNewCurrentSceneEvent(Event eventData, object? sender)
        {
            var data = (NewCurrentSceneEvent)eventData;
            Position = data.Scene.WorldOffset + PositionOffsetFromScene();
        }

        private static void GetInput(out NextElementDirection direction, out bool select)
        {
            select = Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsKeyPressed(KeyboardKey.Enter);
            direction = NextElementDirection.None;

            // Keyboard
            if (Raylib.IsKeyPressed(KeyboardKey.A))
                direction = NextElementDirection.West;
            else if (Raylib.IsKeyPressed(KeyboardKey.D))
                direction = NextElementDirection.East;
            else if (Raylib.IsKeyPressed(KeyboardKey.S))
                direction = NextElementDirection.South;
            else if (Raylib.IsKeyPressed(KeyboardKey.W))
                direction = NextElementDirection.North;

            // Gamepad
            if (!select && direction == NextElementDirection.None && Raylib.IsGamepadAvailable(0))
            {
                select = Raylib.IsGamepadButtonPressed(0, GamepadButton.RightFaceDown);

                if (Raylib.IsGamepadButtonPressed(0, GamepadButton.LeftFaceLeft))
                    direction = NextElementDirection.West;
                else if (Raylib.IsGamepadButtonPressed(0, GamepadButton.LeftFaceRight))
                    direction = NextElementDirection.East;
                else if (Raylib.IsGamepadButtonPressed(0, GamepadButton.LeftFaceDown))
                    direction = NextElementDirection.South;
                else if (Raylib.IsGamepadButtonPressed(0, GamepadButton.LeftFaceUp))
                    direction = NextElementDirection.North;
            }
        }

        public enum NextElementDirection
        {
            None,
            North,
            East,
            South,
            West,
        }

        public class ElementGroup
        {
            public delegate void OnPressCallback();

            public Vector2 Position { get; init; }
            public OnPressCallback? OnPress { get; init; } = null;
            public List<(NextElementDirection, string)> Next { get; init; } = [];
            public required List<Element> Elements { get; init; }

            public bool IsSelectable => OnPress != null;

            public void Draw(Vector2 actorPosition, bool isHovering)
            {
                foreach (Element element in Elements)
                    element.Draw(actorPosition + Position, isHovering);
            }
        }

        public abstract class Element
        {
            public Vector2 RelativePosition { get; init; }

            public Element(Vector2 relativePosition)
            {
                RelativePosition = relativePosition;
            }

            public abstract void Draw(Vector2 position, bool isHovering);
        }

        public class BasicElement : Element
        {
            private SpriteAtlas _atlas;
            private Vector2 _baseOffset;
            private Vector2 _hoveredOffset;
            private int _width;
            private int _height;

            public BasicElement(Vector2 relativePosition, SpriteAtlas atlas, Vector2 baseOffset, int width, int height)
                : base(relativePosition)
            {
                _atlas = atlas;
                _baseOffset = baseOffset;
                _width = width;
                _height = height;
            }

            public BasicElement(Vector2 relativePosition, SpriteAtlas atlas, Vector2 baseOffset, Vector2 hoveredOffset, int width, int height)
                : base(relativePosition)
            {
                _atlas = atlas;
                _baseOffset = baseOffset;
                _hoveredOffset = hoveredOffset;
                _width = width;
                _height = height;
            }

            public override void Draw(Vector2 position, bool isHovering)
            {
                _atlas.GridPosition = isHovering ? _hoveredOffset : _baseOffset;
                _atlas.GridWidth = _width;
                _atlas.GridHeight = _height;
                _atlas.Draw(position + RelativePosition, false, false);
            }
        }

        public class TextElement : Element
        {
            private int _size;
            private bool _center;
            private FontInstance _font;

            public string Text { get; set; }

            public TextElement(FontInstance font, Vector2 relativePosition, string text, int size = 1, bool center = true)
                : base(relativePosition)
            {
                _size = size;
                _center = center;
                Text = text;
                _font = font;
            }

            public override void Draw(Vector2 position, bool isHovering)
            {
                position += RelativePosition;
                if (_center)
                    position -= new Vector2(_font.MeasureText(Text, _size) / 2, 0.0f);

                _font.Draw(position, Text, _size);
            }
        }

        public class AnimatedElement : Element, IAnimatable
        {
            private SpriteAtlas _atlas;
            private AnimationController _animations;

            public bool AnimationPaused => _animations.Paused;
            public string CurrentAnimation => _animations.CurrentAnimation.Name;

            public AnimatedElement(Vector2 relativePosition, SpriteAtlas atlas, AnimationSet anims, string startAnimation)
                : base(relativePosition)
            {
                _atlas = atlas;
                _animations = new AnimationController(anims);
                PlayAnimation(startAnimation);
            }

            public override void Draw(Vector2 position, bool isHovering)
            {
                _animations.DrawFrame(_atlas, false, false, position + RelativePosition);
            }

            public void UpdateAnimation(float deltaTime)
            {
                _animations.Update(deltaTime);
            }

            public void PlayAnimation(string name, int startingFrame = 0)
            {
                _animations.Play(name, startingFrame);
            }

            public void PauseAnimation()
            {
                _animations.Pause();
            }

            public void ResumeAnimation()
            {
                _animations.Resume();
            }
        }
    }
}