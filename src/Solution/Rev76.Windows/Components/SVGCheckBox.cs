using Svg;
using System;
using System.Drawing;
using System.Linq;

namespace Rev76.Windows.Components
{
    public class SVGCheckBox : SVGComponent
    {
        private SvgElement _Checkmark;
        

        public SVGCheckBox(SvgElement el) : base(el)
        {
            Element = el;
            _Checkmark = Element.Children.FirstOrDefault(c => c.GetType().Name  == "SvgPath") as SvgPath;
        }

        public override void SVGCheckBox_Clicked(object sender, EventArgs e)
        {
            Checked = !Checked;
            base.SVGCheckBox_Clicked(sender, e);
        }

        public override void SVGCheckBox_MouseOut(object sender, EventArgs e)
        {
           
            base.SVGCheckBox_MouseOut(sender, e);
        }

        public override void SVGCheckBox_MouseOver(object sender, EventArgs e)
        {
           
            base.SVGCheckBox_MouseOver(sender, e);
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
