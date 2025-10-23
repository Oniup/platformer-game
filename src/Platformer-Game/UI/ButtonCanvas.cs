using System.Numerics;
using PlatformerGame.Engine;
using PlatformerGame.Engine.Level.UI;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.UI
{
    public abstract class ButtonCanvas : Canvas
    {
        public ButtonCanvas(SpriteAtlas uiPanels, FontInstance buttonFont, Vector2 position)
            : base(position)
        {
            UiPanels = uiPanels;
            ButtonFont = buttonFont;
        }

        protected SpriteAtlas UiPanels { get; init; }
        protected FontInstance ButtonFont { get; init; }

        protected static Vector2 ButtonBaseSpriteOffset => new Vector2(0, 3) * 16;
        protected static Vector2 ButtonHoveredSpriteOffset => Vector2.Zero;
        protected static Vector2 ButtonSize => new Vector2(19, 3) * 16;

        protected static int ButtonFontSize => 30;
        protected static Vector2 ButtonCenter => ButtonSize / 2 - Vector2.UnitY * (ButtonFontSize / 2);
        protected static int InfoFontSize => 8;

        protected static Vector2 SqButtonBaseSpriteOffset => new Vector2(6, 6) * 16;
        protected static Vector2 SqButtonHoverSpriteOffset => new Vector2(0, 6) * 16;

        protected static Vector2 SqButtonSize => new Vector2(6, 6) * 16;

        public override Vector2 Center
        {
            get
            {
                (int winWidth, int winHeight) = Window.GetResolutionSize(WindowResolution.nHD);
                return new Vector2
                {
                    X = (winWidth - ButtonSize.X) / 2,
                    Y = (winHeight - ButtonSize.Y) / 2,
                };
            }
        }

        public ElementGroup AddButtonVertical(Vector2 listStartPosition, int index, string text, ElementGroup.OnPressCallback callback, List<(NextElementDirection, string)> next)
        {
            return AddButtonVertical(listStartPosition, index, text, text, callback, next);
        }

        public ElementGroup AddButtonVertical(Vector2 listStartPosition, int index, string name, string text, ElementGroup.OnPressCallback callback, List<(NextElementDirection, string)> next)
        {
            return AddElement(name, new ElementGroup
            {
                Position = listStartPosition + (Vector2.UnitY * ButtonSize.Y * index),
                Elements = [
                    new BasicElement(Vector2.Zero, UiPanels, ButtonBaseSpriteOffset, ButtonHoveredSpriteOffset, (int)ButtonSize.X, (int)ButtonSize.Y),
                    new TextElement(ButtonFont, ButtonCenter, text, ButtonFontSize),
                ],
                Next = next,
                OnPress = callback
            });
        }
    }
}