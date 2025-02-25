using Rev76.DataModels;
using System.Drawing;

namespace Rev76.Windows.Widgets
{
    internal class SetupMaskWidget : OverlayWindow
    {
        SVGRenderer SVG = new SVGRenderer();
        internal SetupMaskWidget(int x, int y, int width, int height, float scale, Icon icon) : base(x, y, width, height, scale, icon)
        {
            this.FPS = 4;
            
        }

        public override string Title => "Setup Mask";

        public override bool Visible { get => GameData.Snapshot.GameState.IsSetupMenuVisible; }

        protected override void OnRender(System.Drawing.Graphics gfx)
        {
            //gfx.FillRectangle(_Brushes["background"], 0, 0, Width, Height);
            _Brushes["background"] = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
            base.OnRender(gfx);
            SVG.DrawSvg
            (
                gfx,
                0,
                Width /4 , 0, Width /2 , Height,
                element =>
                {
                }
           );

          
           
        }

        protected override void OnGraphicsSetup(System.Drawing.Graphics gfx)
        {
            this.SVG.LoadSvgFiles(
                new System.Collections.Generic.List<string>
                {
                    "Assets/SetupMask.svg",
                });
            base.OnGraphicsSetup(gfx);
            _Brushes["background"] = new SolidBrush(Color.FromArgb(255,0,0,0));
        }


    }
}
