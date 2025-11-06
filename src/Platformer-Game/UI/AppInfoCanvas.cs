using System.Numerics;
using PlatformerGame.Engine;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Level.UI;
using PlatformerGame.Engine.Resources;

namespace PlatformerGame.UI
{
    public class AppInfoCanvas : Canvas
    {
        public AppInfoCanvas(FontInstance font, Vector2 position)
            : base(position)
        {
            Showing = true;

            (int winWidth, int winHeight) = Window.GetResolutionSize(WindowResolution.nHD);
            string author = "Ewan Robson (ID: 103992579)";
            string unitModule = "COS20007: OOP Custom Program";
            float authorLength = font.MeasureText(author);
            float unitModuleLength = font.MeasureText(unitModule);

            AddElement("App info", new ElementGroup
            {
                Position = new Vector2(winWidth - font.Size, winHeight - font.Size * 3),
                Elements = [
                    new TextElement(font, new Vector2(-authorLength, 0.0f), author, 1, false),
                    new TextElement(font, new Vector2(-unitModuleLength, font.Size), unitModule, 1, false),
                ],
            });
        }

        public class CreateInfo : CreateInfo<AppInfoCanvas>
        {
            public override Actor Instantiate(ResourceRegistry resources, SpawnInfo info)
            {
                var infoFont = resources.Get<FontInstance>(ButtonCanvas.InfoFontResourceName);
                return new AppInfoCanvas(infoFont, info.Position);
            }
        }
    }
}