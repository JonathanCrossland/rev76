using Rev76.DataModels;
using Rev86.Core.Config;
using Svg;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

namespace Rev76.Windows.Widgets
{
    public class SetupOverlayWidget : OverlayWindow
    {
      
        SetupMaskWidget _mask = null;
        public SetupOverlayWidget(int x, int y, int width, int height, float scale, Icon icon) : base(x, y, width, height, scale, icon)
        {
            ShowMask(x, y, width, height, scale, icon);
            this.ShowInTaskbar = false;
        }

        private void ShowMask(int x, int y, int width, int height, float scale, Icon icon)
        {
            Task.Run(() =>
            {
                _mask = new SetupMaskWidget(0, WindowManager.Screen.VirtualScreen.Right + 10, width, height, scale, icon);
                _mask.FPS = 4;
                _mask.ShowInTaskbar = true;
                _mask.Show();
            });
        }

        public override string Title => "Setup Overlay";

        public override bool Visible { get =>
                GameData.Snapshot.GameState.IsSetupMenuVisible;
        }


        protected override void OnGraphicsSetup(System.Drawing.Graphics gfx)
        {
           
            base.OnGraphicsSetup(gfx);
        }

       

        protected override void OnRender(System.Drawing.Graphics gfx)
        {
            
            DrawBorder(gfx);
            base.OnRender(gfx);
            _Brushes["background"] = new SolidBrush(Color.FromArgb(1, 0, 0, 0));
        }

        private void DrawBorder(System.Drawing.Graphics gfx)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                int cornerRadius = 20;
                path.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90);
                path.AddArc(Width - cornerRadius, 0, cornerRadius, cornerRadius, 270, 90);
                path.AddArc(Width - cornerRadius, Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
                path.AddArc(0, Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
                path.CloseFigure();

                using (Pen pen = new Pen(Color.FromArgb(155, 255, 255, 255), 4))
                {
                    pen.DashStyle = DashStyle.Solid;
                    gfx.DrawPath(pen, path);
                }
            }
        }

        public override void OnClose()
        {
            _mask.Close();
            base.OnClose();
        }



    }
}
