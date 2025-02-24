using Svg;
using System.Drawing;
using System.Linq;

namespace Rev76.Windows.Components
{
    public class SVGCheckBox : ISVGComponent
    {
        private SvgElement _Checkmark;
        

        public SVGCheckBox(SvgElement el)
        {
            Element = el;
            _Checkmark = Element.Children.FirstOrDefault(c => c.GetType().Name  == "SvgPath") as SvgPath;
            this.Clicked += SVGCheckBox_Clicked;
            this.MouseOver += SVGCheckBox_MouseOver;
            this.MouseOut += SVGCheckBox_MouseOut;

           
        }

        private void SVGCheckBox_MouseOut(object sender, System.EventArgs e)
        {
            SvgRectangle rect = Element.Children.FirstOrDefault(c => c.GetType().Name == "SvgRectangle") as SvgRectangle;
            rect.Fill = new SvgColourServer(Color.White);
        }

        private void SVGCheckBox_MouseOver(object sender, System.EventArgs e)
        {
            SvgRectangle rect = Element.Children.FirstOrDefault(c => c.GetType().Name == "SvgRectangle") as SvgRectangle;
            rect.Fill = new SvgColourServer(Color.Red);
        }

        private void SVGCheckBox_Clicked(object sender, System.EventArgs e)
        {
            Checked = !Checked;
        }

        public bool Checked
        {
            get { return _Checkmark.Visibility == "visible"; }
            set { 
                
                _Checkmark.Visibility = (value == true) ? "visible" : "hidden";

            }
        }


    }
}
