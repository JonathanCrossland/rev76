using ExCSS;
using Rev76.DataModels;
using Svg;

using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Rev76.Windows.Widgets
{
    public class LeaderboardWidget : OverlayWindow
    {
        private SVGOverlayWindow SVG = new SVGOverlayWindow();
        private bool _DriversAdded = false;
        public LeaderboardWidget(int x, int y, int width, int height, float scale, Icon icon) : base(x, y, width, height, scale, icon)
        {
        }

        public override string Title => "Leaderboard";

        public override bool Visible { get => GameData.Instance.GameState.Status == GameStatus.LIVE; }


     

       
        protected override void OnRender(System.Drawing.Graphics gfx)
        {
            try
            {

           
                SvgGroup template = null;

                SVG.DrawSvg
                (
                    gfx,
                    0,
                    0 , 0, 450 * Scale, 650 * Scale,
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


                        if (element.ID == "template")
                        {
                            template = element.Clone();
                        }

                        if (element.ID == "DriverRows")
                        {
                            InitDrivers(element, template);
                        }

                    }
                );

          

            }
            catch (System.Exception ex)
            {

                Trace.WriteLine($"Leaderboard: {ex.Message}");
            }
            finally
            {
                base.OnRender(gfx);
            }

            
        }

        private void InitDrivers(SvgGroup parentGroup, SvgGroup template)
        {
            if (template == null) return;

            if (parentGroup.ID == "DriverRows" && GameData.Instance.Track.Cars.Count() > 1 && _DriversAdded == false)
            {
                List<Car> cars = GameData.Instance.Track.Cars.OrderBy(car => car.Position).ToList<Car>();

                for (int i = 0; i < GameData.Instance.Track.Cars.Count; i++)
                {
                    Car car = GameData.Instance.Track.Cars[i];

                    SvgGroup row = template.Clone() as SvgGroup;
                    row.Visibility = "visible";
                    row.Transforms = new Svg.Transforms.SvgTransformCollection
                    {
                        new Svg.Transforms.SvgTranslate(0, i * template.Bounds.Height + 1)
                    };

                    SvgText pos = (row as SvgGroup).Children.FirstOrDefault(c => c.ID == "pos") as SvgText;
                    pos.Text = car.Position.ToString();
                    SvgText drivername = (row as SvgGroup).Children.FirstOrDefault(c => c.ID == "drivername") as SvgText;
                    var driver = car.Drivers[car.DriverIndex];
                    drivername.Text = $"{driver.FirstName[0].ToString().ToUpper()} {driver.LastName}";

                    SvgText number = (row as SvgGroup).Children.FirstOrDefault(c => c.ID == "number") as SvgText;
                    number.Text = car.Number.ToString();

                    SvgText lastlaptime = (row as SvgGroup).Children.FirstOrDefault(c => c.ID == "lastlaptime") as SvgText;
                    float time = float.TryParse(car.BestSessionLap?.LaptimeMS.ToString(), out time) ? time : 0;
                    lastlaptime.Text = GameData.GetFormattedLapTime(time);

                    if (GameData.Instance.Track.Cars.Count() < i + 1)
                    {
                        SvgText offsettime = (row as SvgGroup).Children.FirstOrDefault(c => c.ID == "offsettime") as SvgText;

                        float postcarGap = Car.CalculateTimeGap(car, GameData.Instance.Track.Cars[i + 1], GameData.Instance.Track.TrackLength);
                        offsettime.Text = GameData.GetFormattedGap(postcarGap);
                    }
                    (parentGroup as SvgGroup).Children.Add(row);

                }
                _DriversAdded = true;
            }
        }

        protected override void OnGraphicsSetup(System.Drawing.Graphics gfx)
        {
            this.SVG.LoadSvgFiles(
               new List<string>
               {
                    "Assets/Leaderboard.svg",
               });

            base.OnGraphicsSetup(gfx);
        }



    }
}
