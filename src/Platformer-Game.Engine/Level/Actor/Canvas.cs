using System.Numerics;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Utilities;
using Raylib_cs;

namespace PlatformerGame.Engine.Level.UI
{
    public abstract class Canvas : Actor
    {
        private OrderedDictionary<string, ElementGroup> _elements;

        protected ElementGroup HoveringElement { get; set; } = null!;
        public bool Showing { get; set; }

        public Canvas(Vector2 position)
            : base(position)
        {
            _elements = new OrderedDictionary<string, ElementGroup>();
            Showing = false;
        }

        public override void OnUpdate(float deltaTime)
        {
            if (!Showing)
                return;

            GetInput(out NextElementDirection direction, out bool select);
            if (select)
            {
                HoveringElement.OnPress();
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
                        break;
                    }
                }
            }

            // Make sure to call the update animation for animated elements
            foreach (Element element in HoveringElement.Elements)
            {
                if (element is AnimatedElement animated)
                    animated.UpdateAnimation(deltaTime);
            }
        }

        public override void OnDraw()
        {
            if (!Showing)
                return;

            foreach ((_, ElementGroup element) in _elements)
                element.Draw(HoveringElement == element);
        }

        protected void AddElement(string name, ElementGroup element)
        {
            _elements.Add(name, element);
            if (_elements.Count == 1)
                HoveringElement = _elements.First().Value;
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
            public required List<Element> Elements { get; init; }
            public required List<(NextElementDirection, string)> Next { get; init; }
            public required OnPressCallback OnPress { get; init; }

            public bool IsSelectable => OnPress != null;

            public void Draw(bool isHovering)
            {
                foreach (Element element in Elements)
                    element.Draw(Position, isHovering);
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

            public string Text { get; set; }

            public TextElement(Vector2 relativePosition, string text, int size)
                : base(relativePosition)
            {
                Text = text;
                _size = size;
            }

            public override void Draw(Vector2 position, bool isHovering)
            {
                position += RelativePosition - new Vector2(Raylib.MeasureText(Text, _size) / 2, 0.0f);
                Raylib.DrawText(Text, (int)position.X, (int)position.Y, _size, Color.Black);
            }
        }

        public class AnimatedElement : Element, IAnimatable
        {
            private SpriteAtlas _atlas;
            private AnimationController _animtions;

            public bool AnimationPaused => _animtions.Paused;
            public string CurrentAnimation => _animtions.CurrentAnimation.Name;

            public AnimatedElement(Vector2 relativePosition, SpriteAtlas atlas, AnimationSet anims, string startAnimation)
                : base(relativePosition)
            {
                _atlas = atlas;
                _animtions = new AnimationController(anims);
                PlayAnimation(startAnimation);
            }

            public override void Draw(Vector2 position, bool isHovering)
            {
                if (isHovering)
                    ResumeAnimation();
                else
                    PauseAnimation();
                _animtions.DrawFrame(_atlas, false, false, position + RelativePosition);
            }

            public void UpdateAnimation(float deltaTime)
            {
                _animtions.Update(deltaTime);
            }

            public void PlayAnimation(string name, int startingFrame = 0)
            {
                _animtions.Play(name, startingFrame);
            }

            public void PauseAnimation()
            {
                _animtions.Pause();
            }

            public void ResumeAnimation()
            {
                _animtions.Resume();
            }
        }
    }
}