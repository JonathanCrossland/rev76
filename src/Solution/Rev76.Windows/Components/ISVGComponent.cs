using Svg;
using System;

namespace Rev76.Windows.Components
{
    public class ISVGComponent
    {
        
        public SvgElement Element;
        public event EventHandler Clicked;

        internal void RaiseClickEvent()
        {
            Clicked?.Invoke(this, EventArgs.Empty);
        }
    }
}