﻿using Rev76.DataModels;
using Rev76.Windows.Components;
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

        public Rev76Widget(int x, int y, int width, int height, float scale, Icon icon) 
            : base(x, y, width, height, scale, icon)
        {
          
        }
        
        protected override void OnRender(System.Drawing.Graphics gfx)
        {
           

            SVG.DrawSvg(
             gfx,
             0,
              0, 0, 300 * Scale, 550 * Scale,
              element =>
              {
                  if (element is SvgText text)
                  {
                    if (text.ID == "version")
                    {
                        text.Text = $"Version {new AppData().Version}";
                    }
                  }

                  if (element is SVGCheckBox)
                  {
                      var x = 0;
                  }

              },
              clickElement =>
              {
                 

                  switch (clickElement.Element.ID)
                  {
                      case "check_Purple_ShowInTaskbar":

                         

                          //var checkmark = clickElement.Children.FirstOrDefault(c => c.ID == "checkmark") as SvgPath;

                          //checkmark.Visibility = (checkmark.Visibility == "hidden") ? "visible" : "hidden";
                          

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
            PointF svgCoords = new PointF(position.X / Scale, position.Y / Scale);
            return SVG.IsMouseOverInteractiveElement(svgCoords);
        }


        protected override void OnMouseClick(PointF position)
        {
            PointF svgCoords = new PointF(position.X / Scale, position.Y / Scale);
            SVG.HandleSvgClick(svgCoords);
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
