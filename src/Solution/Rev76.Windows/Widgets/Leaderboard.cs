using Rev76.DataModels;
using Svg;
using System.Collections.Generic;
using System.Drawing;

namespace Rev76.Windows.Widgets
{
    public class LeaderboardWidget : OverlayWindow
    {
        private SVGOverlayWindow SVG = new SVGOverlayWindow();
        public LeaderboardWidget(int x, int y, int width, int height, float scale, Icon icon) : base(x, y, width, height, scale, icon)
        {
        }

        public override string Title => "Leaderboard";

        public override bool Visible { get => GameData.Instance.GameState.Status == GameStatus.LIVE; }


        protected override void OnGraphicsSetup(System.Drawing.Graphics gfx)
        {
            this.SVG.LoadSvgFiles(
               new List<string>
               {
                    "Assets/Leaderboard.svg",
               });

            base.OnGraphicsSetup(gfx);
        }

       
        protected override void OnRender(System.Drawing.Graphics gfx)
        {
            SVG.DrawSvg
            (
                gfx,
                0,
                0 , 0, 400 * Scale, 600 * Scale,
                element =>
                {

                    if (element is SvgText text)
                    {
                        switch (text.ID)
                        {
                            case "racetype":
                                text.Text = $"{GameData.Instance.Session.SessionType.ToString().ToUpper()}";
                                break;
                            case "time":
                                text.Text = GameData.FormatTimeLeft(GameData.Instance.Session.SessionTimeLeft);
                                break;
                            case "laps":
                                text.Text = $"Lap {GameData.Instance.Session.CompletedLaps}";
                                break;
                            default:
                                break;
                        }
                    }
                }
            );

            base.OnRender(gfx);
        }

       

    }
}
