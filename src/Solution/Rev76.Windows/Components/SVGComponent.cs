using Svg;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;

namespace Rev76.Windows.Components
{
    public class SVGComponent : ISVGComponent
    {
        private SvgElement _Checkmark;


        public SVGComponent(SvgElement el)
        {
            Element = el;

            this.MouseOver += SVGCheckBox_MouseOver;
            this.MouseOut += SVGCheckBox_MouseOut;
            this.Clicked += SVGCheckBox_Clicked;
        }

        public virtual void SVGCheckBox_Clicked(object sender, System.EventArgs e)
        {
           
        }

        public virtual void SVGCheckBox_MouseOut(object sender, System.EventArgs e)
        {

        }

        public virtual void SVGCheckBox_MouseOver(object sender, System.EventArgs e)
        {
          

        }

    }
}
