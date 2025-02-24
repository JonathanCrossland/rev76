using Assetto.Data.Broadcasting;
using Assetto.Data.Broadcasting.Structs;
using ExCSS;
using Rev76.DataModels;
using Svg;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using Color = System.Drawing.Color;

namespace Rev76.Windows.Widgets
{
    public class LeaderboardWidget : OverlayWindow
    {
        private SVGRenderer SVG = new SVGRenderer();
        private bool _DriversAdded = false;

        private bool _InRender = false;

        List<Car> carList = null;


        public LeaderboardWidget(int x, int y, int width, int height, float scale, Icon icon) : base(x, y, width, height, scale, icon)
        {
        }

        public override string Title => "Leaderboard";

        public override bool Visible { get => GameData.Snapshot.GameState.Status == GameStatus.LIVE; }

       
        protected override void OnRender(System.Drawing.Graphics gfx)
        {
            try
            {
                if (_InRender == true) return;

                _InRender = true;

                //if (carList == null || carList.Count() == 0) // TODO: ADD timer to only do this every so often as its a performance hit.
                    carList = new ConcurrentBag<Car>(GameData.Snapshot.Track.Cars.Values).ToList();


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
                                    text.Text = $"{GameData.Snapshot.Session.SessionType.ToString().ToUpper()}";
                                    break;
                                case "time":
                                    text.Text = GameData.FormatTimeLeft(GameData.Snapshot.Session.SessionTimeLeft);
                                    
                                    if (GameData.Snapshot.Session.Flag == FlagType.CHECKERED_FLAG)
                                    {
                                        text.Text = GameData.FormatTimeLeft(GameData.Snapshot.Session.SessionTimeLeft);
                                    }
                                 
                                    break;
                                case "laps":
                                    text.Text = $"Lap {GameData.Snapshot.Session.CompletedLaps}";
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (element is SvgRectangle rect)
                        {
                            if (rect.ID == "flag")
                            {
                                if (GameData.Snapshot.Session.GlobalChequered == 1)
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
                          
                            Car car = carList.Find(c => c.Position == position);
                            if (car != null)
                            {
                                Car postCar = carList.Find(c => c.Position == position - 1);

                                SvgGroup row = element;

                                SvgText pos = (row as SvgGroup).Children
                                 .OfType<SvgText>()
                                 .FirstOrDefault(t => t.CustomAttributes.TryGetValue("class", out var value) && value == "pos");

                                pos.Text = car.Position.ToString();

                                if (car.CarIndex == GameData.Snapshot.PlayerCarIndex || car.CarIndex == GameData.Snapshot.BroadcastCar.CarIndex)
                                {
                                    pos.Fill = new SvgColourServer(System.Drawing.Color.FromArgb(255, 255, 255, 255));
                                }
                                else if (car.CarIndex == GameData.Snapshot.Session.BestSession?.CarIndex)
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

                                if (car.CarIndex == GameData.Snapshot.PlayerCarIndex || car.CarIndex == GameData.Snapshot.BroadcastCar.CarIndex)
                                {

                                    posRect.Fill = new SvgColourServer(System.Drawing.Color.Red);
                                }
                                else if (car.CarIndex == GameData.Snapshot.Session.BestSession?.CarIndex)
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

                                if (car.Drivers.TryGetValue(car.DriverIndex, out DriverInfo driver))
                                {

                                    drivername.Text = $"{driver.FirstName[0].ToString().ToUpper()} {driver.LastName}";

                                }

                                SvgRectangle numberRect = (row as SvgGroup).Children
                                 .OfType<SvgRectangle>()
                                 .FirstOrDefault(t => t.CustomAttributes.TryGetValue("class", out var value) && value == "numberrect");

                                numberRect.Fill = GetFillForDriverLicense(car.Drivers[car.DriverIndex].Category);

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
                                    if (postCar.Laps > car.Laps)
                                    {
                                        offsettime.Text = $"+{postCar.Laps - car.Laps} Laps";
                                        offsettime.Fill = new SvgColourServer(System.Drawing.Color.Orange);
                                    }
                                    else
                                    {
                                        float postcarGap = Car.CalculateTimeGap(postCar, car, GameData.Snapshot.Track.TrackLength);
                                        if (postcarGap > 0)
                                        {
                                            offsettime.Text = $"+{GameData.GetFormattedGap(postcarGap)}";
                                        }
                                        offsettime.Fill = new SvgColourServer(System.Drawing.Color.LightYellow);
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

        private SvgPaintServer GetFillForDriverLicense(DriverCategory category)
        {
            switch (category)
            {
                case DriverCategory.Platinum:
                    return new SvgColourServer(System.Drawing.Color.Silver);
                    
                case DriverCategory.Gold:
                    return new SvgColourServer(System.Drawing.Color.Gold);
                    
                case DriverCategory.Silver:
                    return new SvgColourServer(System.Drawing.Color.Silver);
                    udimen
                case DriverCategory.Bronze:
                    return new SvgColourServer(System.Drawing.Color.SaddleBrown);
                    
                case DriverCategory.Error:
                    break;
                default:
                    break;
            }
            return new SvgColourServer(System.Drawing.Color.White);
        }

        private SvgPaintServer GetFillForDriverClass(CarClass carClass)
        {
            switch (carClass)
            {
                case CarClass.GT3:
                    new SvgColourServer(System.Drawing.Color.White);
                    break;
                case CarClass.GT4:
                    new SvgColourServer(System.Drawing.Color.White);
                    break;
                case CarClass.CUP:
                    new SvgColourServer(System.Drawing.Color.White);
                    break;
                case CarClass.ST:
                    new SvgColourServer(System.Drawing.Color.White);
                    break;
                default:
                    break;
            }

            return new SvgColourServer(System.Drawing.Color.White);
        }

        private void InitDrivers(SvgGroup parentGroup, SvgGroup template)
        {
            if (template == null) return;

            if (GameData.Snapshot.Track.Cars.Count() <= 0 ) return; 

            if (GameData.Snapshot.Track.Cars.Count() != GameData.Snapshot.Track.NumberOfCars)
            {
                parentGroup.Children.Clear();
                _DriversAdded = false;

            }

            if (parentGroup.ID == "DriverRows" && GameData.Snapshot.Track.Cars.Count() > 1 && _DriversAdded == false)
            {
                ConcurrentBag<Car> carList = new ConcurrentBag<Car>(GameData.Snapshot.Track.Cars.Values);
                List<Car> cars = carList.OrderBy(car => car.Position).ToList<Car>();

                for (int i = 0; i < GameData.Snapshot.Track.Cars.Count; i++)
                {
                    Car car = cars[i];
                    

                    SvgGroup row = template.Clone() as SvgGroup;
                    row.Visibility = "visible";
                    row.ID = $"car_{(i + 1).ToString()}";

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
