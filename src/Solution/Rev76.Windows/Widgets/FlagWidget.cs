using Rev76.DataModels;
using Svg;
using System.Collections.Generic;
using System.Drawing;

namespace Rev76.Windows.Widgets
{
    public class FlagWidget : OverlayWindow
    {
        private SVGOverlayWindow SVG = new SVGOverlayWindow();
        public FlagWidget(int x, int y, int width, int height, Icon icon) : base(x, y, width, height, icon)
        {
        }

        public override string Title => "Flags";

        public override bool Visible { get =>
                GameData.Instance.GameState.Status == GameStatus.LIVE;
                //&& 
                //GameData.Instance.Session.Flag != FlagType.NO_FLAG; 
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

            SvgPaintServer ret = null;

            switch (GameData.Instance.Session.Flag)
            {
                case FlagType.NO_FLAG:
                    ret = new SvgColourServer(Color.Transparent);
                    break;
                case FlagType.BLUE_FLAG:
                    ret = new SvgColourServer(Color.Blue);
                    break;
                case FlagType.YELLOW_FLAG:
                    ret = new SvgColourServer(Color.Yellow);
                    break;
                case FlagType.BLACK_FLAG:
                    ret = new SvgColourServer(Color.Black);
                    break;
                case FlagType.WHITE_FLAG:
                    ret = new SvgColourServer(Color.White);
                    break;
                case FlagType.CHECKERED_FLAG:
                    ret = new SvgDeferredPaintServer("url(#chequeredFlag)");
                    break;
                case FlagType.PENALTY_FLAG:
                    ret = new SvgColourServer(Color.Red);
                    break;
                case FlagType.GREEN_FLAG:
                    ret = new SvgColourServer(Color.LightGreen);
                    break;
                case FlagType.BLACK_FLAG_WITH_ORANGE_CIRCLE:
                    ret = new SvgDeferredPaintServer("url(#meatballFlag)");
                    break;
            }


            if (GameData.Instance.Session.GlobalGreen == 1)
            {
                ret = new SvgColourServer(Color.LightGreen);
            }
            if (GameData.Instance.Session.GlobalYellow == 1)
            {
                ret = new SvgColourServer(Color.Yellow);
            }
            if (GameData.Instance.Session.GlobalWhite == 1)
            {
                ret = new SvgColourServer(Color.White);
            }
            if (GameData.Instance.Session.GlobalRed == 1)
            {
                ret = new SvgColourServer(Color.Red);
            }
            if (GameData.Instance.Session.GlobalChequered == 1)
            {
                ret = new SvgDeferredPaintServer("url(#chequeredFlag)");
            }

            return ret;

        }

        protected override void OnRender(System.Drawing.Graphics gfx)
        {
            SVG.DrawSvg(
              gfx,
              0,
               10, 10, 310, 60,
               element =>
               {
                   if (element is SvgRectangle rect)
                   {
                       switch (element.ID)
                       {
                           case "flagrect1":
                               rect.Fill = GetFlagColor();
                               break;
                           case "flagrect2":
                               rect.Fill = GetFlagColor();
                               break;
                           case "flagrect3":
                               rect.Fill = GetFlagColor();
                               break;
                           default:
                               break;
                       }
                   }

                   if (element is SvgText text)
                   {
                       text.Fill = new SvgColourServer(Color.Transparent);
                       switch (element.ID)
                       {
                           case "flagtext1":
                               if (GameData.Instance.Session.FlagSector1 == 1) text.Fill = new SvgColourServer(Color.Black);
                               break;
                           case "flagtext2":
                               if (GameData.Instance.Session.FlagSector2 == 1) text.Fill = new SvgColourServer(Color.Black);
                               break;
                           case "flagtext3":
                               if (GameData.Instance.Session.FlagSector3 == 1) text.Fill = new SvgColourServer(Color.Black);
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
