
using Rev76.DataModels;
using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Rev76.Windows.Widgets
{
    public class TyreWidgetEx : OverlayWindow
    {
        SVGRenderer SVG = new SVGRenderer();
        public TyreWidgetEx(int x, int y, int width, int height, float scale, Icon icon = null) : base(x, y, width, height, scale, icon)
        {
        }

        public override string Title => "Tyres";

        public override bool Visible
        {
            get =>
                GameData.Snapshot.GameState.Status == GameStatus.LIVE
                &&
                GameData.Snapshot.GameState.IsSetupMenuVisible == false;
        }

        protected override void OnRender(System.Drawing.Graphics gfx)
        {
            if (GameData.Snapshot.Tyres.BrakeTemp.FrontLeft == 0 && GameData.Snapshot.Tyres.TC == 0)
            {
                //base.DrawNoDataMessage();
            }
            else {

                if (Settings.ContainsKey("Kind") && Settings["Kind"].ToString() == "extended")
                {
                    DrawTyrePanel(gfx, 0, 0);
                    DrawTyreWidget(gfx, 0, (int) (65 * Scale));
                }
                else
                {
                    DrawTyreWidget(gfx, 0,0);
                }
            }
            

            base.OnRender(gfx);
        }


        private void DrawTyrePanel(System.Drawing.Graphics g, int x, int y)
        {
          


            var dynamicFillColor = Color.Green;
         

            SVG.DrawSvg(
                g,
                1,
                x, y, 325 * Scale, 60 * Scale,
                element =>
                {
                    if (element is SvgRectangle rect)
                    {

                        switch (rect.ID)
                        {
                            case "tc1rect":
                                if (GameData.Snapshot.Tyres.TCInAction > 0)
                                {
                                    //rect.Fill = new SvgDeferredPaintServer("url(#stripedPattern)");
                                }
                                break;
                            case "bbrect":
                                break;
                            case "absrect":
                                if (GameData.Snapshot.Tyres.AbsVibrations > 0)
                                {
                                    //rect.Fill = new SvgDeferredPaintServer("url(#stripedPattern)");
                                }
                                break;

                            default:
                                break;
                        }

                    }

                    if (element is SvgText el)
                    {
                    
                        switch (el.ID)
                        { 
                            case "tc1":
                                el.Text = GameData.Snapshot.Tyres.TC.ToString();
                                break;
                            case "bb":
                                el.Text = $"{(GameData.Snapshot.Tyres.BrakeBias * 100).ToString("0.0")}";
                                break;
                            case "abs":
                                el.Text = GameData.Snapshot.Tyres.ABS.ToString();
                                break;

                            default:
                                break;
                        }





                    }

                });
        }


        private void DrawTyreWidget(System.Drawing.Graphics g, int x, int y)
        {
           
            
            var dynamicFillColor = Color.Green;      
            var offset = y;
            
            
            SVG.DrawSvg(
                g,
                0,
                x, y, 320 * Scale, 350 * Scale,
                element =>
                {
                    if (element is SvgRectangle rect)
                    {
                        if (rect.ID == null) rect.ID = "";
                        switch (rect.ID)
                        {
                            case "brakebiasdefault":
                                if (GameData.Snapshot.Tyres.BrakeBiasDefault <= 0)
                                {
                                    break;
                                }

                                rect.Y =   ((int)(this.Height / Scale)) / (1.0f - GameData.Snapshot.Tyres.BrakeBiasDefault) - offset;
                                

                                break;
                            case "brakebiasrect":
                                if (GameData.Snapshot.Tyres.BrakeBias <= 0)
                                {
                                    break;
                                }
                                rect.Y =  ((int)(this.Height / Scale)) * (1.0f - GameData.Snapshot.Tyres.BrakeBias) - offset ;
                                
                                break;
                            case "leftfronttyre":
                                rect.Fill = new SvgColourServer(GetTyreTempColor(GameData.Snapshot.Tyres.TyreTemp.FrontLeft));
                                if (GameData.Snapshot.Tyres.TCInAction > 0)
                                {
                                    rect.Fill = new SvgDeferredPaintServer("url(#stripedPattern)");
                                }

                                if (GameData.Snapshot.Tyres.WheelSlip.FrontLeft == 0)
                                {
                                    rect.Fill = new SvgDeferredPaintServer("url(#tyreBlowOut)");
                                }
                                break;
                            case "rightfronttyre":
                                rect.Fill = new SvgColourServer(GetTyreTempColor(GameData.Snapshot.Tyres.TyreTemp.FrontRight));
                                if (GameData.Snapshot.Tyres.TCInAction > 0)
                                {
                                    rect.Fill = new SvgDeferredPaintServer("url(#stripedPattern)");
                                }

                                if (GameData.Snapshot.Tyres.WheelSlip.FrontRight == 0)
                                {
                                    rect.Fill = new SvgDeferredPaintServer("url(#tyreBlowOut)");
                                }
                                break;
                            case "leftreartyre":
                                rect.Fill = new SvgColourServer(GetTyreTempColor(GameData.Snapshot.Tyres.TyreTemp.RearLeft));
                                if (GameData.Snapshot.Tyres.TCInAction > 0)
                                {
                                    rect.Fill = new SvgDeferredPaintServer("url(#stripedPattern)");
                                }
                                if (GameData.Snapshot.Tyres.WheelSlip.RearLeft == 0)
                                {
                                    rect.Fill = new SvgDeferredPaintServer("url(#tyreBlowOut)");
                                }
                                break;
                            case "rightreartyre":
                                rect.Fill = new SvgColourServer(GetTyreTempColor(GameData.Snapshot.Tyres.TyreTemp.RearRight));
                                if (GameData.Snapshot.Tyres.TCInAction > 0)
                                {
                                    rect.Fill = new SvgDeferredPaintServer("url(#stripedPattern)");
                                }
                                if (GameData.Snapshot.Tyres.WheelSlip.RearRight == 0)
                                {
                                    rect.Fill = new SvgDeferredPaintServer("url(#tyreBlowOut)");
                                }
                                break;
                         
                            case "leftfrontbrake":
                                rect.Fill = new SvgColourServer(GetBrakeTempColor(GameData.Snapshot.Tyres.BrakeTemp.FrontLeft));
                                
                                if (GameData.Snapshot.Tyres.AbsVibrations > 0)
                                {
                                    rect.Fill = new SvgDeferredPaintServer("url(#stripedPattern)");
                                }
                                break;
                            case "rightfrontbrake":
                                rect.Fill = new SvgColourServer(GetBrakeTempColor(GameData.Snapshot.Tyres.BrakeTemp.FrontRight));
                                if (GameData.Snapshot.Tyres.AbsVibrations > 0)
                                {
                                    rect.Fill = new SvgDeferredPaintServer("url(#stripedPattern)");
                                }
                                break;
                            case "leftrearbrake":
                                rect.Fill = new SvgColourServer(GetBrakeTempColor(GameData.Snapshot.Tyres.BrakeTemp.RearLeft));
                                if (GameData.Snapshot.Tyres.AbsVibrations > 0)
                                {
                                    rect.Fill = new SvgDeferredPaintServer("url(#stripedPattern)");
                                }
                                break; 
                            case "rightrearbrake":
                                rect.Fill = new SvgColourServer(GetBrakeTempColor(GameData.Snapshot.Tyres.BrakeTemp.RearRight));
                                if (GameData.Snapshot.Tyres.AbsVibrations > 0)
                                {
                                    rect.Fill = new SvgDeferredPaintServer("url(#stripedPattern)");
                                }
                                break;
                            default:
                                break;
                        }

                    }
                    
                    if (element is SvgText el)
                    {
                        switch (el.ID)
                        {
                            case "brakeset":
                                el.Text = GameData.Snapshot.Tyres.GetBrakeString();;
                                break;
                            case "tyreset":
                                el.Text = GameData.Snapshot.Tyres.GetTyreString(); ;
                                break;
                            case "leftfrontpsi":
                                el.Text = $"{Math.Round(GameData.Snapshot.Tyres.WheelsPressure.FrontLeft, 1).ToString("0.0")}";
                                break;
                            case "rightfrontpsi":
                                el.Text = $"{Math.Round(GameData.Snapshot.Tyres.WheelsPressure.FrontRight, 1).ToString("0.0")}";
                                break;
                            case "leftrearpsi":
                                el.Text = $"{Math.Round(GameData.Snapshot.Tyres.WheelsPressure.RearLeft, 1).ToString("0.0")}";
                                break;
                            case "rightrearpsi":
                                el.Text = $"{Math.Round(GameData.Snapshot.Tyres.WheelsPressure.RearRight, 1).ToString("0.0")}";
                                break;
                            case "leftfronttemp":
                                el.Text = $"{Math.Round(GameData.Snapshot.Tyres.TyreTemp.FrontLeft, 0)}°";
                                break;
                            case "rightfronttemp":
                                el.Text = $"{Math.Round(GameData.Snapshot.Tyres.TyreTemp.FrontRight, 0)}°";
                                break;
                            case "leftreartemp":
                                el.Text = $"{Math.Round(GameData.Snapshot.Tyres.TyreTemp.RearLeft, 0)}°";
                                break;
                            case "rightreartemp":
                                el.Text = $"{Math.Round(GameData.Snapshot.Tyres.TyreTemp.RearRight, 0)}°";
                                break;
                            case "leftfrontbraketemp":
                                el.Text = $"{Math.Round(GameData.Snapshot.Tyres.BrakeTemp.FrontLeft, 0)}°";
                                break;
                            case "rightfrontbraketemp":
                                el.Text = $"{Math.Round(GameData.Snapshot.Tyres.BrakeTemp.FrontRight, 0)}°";
                                break;
                            case "leftrearbraketemp":
                                el.Text = $"{Math.Round(GameData.Snapshot.Tyres.BrakeTemp.RearLeft, 0)}°";
                                break;
                            case "rightrearbraketemp":
                                el.Text = $"{Math.Round(GameData.Snapshot.Tyres.BrakeTemp.RearRight, 0)}°";
                                break;
                            case "leftfrontbrakewear":
                                el.Text = $"{Math.Round(GameData.Snapshot.Tyres.GetPadLifePercentage(Tyres.Position.FrontLeft), 0)}%";
                                break;
                            case "rightfrontbrakewear":
                                el.Text = $"{Math.Round(GameData.Snapshot.Tyres.GetPadLifePercentage(Tyres.Position.FrontRight), 0)}%";
                                break;
                            case "leftrearbrakewear":
                                el.Text = $"{Math.Round(GameData.Snapshot.Tyres.GetPadLifePercentage(Tyres.Position.RearLeft), 0)}%";
                                break;
                            case "rightrearbrakewear":
                                el.Text = $"{Math.Round(GameData.Snapshot.Tyres.GetPadLifePercentage(Tyres.Position.RearRight), 0)}%";
                                break;
                            case "brakebias":
                                el.Text = "";//$"{(GameData.Snapshot.Tyres.BrakeBias * 100).ToString("0.0")}%";
                                break;
                            default:
                                break;
                        }
                    }
                });
        }



        private Color GetTyreTempColor(float temp)
        {
            if (temp < 25)
                return ColorTranslator.FromHtml("#5433fd");
            else if (temp >= 25 && temp < 55)
                return ColorTranslator.FromHtml("#33cdfd");
            else if (temp >= 55 && temp < 70)
                return ColorTranslator.FromHtml("#33fdc5");
            else if (temp >= 70 && temp < 100)
                return ColorTranslator.FromHtml("#33fd64");
            else if (temp >= 100 && temp < 110)
                return ColorTranslator.FromHtml("#fdc633");
            else if (temp >= 110 && temp < 120)
                return ColorTranslator.FromHtml("#fdc633");
            else
                return ColorTranslator.FromHtml("#fd3333");
        }


        private Color GetBrakeTempColor(float brakeTemp)
        {
            if (brakeTemp < 50f)
                return ColorTranslator.FromHtml("#5433fd");
            else if (brakeTemp >= 50f && brakeTemp < 100f)
                return ColorTranslator.FromHtml("#33cdfd");
            else if (brakeTemp >= 100f && brakeTemp < 200f)
                return ColorTranslator.FromHtml("#33fdc5");
            else if (brakeTemp >= 200f && brakeTemp < 800f)
                return ColorTranslator.FromHtml("#33fd64");
            else if (brakeTemp >= 800f && brakeTemp < 850f)
                return ColorTranslator.FromHtml("#9ffd33");
            else if (brakeTemp >= 850f && brakeTemp < 900f)
                return ColorTranslator.FromHtml("#fdc633");
            else if (brakeTemp >= 900f && brakeTemp < 950f)
                return ColorTranslator.FromHtml("#fd8333");
            else
                return ColorTranslator.FromHtml("#fd3333");
        }


        protected override void OnGraphicsSetup(System.Drawing.Graphics gfx)
        {
            SVG.LoadSvgFiles(new List<string>
            {
                "Assets/TyreWidget.svg",
                "Assets/TyrePanel.svg"
            });
            base.OnGraphicsSetup(gfx);
        }
    }
}
