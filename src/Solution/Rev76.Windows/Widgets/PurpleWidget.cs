using Assetto.Data.Broadcasting.Structs;
using Rev76.DataModels;
using Rev76.Windows.Helpers;
using Rev76.Windows.Rendering;
using Svg;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using static Rev76.DataModels.Tyres;

namespace Rev76.Windows.Widgets
{
    public class PurpleWidget : OverlayWindow
    {
        //private SVGRenderer _renderer = new SVGRenderer();
        private DirectXRenderer _renderer = new DirectXRenderer();
        private Stopwatch _Stopwatch = new Stopwatch();
        private int _StateChangeInterval = 4500;
        private DrawState _DrawState = DrawState.State1;

       
        private enum DrawState
        {
            State1 = 0,
            State2 = 1
        }

        public PurpleWidget(int x, int y, int width, int height, float scale, Icon icon=null) : base(x, y, width, height, scale, icon)
        {
        }

        public override string Title { get => "Purple"; }
        public override bool Visible { get => 
                GameData.Snapshot.GameState.Status == GameStatus.LIVE
                &&
                GameData.Snapshot.GameState.IsSetupMenuVisible == false;
        }
      

        private bool DrawDriver(System.Drawing.Graphics g)
        {
            ConcurrentBag<Car> cars = new ConcurrentBag<Car>(GameData.Snapshot.Track.Cars.Values);

            Car meCar = GameData.Snapshot.MeCar;
            DriverInfo driver = GetMeDriver(meCar);
            Car purpleCar = GetPurpleCar(cars);
            DriverInfo purpleDriver = GetPurpleDriver(purpleCar);
            Car preCar = GetPreCar(meCar, cars);
            DriverInfo preDriver = GetPreDriver(preCar);
            Car postCar = GetPostCar(meCar, cars);
            DriverInfo postDriver = GetPostDriver(postCar);


            Trace.WriteLine($"PreCar: {preCar?.Number}");
            Trace.WriteLine($"PostCar: {preCar?.Number}");

            AdjustMeCarPositionBasedOnPreCar(meCar, preCar, postCar);

            float time = 0;
            string formattedTime = string.Empty;
            string numberText = "";

            float preCarGap = 0;
            float postCarGap = 0;
            List<LapInfo> preCarLaps = new List<LapInfo>();
            List<LapInfo> postCarLaps = new List<LapInfo>();
            List<LapInfo> meCarLaps = new List<LapInfo>();

            if (meCar != null)
            {
                meCarLaps = GetLastLapTimes(meCar);
            }

            StateTimer();


            if (meCar != null && preCar != null)
            {
                preCarGap = Car.CalculateTimeGap(preCar, meCar, GameData.Snapshot.Track.TrackLength);

                if (preCar != null)
                {
                    preCarLaps = GetLastLapTimes(preCar);
                }
            }

            if (meCar != null && postCar != null)
            {
                postCarGap = Car.CalculateTimeGap(meCar, postCar, GameData.Snapshot.Track.TrackLength);

                if (postCar != null)
                {
                    postCarLaps = GetLastLapTimes(postCar);
                }
            }

           

            _renderer.DrawSvg(
             g,
             0,
             5, 5, 435 * Scale, 230 * Scale,
             element =>
             {


                 if (element is SvgRectangle rect)
                 {
                     if (element.ID.StartsWith("PreDriverRect"))
                     {
                         DrawLap(preCar, preCarLaps, rect);
                     }
                     else if (element.ID.StartsWith("MeDriverRect"))
                     {
                         DrawLap(meCar, meCarLaps, rect);
                     }
                     else if (element.ID.StartsWith("PostDriverRect"))
                     {
                         DrawLap(postCar, postCarLaps, rect);
                     }

                     switch (rect.ID)
                     {

                         case "PreDriverRect":
                             if (preCar == null) return false;
                             if (preCar?.Delta > 0)
                             {
                                 rect.Fill = new SvgDeferredPaintServer("deltared"); // new SvgColourServer(Color.FromArgb(255, 240, 0, 0));
                             }
                             else if (preCar?.Delta < 0)
                             {
                                 rect.Fill = new SvgDeferredPaintServer("deltagreen"); // new SvgColourServer(Color.FromArgb(255, 0, 200, 0));
                             }
                             if (preCar?.CurrentLap?.IsInvalid == true)
                             {
                                 rect.StrokeWidth = 2;
                                 rect.Stroke = new SvgColourServer(Color.FromArgb(255, 249, 131, 4));
                             }
                             if (preCar?.Flag == FlagType.YELLOW_FLAG)
                             {
                                 if (preCar?.Kmh < 40 || preCar?.Gear <= 0)
                                 {
                                     rect.StrokeWidth = 2;
                                     rect.Stroke = new SvgColourServer(Color.FromArgb(255, 255, 255, 0));
                                 }
                             }

                             break;

                         case "MeDriverRect":
                             if (meCar?.Delta > 0)
                             {
                                 rect.Fill = new SvgDeferredPaintServer("medeltared"); //new SvgColourServer(Color.FromArgb(255, 240, 0, 0));
                             }
                             else if (meCar?.Delta < 0)
                             {
                                 rect.Fill = new SvgDeferredPaintServer("medeltagreen"); //new SvgColourServer(Color.FromArgb(255, 0, 200, 0));
                             }
                             if (meCar?.CurrentLap?.IsInvalid == true)
                             {
                                 rect.StrokeWidth = 1;
                                 rect.Stroke = new SvgColourServer(Color.FromArgb(255, 249, 131, 4));
                             }
                             if (meCar?.Flag == FlagType.BLUE_FLAG)
                             {
                                 rect.StrokeWidth = 2;
                                 rect.Stroke = new SvgColourServer(Color.FromArgb(255, 0, 0, 255));
                             }
                             if (meCar?.Flag == FlagType.YELLOW_FLAG)
                             {
                                 if (meCar?.Kmh < 40 || meCar?.Gear <= 0)
                                 {
                                     rect.StrokeWidth = 2;
                                     rect.Stroke = new SvgColourServer(Color.FromArgb(255, 255, 255, 0));
                                 }
                             }
                             break;

                         case "PostDriverRect":
                             if (postCar == null) return false;
                             if (postCar?.Delta > 0)
                             {
                                 rect.Fill = new SvgDeferredPaintServer("deltared"); //new SvgColourServer(Color.FromArgb(255, 240, 0, 0));
                             }
                             else if (postCar?.Delta < 0)
                             {
                                 rect.Fill = new SvgDeferredPaintServer("deltagreen"); //new SvgColourServer(Color.FromArgb(255, 0, 200, 0));
                             }
                             if (postCar?.CurrentLap?.IsInvalid == true)
                             {
                                 rect.StrokeWidth = 2;
                                 rect.Stroke = new SvgColourServer(Color.FromArgb(255, 249, 131, 4));
                             }
                             if (postCar?.Flag == FlagType.YELLOW_FLAG)
                             {
                                 if (postCar?.Kmh < 40 || postCar?.Gear <= 0)
                                 {
                                     rect.StrokeWidth = 2;
                                     rect.Stroke = new SvgColourServer(Color.FromArgb(255, 255, 255, 0));
                                 }
                             }
                             break;

                     }
                 }


                 if (element is SvgText el)
                 {
                     el.Text = "-";
                     switch (el.ID)
                     {
                         case "PreTimeDiff":
                             el.Text = "+ " + GameData.GetFormattedGap(preCarGap);
                             break;

                         case "PostTimeDiff":
                             el.Text = "- " + GameData.GetFormattedGap(postCarGap);
                             break;

                         case "PreDriverPos":
                             el.Text = preCar?.Position.ToString();
                             break;
                         case "PreDriver":
                             time = float.TryParse(preCar?.BestSessionLap?.LaptimeMS.ToString(), out time) ? time : 0;

                             if (_DrawState == DrawState.State1 || time == 0)
                             {
                                
                                 if (preCar != null)
                                 {
                                     numberText = preCar?.Number.ToString() + "";
                                     if (numberText.Length > 0) numberText = "#" + numberText;
                                     var fname = preDriver.FirstName.Length >= 1 ? preDriver.FirstName[0].ToString().ToUpper() : "";

                                     el.Text = $"{fname} {preDriver.LastName} {numberText}";
                                     el.Fill = new SvgColourServer(Color.FromArgb(255, 255, 255, 255));
                                 }
                             }
                             else if (_DrawState == DrawState.State2)
                             {

                                 formattedTime = GameData.GetFormattedLapTime(time);
                                 el.Text = formattedTime;
                                 el.Fill = new SvgColourServer(Color.FromArgb(255, 188, 100, 205));
                             }
                             break;
                         case "PreDriverTime":
                             time = 0;
                             if (preCar?.LastLap != null && preCar?.LastLap.LaptimeMS > 0)
                             {
                                 time = float.TryParse(preCar?.LastLap.LaptimeMS.ToString(), out time) ? time : 0;
                                 formattedTime = GameData.GetFormattedLapTime(time);
                                 el.Text = formattedTime;
                             }

                             break;

                         case "MeDriverPos":
                             el.Text = meCar?.Position.ToString();
                             break;
                         case "MeDriver":

                             time = float.TryParse(meCar?.BestSessionLap?.LaptimeMS.ToString(), out time) ? time : 0;
                             if (_DrawState == DrawState.State1 || time == 0)
                             {
                                
                                 numberText = meCar?.Number.ToString() + "";
                                 if (numberText.Length > 0) numberText = "#" + numberText;
                                 el.Text = $"{driver.FirstName[0].ToString().ToUpper()} {driver.LastName} {numberText} ";
                                 el.Fill = new SvgColourServer(Color.FromArgb(255, 255, 255, 255));
                             }
                             else if (_DrawState == DrawState.State2)
                             {
                                 formattedTime = GameData.GetFormattedLapTime(time);
                                 el.Text = formattedTime;
                                 el.Fill = new SvgColourServer(Color.FromArgb(255, 168, 0, 195));
                             }


                             break;
                         case "MeDriverTime":
                             if (meCar?.LastLap != null && meCar?.LastLap.LaptimeMS > 0)
                             {
                                 time = float.TryParse(meCar.LastLap.LaptimeMS.ToString(), out time) ? time : 0;
                                 formattedTime = GameData.GetFormattedLapTime(time);
                                 el.Text = formattedTime;
                             }

                             break;

                         case "PostDriverPos":
                             el.Text = postCar?.Position.ToString();
                             break;
                         case "PostDriver":
                             time = float.TryParse(postCar?.BestSessionLap?.LaptimeMS.ToString(), out time) ? time : 0;
                             if (_DrawState == DrawState.State1 || time == 0)
                             {
                                 if (postCar != null)
                                 {
                                     numberText = postCar?.Number.ToString() + "";
                                     if (numberText.Length > 0) numberText = "#" + numberText;
                                     el.Text = $"{postDriver.FirstName[0].ToString().ToUpper()} {postDriver.LastName} {numberText}";
                                     el.Fill = new SvgColourServer(Color.FromArgb(255, 255, 255, 255));
                                 }
                             }
                             else if (_DrawState == DrawState.State2)
                             {

                                 formattedTime = GameData.GetFormattedLapTime(time);
                                 el.Text = formattedTime;
                                 el.Fill = new SvgColourServer(Color.FromArgb(255, 188, 100, 205));

                             }
                             break;
                         case "PostDriverTime":
                             time = 0;
                             if (postCar?.LastLap != null && postCar?.LastLap.LaptimeMS > 0)
                             {
                                 time = float.TryParse(postCar.LastLap.LaptimeMS.ToString(), out time) ? time : 0;
                                 formattedTime = GameData.GetFormattedLapTime(time);
                                 el.Text = formattedTime;
                             }

                             break;

                         case "PurpleDriverPos":
                             el.Text = purpleCar?.Position.ToString();
                             break;
                         case "PurpleDriver":

                             if (purpleCar != null)
                             {
                                 numberText = purpleCar?.Number.ToString();
                                 if (numberText.Length > 0) numberText = "#" + numberText;
                                 el.Text = $"{purpleDriver.FirstName[0].ToString().ToUpper()} {purpleDriver.LastName} {numberText}";
                             }
                             break;
                         case "PurpleDriverTime":
                             time = 0;
                             if (purpleCar?.BestSessionLap != null)
                             {
                                 time = float.TryParse(purpleCar.BestSessionLap.LaptimeMS.ToString(), out time) ? time : 0;
                                 formattedTime = GameData.GetFormattedLapTime(time);
                                 el.Text = formattedTime;
                             }

                             break;


                         default:
                             break;
                     }
                 }

                 return true;
             },
            click =>
            {

                click.Element.StrokeWidth = new SvgUnit(2);
                click.Element.Stroke = new SvgColourServer(Color.FromArgb(255, 255, 255, 255));
                Environment.Exit(0);
            }


             );

            return true;
        }

        private static void DrawLap(Car car, List<LapInfo> carLaps, SvgRectangle rect)
        {
            // Extract the lap index from the rectangle ID (e.g., "PreDriverRect1" -> 1)
            if (int.TryParse(rect.ID.Substring(rect.ID.Length - 1), out int lapIndex))
            {
                // If we have a lap for this index
                if (lapIndex <= carLaps.Count)
                {
                    var currentLap = carLaps[lapIndex - 1];
                    
                    // Compare with next lap if available
                    if (lapIndex < carLaps.Count)
                    {
                        if (currentLap.LaptimeMS > carLaps[lapIndex].LaptimeMS)
                        {
                            rect.Fill = new SvgColourServer(Color.FromArgb(255, 240, 0, 0)); // Red for slower
                        }
                        else
                        {
                            rect.Fill = new SvgColourServer(Color.FromArgb(255, 0, 200, 0)); // Green for faster
                        }
                    }
                    else
                    {
                        // Last lap with no comparison
                        rect.Fill = new SvgColourServer(Color.FromArgb(255, 0, 200, 0)); // Green for last lap
                    }

                    // If this is the best lap of the session
                    if (car?.BestSessionLap != null && currentLap.LaptimeMS == car.BestSessionLap.LaptimeMS)
                    {
                        rect.Fill = new SvgColourServer(Color.FromArgb(255, 148, 0, 185)); // Purple for best lap
                    }
                    
                    // Invalid lap
                    if (currentLap.IsInvalid)
                    {
                        rect.Fill = new SvgColourServer(Color.FromArgb(255, 249, 131, 4)); // Orange for invalid
                    }
                }
                else
                {
                    // No lap data for this index
                    rect.Fill = new SvgColourServer(Color.FromArgb(255, 34, 34, 34)); // Dark gray for no data
                }
            }
        }

      

        private static List<LapInfo> GetLastLapTimes(Car car)
        {
            if (car?.LapTimes == null || car.LapTimes.Count == 0)
            {
                return new List<LapInfo>();
            }

            try
            {
                var laptimes = new List<LapInfo>(car.LapTimes.Values);
                return laptimes
                        .Where(lap => lap != null && lap.LaptimeMS > 0)
                        .OrderByDescending(lap => lap.LapNumber)
                        .Take(6)
                        .ToList();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error getting lap times: {ex.Message}");
                return new List<LapInfo>();
            }
        }

        //The ACC data take a long time to update. If we overtake, then our position should be the preCar position + 1.
        //If we do this ourselves, we are going to speeed this up
        private void AdjustMeCarPositionBasedOnPreCar(Car meCar, Car preCar, Car postCar)
        {
            try
            {

          
                // we cant try this if its based on position, because are sorting on position and if its wrong , its wrong
                if (Settings.ContainsKey("Kind") && Settings["Kind"].ToString() == "position") return;

                if (meCar == null) return;
                if (preCar == null) return;

                //if the laps are not the same, we cannot fudge this
                if (preCar.Laps == meCar.Laps)
                {
                    meCar.Position = preCar.Position + 1;
                    if (postCar != null && postCar.Position == meCar.Position)
                    {
                        postCar.Position = meCar.Position + 1;
                    }
                  

                }
            }
            catch (Exception ex )
            {
                Trace.TraceError(ex.Message);
                //throw;
            }

        }

        private Car GetPreCar(Car meCar, ConcurrentBag<Car> cars)
        {
            Car preCar = null;

            if (Settings.ContainsKey("Kind") && Settings["Kind"].ToString() == "relative")
            {
               
                preCar = SplineUtil.GetPreCar(meCar, new ConcurrentBag<Car>(GameData.Snapshot.Track.Cars.Values), GameData.Snapshot.Track.TrackLength);
            }

            if (Settings.ContainsKey("Kind") && Settings["Kind"].ToString() == "position")
            {
                
                preCar = cars.FirstOrDefault(c => c.Position == meCar?.Position - 1);
            }

            return preCar;
        }

       

        private Car GetPostCar(Car meCar, ConcurrentBag<Car> cars)
        {
            Car postCar = null;

            if (Settings.ContainsKey("Kind") && Settings["Kind"].ToString() == "relative")
            {
                postCar = SplineUtil.GetPostCar(meCar, new ConcurrentBag<Car>(GameData.Snapshot.Track.Cars.Values), GameData.Snapshot.Track.TrackLength);
            }
            else
            {
                postCar = cars.FirstOrDefault(c => c.Position == meCar?.Position + 1);
            }

            return postCar;
        }

        private static Car GetPurpleCar(ConcurrentBag<Car> cars)
        {
            return cars.FirstOrDefault(c => c.CarIndex == GameData.Snapshot.Session.BestSession?.CarIndex);
        }


        private static DriverInfo GetPreDriver(Car preCar)
        {
            DriverInfo preDriver = new DriverInfo() { FirstName = " ", LastName = " " };

            if (preCar == null) return preDriver;

            if (preCar.DriverIndex < preCar.Drivers.Count)
            {
                preDriver = preCar.Drivers[preCar.DriverIndex];
            }

            return preDriver;
        }

        private static DriverInfo GetMeDriver(Car meCar)
        {
            DriverInfo driver = new DriverInfo() { FirstName = " ", LastName = " " };

            if (meCar == null) return driver;

            if (meCar.DriverIndex < meCar.Drivers.Count)
            {
                driver = meCar.Drivers[meCar.DriverIndex];
            }

            return driver;
        }

        private static DriverInfo GetPostDriver(Car postCar)
        {
            DriverInfo postDriver = new DriverInfo() { FirstName = " ", LastName = " " };

            if (postCar == null) return postDriver;

            if (postCar.DriverIndex < postCar.Drivers.Count)
            {
                postDriver = postCar.Drivers[postCar.DriverIndex];
            }

            return postDriver;
        }


        private static DriverInfo GetPurpleDriver(Car purpleCar)
        {
            DriverInfo driver = new DriverInfo() { FirstName = " ", LastName = " " };

            if (purpleCar == null) return driver;

            if (purpleCar.DriverIndex < purpleCar.Drivers.Count)
            {
                driver = purpleCar.Drivers[purpleCar.DriverIndex];
            }

            return driver;

        }
      

       

       

        private void StateTimer()
        {
            if (!_Stopwatch.IsRunning) _Stopwatch.Start();

            long currentTime = _Stopwatch.ElapsedMilliseconds;

            if (currentTime > _StateChangeInterval)
            {
                _DrawState = (DrawState)(((int)_DrawState + 1) % Enum.GetValues(typeof(DrawState)).Length);
                _Stopwatch.Restart();
            }
        }


        protected override void OnRender(System.Drawing.Graphics g)
        {
            if (!Visible) return;


            if (g.IsVisibleClipEmpty)
            {
                // Handle the error or log it
                return;
            }

            if (GameData.Snapshot.Track.Cars.Count == 0) return;
            if (GameData.Snapshot.PlayerCarIndex == 0) return;
            if (GameData.Snapshot.GameState.Status == GameStatus.PAUSE) return;

            bool rendered = DrawDriver(g);

            if (Settings.TryGetValue("Kind", out object kindObj))
            {
                string kind = kindObj.ToString();
                if (kind == "position" || kind == "relative")
                {
                    g.TranslateTransform(7 , this.Height / 2);
                    g.RotateTransform(-90);

                    string text = kind == "position" ? "o v e r a l l" : "r e l a t i v e";
                    SizeF textSize = g.MeasureString(text, this._Fonts["consolas"]);
                    
                    g.DrawString(text, this._Fonts["consolas"], this._Brushes["white"], -textSize.Width / 2, -textSize.Height / 2);

                    g.ResetTransform();
                }
            }



            if (!rendered)
            {
                //base.DrawNoDataMessage();
            }


            base.OnRender(g);


        }

        protected override void OnGraphicsSetup(System.Drawing.Graphics g)
        {
            this._renderer.LoadSvgFiles(
                new List<string>
                {
                    "Assets/Purple.svg",
                });

            _Brushes["white"] = new SolidBrush(Color.FromArgb(250, 255, 255, 255));
          

            base.OnGraphicsSetup(g);
        }

        protected override void OnGraphicsDestroyed(System.Drawing.Graphics g)
        {
            base.OnGraphicsDestroyed(g);
        }

        protected override bool HitTest(PointF position)
        {
            float scaledX = position.X * Scale;
            float scaledY = position.Y * Scale;
            return _renderer.IsMouseOverInteractiveElement(new PointF(scaledX, scaledY));
        }

        protected override void OnMouseClick(PointF clickPoint)
        {
            // Convert click to SVG space
            //PointF svgCoords = SVG.ConvertToSvgSpace((int)clickPoint.X, (int)clickPoint.Y);
            float scaledX = clickPoint.X * Scale;
            float scaledY = clickPoint.Y * Scale;
            // Handle the click on the SVG
            _renderer.HandleSvgClick(clickPoint);
        }


    }
}
