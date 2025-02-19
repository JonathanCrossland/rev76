using Svg;
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
