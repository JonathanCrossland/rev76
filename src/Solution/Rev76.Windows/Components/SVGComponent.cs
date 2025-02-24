using Svg;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;

namespace Rev76.Windows.Components
{
    public class SVGComponent : ISVGComponent
    {
        public SVGComponent(SvgElement el)
        {
            Element = el;

            this.MouseOver += SVGComponent_MouseOver;
            this.MouseOut += SVGComponent_MouseOut;
            this.Clicked += SVGComponent_Clicked;
        }
        public string Name { get; set; }

        public virtual void SVGComponent_Clicked(object sender, System.EventArgs e)
        {
           
        }

        public virtual void SVGComponent_MouseOut(object sender, System.EventArgs e)
        {

        }

        public virtual void SVGComponent_MouseOver(object sender, System.EventArgs e)
        {
          

        }

    }
}
