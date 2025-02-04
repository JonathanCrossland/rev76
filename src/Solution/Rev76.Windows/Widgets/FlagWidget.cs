using Rev76.DataModels;
using Svg;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rev76.DataModels;

namespace Rev76.Windows.Widgets
{
    public class FlagWidget : OverlayWindow
    {
        private SVGOverlayWindow SVG = new SVGOverlayWindow();
        public FlagWidget(int x, int y, int width, int height, Icon icon) : base(x, y, width, height, icon)
        {
        }

        protected override string Title => "Flags";

        protected override bool Visible { get =>
                GameData.GameState.Status == GameStatus.LIVE
                && 
                GameData.Session.Flag != FlagType.NO_FLAG; 
        }


        protected override void OnGraphicsSetup(System.Drawing.Graphics gfx)
        {
            this.SVG.LoadSvgFiles(
               new List<string>
               {
                    "Assets/Flags.svg",
               });

            base.OnGraphicsSetup(gfx);
        }

       

        public SvgPaintServer GetFlagColor()
        {
            switch (GameData.Session.Flag)
            {
                case FlagType.NO_FLAG:
                    return new SvgColourServer(Color.Transparent);
                    break;
                case FlagType.BLUE_FLAG:
                    return new SvgColourServer(Color.Blue);
                    break;
                case FlagType.YELLOW_FLAG:
                    return new SvgColourServer(Color.Yellow);
                    break;
                case FlagType.BLACK_FLAG:
                    return new SvgColourServer(Color.Black);
                    break;
                case FlagType.WHITE_FLAG:
                    return new SvgColourServer(Color.White);
                    break;
                case FlagType.CHECKERED_FLAG:
                    return new SvgDeferredPaintServer("url(#chequeredFlag)");
                    break;
                case FlagType.PENALTY_FLAG:
                    return new SvgColourServer(Color.Red);
                case FlagType.GREEN_FLAG:
                    return new SvgColourServer(Color.Green);
                case FlagType.BLACK_FLAG_WITH_ORANGE_CIRCLE:
                    return new SvgDeferredPaintServer("url(#meatballFlag)");
                    break;
            }
            return new SvgColourServer(Color.Transparent);
        }

        protected override void OnRender(System.Drawing.Graphics gfx)
        {
            SVG.DrawSvg(
              gfx,
              this.SVG._SVG[0],
               10, 10, 310, 60,
               element =>
               {
                   if (element is SvgRectangle rect)
                   {
                       switch (element.ID)
                       {
                           case "flagrect":
                               rect.Fill = GetFlagColor();
                               break;
                            default:
                               break;
                       }
                   }
                   if (element is SvgText text)
                   {
                       switch (element.ID)
                       {
                           case "flagtext":
                               text.Text ="";
                               break;
                           default:
                               break;
                       }
                   }
               });

            base.OnRender(gfx);
        }

       

    }
}
