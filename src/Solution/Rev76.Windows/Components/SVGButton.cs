using Svg;

namespace Rev76.Windows.Components
{
    public class SVGButton : ISVGComponent
    {
        private SvgElement _Checkmark;
        

        public SVGButton(SvgElement el)
        {
            Element = el;
          
        }

    }
}
