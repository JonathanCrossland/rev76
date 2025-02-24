using Rev76.DataModels;
using Rev76.Windows.Components;
using Rev86.Core.Config;
using Svg;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Rev76.Windows.Widgets
{
    public class Rev76Widget : OverlayWindow
    {
        private SVGRenderer SVG = new SVGRenderer();

        public Rev76Widget(int x, int y, int width, int height, float scale, Icon icon) 
            : base(x, y, width, height, scale, icon)
        {
          
        }

        public override string Title => "Rev76";
        public override bool Visible => true;

        protected override void OnRender(System.Drawing.Graphics gfx)
        {

            try
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

                   if (element is SVGCheckBox checkbox) {
                        WidgetConfig config = config = RevConfig.Instance.Widgets.Find(w => w.Name == element.Element.Parent.ID);

                        switch (checkbox.Name)
                        {
                            case "ShowInTaskBar":
                                checkbox.Checked = config.ShowInTaskBar;
                                break;
                            case "Enable":
                                checkbox.Checked = config.Enable;
                                break;
                            default:
                                break;
                        }

                        
                    }

                },
                clickElement =>
                {
                    if (clickElement is SVGCheckBox checkbox) {
                        WidgetConfig config = config = RevConfig.Instance.Widgets.Find(w => w.Name == clickElement.Element.Parent.ID);

                        switch (checkbox.Name)
                        {
                            case "ShowInTaskBar":
                                config.ShowInTaskBar = checkbox.Checked;
                                break;
                            case "Enable":
                                config.Enable = checkbox.Checked;
                                break;
                            default:
                                break;
                        }

                        RevConfig.Instance.UpdateWidget(config);
                    }

                });   

            }
            catch (System.Exception ex)
            {
                Trace.WriteLine($"Rev76 Render: {ex.Message}");
                throw;
            }
            finally
            {
                base.OnRender(gfx);
            }

            
        }

        private void Element_MouseOut(object sender, MouseArg e)
        {
            (sender as SvgVisualElement).Fill = new SvgColourServer(Color.White);
        }

        private void Element_MouseOver(object sender, MouseArg e)
        {
            (sender as SvgVisualElement).Fill = new SvgColourServer(Color.Red);
        }

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
