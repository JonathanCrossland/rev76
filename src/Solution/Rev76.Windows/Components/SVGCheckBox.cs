using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Xml.Linq;

namespace Rev76.Windows.Components
{
    public class SVGCheckBox : ISVGComponent
    {
        private SvgElement _Checkmark;
        

        public SVGCheckBox(SvgElement el)
        {
            Element = el;
            _Checkmark = Element.Children.FirstOrDefault(c => c.GetType().Name  == "SvgPath") as SvgPath;
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
