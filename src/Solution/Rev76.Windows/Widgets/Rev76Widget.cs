using System.Drawing;

namespace Rev76.Windows.Widgets
{
    public class Rev76Widget : OverlayWindow
    {
        public Rev76Widget(int x, int y, int width, int height, Icon icon) 
            : base(x, y, width, height, icon)
        {
        }
        
        protected override void OnRender(System.Drawing.Graphics gfx)
        {
            Icon resizedIcon = new Icon(Icon, new Size(64, 64));
            Bitmap bmp = resizedIcon.ToBitmap();

            gfx.DrawImage(bmp, 10,10);
            base.OnRender(gfx);
        }

        public override string Title => "Rev76";

        public override bool Visible => true;

      
    }
}
