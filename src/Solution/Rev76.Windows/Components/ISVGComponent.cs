using Svg;
using System;

namespace Rev76.Windows.Components
{
    public class ISVGComponent
    {
        
        public SvgElement Element;
        public event EventHandler Clicked;
        public event EventHandler MouseOver;
        public event EventHandler MouseOut;

        internal void RaiseClickEvent()
        {
            Clicked?.Invoke(this, EventArgs.Empty);
        }

        internal void RaiseMouseOverEvent()
        {
            MouseOver?.Invoke(this, EventArgs.Empty);
        }

        internal void RaiseMouseOutEvent()
        {
            MouseOut?.Invoke(this, EventArgs.Empty);
        }
    }
}