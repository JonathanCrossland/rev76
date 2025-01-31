using Assetto.Data.Broadcasting.Structs;
using Rev76.DataModels;
using Svg;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Rev76.Windows.Widgets
{
    public class PurpleWidget : OverlayWindow
    {
        SVGOverlayWindow SVG = new SVGOverlayWindow();

        public PurpleWidget(int x, int y, int width, int height, Icon icon=null) : base(x, y, width, height, icon)
        {
            this.FPS = 6;
        }

        protected override string Title { get => "Purple"; }
        // protected override bool Visible { get => GameData.GameState.Status == Thomsen.AccTools.SharedMemory.Models.GameStatus.LIVE; }
        protected override bool Visible { get => true; }

        protected override void OnRender(System.Drawing.Graphics g)
        {
            if (!Visible) return;


            if (g.IsVisibleClipEmpty)
            {
                // Handle the error or log it
                return;
            }

            if (GameData.Track.Cars.Count == 0) return;
            if (GameData.Car == null) return;


            bool rendered = DrawDriver(g);


            if (!rendered)
            {
                //base.DrawNoDataMessage();
            }


            base.OnRender(g);


        }

        private bool DrawDriver(System.Drawing.Graphics g)
        {
          

            if (GameData.GameState.SessionType != SessionType.RACE && GameData.GameState.SessionType != SessionType.QUALIFY) return false;

            Car meCar = GameData.Track.Cars.Find(c => c.CarIndex == GameData.Car.CarIndex);

            if (meCar.Drivers.Count == 0) return false;

            var driver = meCar.Drivers[GameData.Car.DriverIndex];


            Car purpleCar = GameData.Track.Cars.Find(c => c.CarIndex == GameData.Session.BestSession?.CarIndex);

            if (purpleCar == null) return false;
            DriverInfo purpleDriver = purpleCar.Drivers[GameData.Session.BestSession.DriverIndex];


            var preCar = GameData.Track.Cars.Find(c => c.Position == meCar.Position - 1);
            DriverInfo preDriver = new DriverInfo() { FirstName = " ", LastName = " " };
            if (preCar != null)
            {
                preDriver = preCar.Drivers[preCar.DriverIndex];
            }

            var postCar = GameData.Track.Cars.Find(c => c.Position  == meCar.Position + 1);
            DriverInfo postDriver = new DriverInfo() { FirstName = " ", LastName = " " };
            if (postCar != null) {
                postDriver = postCar.Drivers[postCar.DriverIndex];
            }
           
    
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

                meCarLaps = meCar.LapTimes
                //.Where(lap => lap.LapNumber != meCar.Laps) // Exclude current lap
                .OrderByDescending(lap => lap.LapNumber) // Sort back to ascending order
                .Take(6) // Take the last 4 laps
                //.OrderBy(lap => lap.LapNumber) // Sort back to ascending order
                .ToList();
                

            }

            if (meCar != null && preCar != null)
            {
                preCarGap = Car.CalculateTimeGap(preCar, meCar, GameData.Track.TrackLength);
             
                if (preCar != null)
                {

                    preCarLaps = preCar.LapTimes
                    //.Where(lap => lap.LapNumber != preCar.Laps) // Exclude current lap
                    .OrderByDescending(lap => lap.LapNumber) // Sort back to ascending order
                    .Take(6) // Take the last 4 laps
                    //.OrderBy(lap => lap.LapNumber) // Sort back to ascending order
                    .ToList();


                }
            }

            if (meCar != null && postCar != null)
            {
                postCarGap = Car.CalculateTimeGap(meCar, postCar, GameData.Track.TrackLength);
               
                if (postCar != null)
                {
                    postCarLaps = postCar.LapTimes
                     // .Where(lap => lap.LapNumber != postCar.Laps) // Exclude current lap
                      .OrderByDescending(lap => lap.LapNumber) // Sort back to ascending order
                      .Take(6) // Take the last 4 laps
                      //.OrderBy(lap => lap.LapNumber) // Sort back to ascending order
                      .ToList();
                }
            }

            
            SVG.DrawSvg(
             g,
             this.SVG._SVG[0],
             5, 5, 360, 230,
             element =>
             {


                 if (element is SvgRectangle rect)
                 {
                     for (int i = 0; i < preCarLaps.Count; i++)
                     {
                        
                         string preDriverRect = "PreDriverRect" + (i+1).ToString();


                         if (rect.ID == preDriverRect)
                         {
                             if (preCarLaps.Count() >= i+2)
                             {
                                 if (preCarLaps[i].LaptimeMS > preCarLaps[i+1].LaptimeMS)
                                 {
                                     rect.Fill = new SvgColourServer(Color.FromArgb(255, 240, 0, 0));
                                 }
                                 else 
                                 {
                                     rect.Fill = new SvgColourServer(Color.FromArgb(255, 0, 200, 0));
                                 }

                                 if (preCarLaps[i].LaptimeMS == preCar.BestSessionLap?.LaptimeMS)
                                 {
                                     rect.Fill = new SvgColourServer(Color.FromArgb(255, 148, 0, 185));
                                 }
                                 if (preCarLaps[i].IsInvalid)
                                 {
                                     rect.Fill = new SvgColourServer(Color.FromArgb(255, 249, 131, 4));
                                 }
                             }
                           
                         }
                     }
                     for (int i = 0; i < meCarLaps.Count; i++)
                     {

                         string meDriverRect = "MeDriverRect" + (i + 1).ToString();


                         if (rect.ID == meDriverRect)
                         {
                             if (meCarLaps.Count() >= i + 2)
                             {
                                 if (meCarLaps[i].LaptimeMS > meCarLaps[i + 1].LaptimeMS)
                                 {
                                     rect.Fill = new SvgColourServer(Color.FromArgb(255, 240, 0, 0));
                                 }
                                 else 
                                 {
                                     rect.Fill = new SvgColourServer(Color.FromArgb(255, 0, 200, 0));
                                 }
                                 if (meCarLaps[i].LaptimeMS == meCar.BestSessionLap?.LaptimeMS)
                                 {
                                     rect.Fill = new SvgColourServer(Color.FromArgb(255, 148, 0, 185));
                                 }
                                 if (meCarLaps[i].IsInvalid)
                                 {
                                     rect.Fill = new SvgColourServer(Color.FromArgb(255, 249, 131, 4));
                                 }
                             }

                         }
                     }


                     for (int i = 0; i < postCarLaps.Count; i++)
                     {

                         string postDriverRect = "PostDriverRect" + (i + 1).ToString();


                         if (rect.ID == postDriverRect)
                         {
                             if (postCarLaps.Count() >= i + 2)
                             {
                                 if (postCarLaps[i].LaptimeMS > postCarLaps[i + 1].LaptimeMS)
                                 {
                                     rect.Fill = new SvgColourServer(Color.FromArgb(255, 240, 0, 0));
                                 }
                                 else 
                                 {
                                     rect.Fill = new SvgColourServer(Color.FromArgb(255, 0, 200, 0));
                                 }
                                 if (postCarLaps[i].LaptimeMS == postCar.BestSessionLap?.LaptimeMS)
                                 {
                                     rect.Fill = new SvgColourServer(Color.FromArgb(255, 148, 0, 185));
                                 }
                                 if (postCarLaps[i].IsInvalid)
                                 {
                                     rect.Fill = new SvgColourServer(Color.FromArgb(255, 249, 131, 4));
                                 }
                             }

                         }
                     }


                     switch (rect.ID)
                     {
                         
                         case "PreDriverRect":

                             if (preCar?.Delta > 0)
                             {
                                 rect.Fill = new SvgDeferredPaintServer("deltared"); // new SvgColourServer(Color.FromArgb(255, 240, 0, 0));
                             }
                             else if (preCar?.Delta < 0)
                             {
                                 rect.Fill = new SvgDeferredPaintServer("deltagreen"); // new SvgColourServer(Color.FromArgb(255, 0, 200, 0));
                             }
                             //if (preCar?.CurrentLap.IsInvalid == true)
                             //{
                             //    rect.Fill = new SvgColourServer(Color.FromArgb(255, 249, 131, 4));
                             //}
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
                           
                             break;

                         case "PostDriverRect":
                             if (postCar?.Delta > 0)
                             {
                                 rect.Fill = new SvgDeferredPaintServer("deltared"); //new SvgColourServer(Color.FromArgb(255, 240, 0, 0));
                             }
                             else if (postCar?.Delta < 0)
                             {
                                 rect.Fill = new SvgDeferredPaintServer("deltagreen"); //new SvgColourServer(Color.FromArgb(255, 0, 200, 0));
                             }

                             break;

                     }
                 }

               
                 if (element is SvgText el)
                 {

                     switch (el.ID)
                     {  
                         case "PreTimeDiff":
                             el.Text = GameData.GetFormattedGap(preCarGap);
                             break;

                         case "PostTimeDiff":
                             el.Text = GameData.GetFormattedGap(postCarGap);
                             break;

                         case "PreDriverPos":
                             el.Text = preCar?.Position.ToString();
                             break;
                         case "PreDriver":
                             if (preCar != null)
                             {
                                 numberText = preCar?.Number.ToString();
                                 if (numberText.Length > 0) numberText = "#" + numberText;
                                 el.Text = $"{preDriver.FirstName[0].ToString().ToUpper()} {preDriver.LastName} {numberText}";
                             }
                             break;
                         case "PreDriverTime":
                             time = 0;
                             if (preCar?.LastLap != null)
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
                             //numberText = meCar?.Number.ToString();
                             //if (numberText.Length > 0) numberText = "#" + numberText;
                             //el.Text = $"{driver.FirstName[0].ToString().ToUpper()} {driver.LastName} {numberText} ";
                             time = float.TryParse(meCar?.BestSessionLap?.LaptimeMS.ToString(), out time) ? time : 0;
                             formattedTime = GameData.GetFormattedLapTime(time);
                             el.Text = formattedTime;
                             

                             break;
                         case "MeDriverTime":
                             if (meCar?.LastLap != null)
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
                             if (postCar != null) {
                                 numberText = postCar?.Number.ToString();
                                 if (numberText.Length > 0) numberText = "#" + numberText;
                                 el.Text = $"{postDriver.FirstName[0].ToString().ToUpper()} {postDriver.LastName} {numberText}";
                                }
                             break;
                         case "PostDriverTime":
                             time = 0;
                             if (postCar?.LastLap != null)
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

                             if (purpleCar !=null)
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


             },
                 click =>
                 {
                    
                     click.StrokeWidth = new SvgUnit(2);
                     click.Stroke  = new SvgColourServer(Color.FromArgb(255, 255, 255, 255));
                 }


             );

            return true;
        }

     

        protected override void OnGraphicsSetup(System.Drawing.Graphics g)
        {
            this.SVG.LoadSvgFiles(
                new List<string>
                {
                    "Assets/DriverEnv.svg",
                });

            base.OnGraphicsSetup(g);
        }

        protected override void OnGraphicsDestroyed(System.Drawing.Graphics g)
        {
            base.OnGraphicsDestroyed(g);
        }

    }
}
