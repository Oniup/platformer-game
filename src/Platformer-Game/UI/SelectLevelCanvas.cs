using System.Numerics;
using PlatformerGame.Engine;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.UI
{
    public class SelectLevelCanvas : ButtonCanvas
    {
        private readonly FontInstance _infoFont;
        private readonly SpriteAtlas _starAtlas;
        private readonly AnimationSet _starAnimations;

        public SelectLevelCanvas(SpriteAtlas uiPanels, FontInstance buttonFont, FontInstance infoFont, SpriteAtlas starAtlas, AnimationSet starAnims, Vector2 position)
            : base(uiPanels, buttonFont, position)
        {
            _infoFont = infoFont;
            _starAtlas = starAtlas;
            _starAnimations = starAnims;

            (int winWidth, int winHeight) = Window.GetResolutionSize(WindowResolution.nHD);
            var startListPosition = new Vector2
            {
                X = (winWidth - SqButtonSize.X * 3) / 2,
                Y = (winHeight - SqButtonSize.Y) / 3,
            };

            SaveData saveData = SaveData.Read();
            AddLevelSelect(saveData, startListPosition, "Testing", 0, [
                (NextElementDirection.West, "Level 3"),
                (NextElementDirection.East, "Level 2"),
                (NextElementDirection.South, "Back"),
            ]);
            AddLevelSelect(saveData, startListPosition, "Level 2", 1, [
                (NextElementDirection.West, "Testing"),
                (NextElementDirection.East, "Level 3"),
                (NextElementDirection.South, "Back"),
            ]);
            AddLevelSelect(saveData, startListPosition, "Level 3", 2, [
                (NextElementDirection.West, "Testing"),
                (NextElementDirection.East, "Level 1"),
                (NextElementDirection.South, "Back"),
            ]);

            var buttonPosition = new Vector2
            {
                X = (winWidth - ButtonSize.X) / 2,
                Y = startListPosition.Y + SqButtonSize.Y
            };
            HoveringElement = AddButtonVertical(buttonPosition, 0, "Back", BackToMainMenu, [
                (NextElementDirection.North, "Testing"),
            ]);
        }

        private static Vector2 TextDisplayOffset => new Vector2(SqButtonSize.X / 2, SqButtonSize.Y / 4);
        private static Vector2 StarOffset => new Vector2(SqButtonSize.X / 4, SqButtonSize.Y / 5);

        private ElementGroup AddLevelSelect(SaveData saveData, Vector2 startListPosition, string name, int i, List<(NextElementDirection, string)> next)
        {
            string strippedName = name.Replace(" ", "");
            SaveData.Score score = saveData.GetLevelScore(strippedName);

            // string timeStr = "Time: 123.12";
            // string scoreStr = "Score: 123";
            // string hitStr = "Hits: 1";
            string timeStr = "";
            string scoreStr = "No Score";
            string hitStr = "";
            if (score.BestEntry != null)
            {
                timeStr = $"Time: {score.BestEntry.Time}";
                scoreStr = $"Score: {score.BestEntry.Score}";
                hitStr = $"Score: {score.BestEntry.Hits}";
            }

            return AddElement(name, new ElementGroup
            {
                Position = startListPosition + (Vector2.UnitX * SqButtonSize.X * i),
                Elements = [
                    new BasicElement(Vector2.Zero, UiPanels, SqButtonBaseSpriteOffset, SqButtonHoverSpriteOffset, (int)SqButtonSize.X, (int)SqButtonSize.Y),
                    new TextElement(_infoFont, TextDisplayOffset + (Vector2.UnitY * InfoFontSize * 1.5f), name, InfoFontSize * 2),

                    // Score Display
                    new TextElement(_infoFont, TextDisplayOffset + (Vector2.UnitY * InfoFontSize * 4), timeStr, InfoFontSize),
                    new TextElement(_infoFont, TextDisplayOffset + (Vector2.UnitY * InfoFontSize * 5), scoreStr, InfoFontSize),
                    new TextElement(_infoFont, TextDisplayOffset + (Vector2.UnitY * InfoFontSize * 6), hitStr, InfoFontSize),

                    // Star score
                    new AnimatedElement(StarOffset * new Vector2(1, 1), _starAtlas, _starAnimations, "Inactive"),
                    new AnimatedElement(StarOffset * new Vector2(2, 1), _starAtlas, _starAnimations, "Inactive"),
                    new AnimatedElement(StarOffset * new Vector2(3, 1), _starAtlas, _starAnimations, "Inactive"),
                ],
                Next = next,
                OnPress = () => World.Load(strippedName),
            });
        }

        private void BackToMainMenu()
        {
            var mainMenu = World.Find<MainMenuCanvas>().First();
            mainMenu.Showing = true;
            Showing = false;
        }

        public class CreateInfo : CreateInfo<SelectLevelCanvas>
        {
            public override Actor Instantiate(ResourceManager resources, SpawnInfo info)
            {
                var panels = resources.Get<SpriteAtlas>("UI Panels");
                var buttonFont = resources.Get<FontInstance>("UI Panels Button Font");
                var infoFont = resources.Get<FontInstance>("UI Panels Info Font");
                var starAtlas = resources.Get<SpriteAtlas>("UI Star 32");
                var starAnims = resources.Get<AnimationSet>("UI Star 32 Animations");
                return new SelectLevelCanvas(panels, buttonFont, infoFont, starAtlas, starAnims, info.Position);
            }
        }
    }
}