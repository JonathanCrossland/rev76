using Rev76.DataModels;
using Svg;
using System.Collections.Generic;
using System.Drawing;

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
            //Icon resizedIcon = new Icon(Icon, new Size(64, 64));
            //Bitmap bmp = resizedIcon.ToBitmap();

            //gfx.DrawImage(bmp, 10,10);
            
            //gfx.FillRectangle(_Brushes["background"],this.Size);

            SVG.DrawSvg(
             gfx,
             this.SVG._SVG[0],
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
              });

            base.OnRender(gfx);
        }

        public override string Title => "Rev76";

        public override bool Visible => true;


        

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
