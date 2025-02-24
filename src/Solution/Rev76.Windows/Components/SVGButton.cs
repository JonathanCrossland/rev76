using Svg;
using System;
using System.Drawing;

namespace Rev76.Windows.Components
{
    public class SVGButton : SVGComponent
    {
        private SvgElement _Checkmark;
        

        public SVGButton(SvgElement el) : base(el)
        {
            Element = el;
        }
    
    }
}
