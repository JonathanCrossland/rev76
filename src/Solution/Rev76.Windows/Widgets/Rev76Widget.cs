using Rev76.DataModels;
using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;

namespace Rev76.Windows.Widgets
{
    public class Rev76Widget : OverlayWindow
    {
        private SVGOverlayWindow SVG = new SVGOverlayWindow();

        public Rev76Widget(int x, int y, int width, int height, Icon icon) 
            : base(x, y, width, height, icon)
        {
           
        }
        
        protected override void OnRender(System.Drawing.Graphics gfx)
        {
           

            SVG.DrawSvg(
             gfx,
             0,
              0, 0, 300, 550,
              element =>
              {
                  if (element is SvgText text)
                  {
                    if (text.ID == "version")
                    {
                        text.Text = $"Version {new AppData().Version}";
                    }
                  }

              },
              hoverElement =>
              {
                  switch (hoverElement.ID)
                  {
                      case "check_Purple_ShowInTaskbar":

                          
                          var element = hoverElement.Children.FirstOrDefault() as SvgVisualElement;
                        
                          


                          break;
                      default:
                          break;
                  }
              },
              clickElement =>
              {
                  switch (clickElement.ID)
                  {
                      case "check_Purple_ShowInTaskbar":

                          var checkmark = clickElement.Children.FirstOrDefault(c => c.ID == "checkmark") as SvgPath;

                          checkmark.Visibility = (checkmark.Visibility == "hidden") ? "visible" : "hidden";
                          

                          break;
                      default:
                          break;
                  }



              });

            base.OnRender(gfx);
        }

        private void Element_MouseOut(object sender, MouseArg e)
        {
            (sender as SvgVisualElement).Fill = new SvgColourServer(Color.White);
        }

        private void Element_MouseOver(object sender, MouseArg e)
        {
            (sender as SvgVisualElement).Fill = new SvgColourServer(Color.Red);
        }

        public override string Title => "Rev76";

        public override bool Visible => true;

        protected override bool HitTest(PointF position)
        {
            return SVG.IsMouseOverInteractiveElement(position);

        }

        protected override void OnMouseClick(PointF clickPoint)
        {
            // Convert click to SVG space
            PointF svgCoords = SVG.ConvertToSvgSpace((int)clickPoint.X, (int)clickPoint.Y);

            // Handle the click on the SVG
            SVG.HandleSvgClick(clickPoint);
        }

        protected override void OnGraphicsSetup(System.Drawing.Graphics gfx)
        {
            this.SVG.LoadSvgFiles(
               new List<string>
               {
                    "Assets/Settings.svg",
               });


            base.OnGraphicsSetup(gfx);
            _Brushes["background"] = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
        }

        

    }
}
