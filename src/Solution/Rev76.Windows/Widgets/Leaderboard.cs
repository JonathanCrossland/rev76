using ExCSS;
using Rev76.DataModels;
using Svg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Rev76.Windows.Widgets
{
    public class LeaderboardWidget : OverlayWindow
    {
        private SVGOverlayWindow SVG = new SVGOverlayWindow();
        private bool _DriversAdded = false;

        private bool _InRender = false;
        public LeaderboardWidget(int x, int y, int width, int height, float scale, Icon icon) : base(x, y, width, height, scale, icon)
        {
        }

        public override string Title => "Leaderboard";

        public override bool Visible { get => GameData.Instance.GameState.Status == GameStatus.LIVE; }


     

       
        protected override void OnRender(System.Drawing.Graphics gfx)
        {
            try
            {
                if (_InRender == true) return;

                _InRender = true;
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
                                    
                                    if (GameData.Instance.Session.Flag == FlagType.CHECKERED_FLAG)
                                    {
                                        text.Text = GameData.FormatTimeLeft(GameData.Instance.Session.SessionTimeLeft);
                                    }
                                    //Random rnd = new Random();
                                    //text.Fill = new SvgColourServer(System.Drawing.Color.FromArgb(255, rnd.Next(256), rnd.Next(256), rnd.Next(256)));

                                    break;
                                case "laps":
                                    text.Text = $"Lap {GameData.Instance.Session.CompletedLaps}";
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (element is SvgRectangle rect)
                        {
                            if (rect.ID == "flag")
                            {
                                if (GameData.Instance.Session.GlobalChequered == 1)
                                {
                                    rect.Fill = new SvgDeferredPaintServer("url(#chequeredFlag)");
                                    rect.Visibility = "visible";
                                }
                                else
                                {
                                    rect.Visibility = "hidden";
                                }
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

                        if (element.ID != null && element.ID.StartsWith("car_"))
                        {
                            var positionString = (element as SvgGroup).ID.Replace("car_", "");
                            int.TryParse(positionString, out int position);

                            Car car = GameData.Instance.Track.Cars.Find(c => c.Position == position);
                            if (car != null)
                            {
                                Car postCar = GameData.Instance.Track.Cars.Find(c => c.Position == position - 1);

                                SvgGroup row = element;

                                SvgText pos = (row as SvgGroup).Children
                                 .OfType<SvgText>()
                                 .FirstOrDefault(t => t.CustomAttributes.TryGetValue("class", out var value) && value == "pos");

                                pos.Text = car.Position.ToString();

                                if (car.CarIndex == GameData.Instance.PlayerCarIndex || car.CarIndex == GameData.Instance.BroadcastCar.CarIndex)
                                {
                                    pos.Fill = new SvgColourServer(System.Drawing.Color.FromArgb(255, 255, 255, 255));
                                }
                                else if (car.CarIndex == GameData.Instance.Session.BestSession?.CarIndex)
                                {
                                    pos.Fill = new SvgColourServer(System.Drawing.Color.FromArgb(255, 255, 255, 236));
                                }
                                else
                                {
                                    pos.Fill = new SvgColourServer(System.Drawing.Color.Black);
                                }

                                SvgRectangle posRect = (row as SvgGroup).Children
                                   .OfType<SvgRectangle>()
                                   .FirstOrDefault(t => t.CustomAttributes.TryGetValue("class", out var value) && value == "posrect");

                                if (car.CarIndex == GameData.Instance.PlayerCarIndex || car.CarIndex == GameData.Instance.BroadcastCar.CarIndex)
                                {

                                    posRect.Fill = new SvgColourServer(System.Drawing.Color.Red);
                                }
                                else if (car.CarIndex == GameData.Instance.Session.BestSession?.CarIndex)
                                {
                                    posRect.Fill = new SvgColourServer(System.Drawing.Color.FromArgb(255, 189, 0, 236));
                                   
                                }
                                else
                                {
                                    posRect.Fill = new SvgColourServer(System.Drawing.Color.White);
                                }

                                SvgText drivername = (row as SvgGroup).Children
                                    .OfType<SvgText>()
                                    .FirstOrDefault(t => t.CustomAttributes.TryGetValue("class", out var value) && value == "drivername");

                                var driver = car.Drivers[car.DriverIndex];
                                drivername.Text = $"{driver.FirstName[0].ToString().ToUpper()} {driver.LastName}";


                                SvgText number = (row as SvgGroup).Children
                                    .OfType<SvgText>()
                                    .FirstOrDefault(t => t.CustomAttributes.TryGetValue("class", out var value) && value == "number");

                                number.Text = car.Number.ToString();


                                SvgText lastlaptime = (row as SvgGroup).Children
                                    .OfType<SvgText>()
                                    .FirstOrDefault(t => t.CustomAttributes.TryGetValue("class", out var value) && value == "lastlaptime");

                                float time = float.TryParse(car.LastLap?.LaptimeMS.ToString(), out time) ? time : 0;
                                lastlaptime.Text = GameData.GetFormattedLapTime(time);

                                if (postCar != null)
                                {

                                    SvgText offsettime = (row as SvgGroup).Children
                                        .OfType<SvgText>()
                                        .FirstOrDefault(t => t.CustomAttributes.TryGetValue("class", out var value) && value == "offsettime");

                                    offsettime.Text = "";
                                    if (postCar.Laps < car.Laps)
                                    {
                                        offsettime.Text = $"+{postCar.Laps - car.Laps} Laps";
                                    }
                                    else
                                    {
                                        float postcarGap = Car.CalculateTimeGap(postCar, car, GameData.Instance.Track.TrackLength);
                                        offsettime.Text = $"+{GameData.GetFormattedGap(postcarGap)}";
                                    }
                                }

                                SvgGroup pit = (row as SvgGroup).Children
                                   .OfType<SvgGroup>()
                                   .FirstOrDefault(t => t.CustomAttributes.TryGetValue("class", out var value) && value == "pit");
                                
                               
                                  pit.Visibility = car.InPits ? "visible" : "hidden";
                                                               
                            }
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
                _InRender = false;
                Random rnd = new Random();
               
               // _Brushes["background"] = new SolidBrush(System.Drawing.Color.FromArgb(100, rnd.Next(256), rnd.Next(256), rnd.Next(256)));


            }


        }

        private void InitDrivers(SvgGroup parentGroup, SvgGroup template)
        {
            if (template == null) return;

            if (GameData.Instance.Track.Cars.Count() != GameData.Instance.Track.NumberOfCars)
            {
                parentGroup.Children.Clear();
                _DriversAdded = false;

            }

            if (parentGroup.ID == "DriverRows" && GameData.Instance.Track.Cars.Count() > 1 && _DriversAdded == false)
            {
                List<Car> cars = GameData.Instance.Track.Cars.OrderBy(car => car.Position).ToList<Car>();

                for (int i = 0; i < GameData.Instance.Track.Cars.Count; i++)
                {
                    Car car = GameData.Instance.Track.Cars[i];

                    SvgGroup row = template.Clone() as SvgGroup;
                    row.Visibility = "visible";
                    row.ID = $"car_{(i+1).ToString()}";
                    
                    row.Transforms = new Svg.Transforms.SvgTransformCollection
                    {
                        new Svg.Transforms.SvgTranslate(0, i * template.Bounds.Height + 1)
                    };

                   
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
