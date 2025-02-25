using Rev76.Windows.Components;
using Svg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace Rev76.Windows.Widgets
{
    public class SVGRenderer
    {
        public List<SvgDocument> _SVGDocuments = new List<SvgDocument>();
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        private Dictionary<ISVGComponent, RectangleF> _ElementsClickEvent = new Dictionary<ISVGComponent, RectangleF>();
        
        private Action<ISVGComponent> svgClickHandler = null;

        const string NamespaceUri = "https://www.lucidocean.com/svgui";

        const float precisionThreshold = 0.1f;

        public void DrawSvg(System.Drawing.Graphics graphics, int documentIndex, float x, float y, float width, float height, Action<dynamic> preRenderCallback, Action <ISVGComponent> clickHandlerCallback = null)
        {
            if (graphics == null) throw new ArgumentNullException(nameof(graphics));
            if (documentIndex < 0 || documentIndex >= _SVGDocuments.Count) throw new ArgumentOutOfRangeException(nameof(documentIndex));

            svgClickHandler = clickHandlerCallback;
            X = x;
            Y = y;
            Width = width;
            Height = height;
              

            var svgDocument = _SVGDocuments[documentIndex];

            PreRender(preRenderCallback, svgDocument);

            var originalSize = svgDocument.GetDimensions();
            var scaleX = width / originalSize.Width;
            var scaleY = height / originalSize.Height;
            svgDocument.Ppi = (int)graphics.DpiX;
            var state = graphics.Save();
            graphics.TranslateTransform(x, y);
            graphics.ScaleTransform(scaleX, scaleY);
            svgDocument.Draw(graphics);
            graphics.Restore(state);
        }

        private void PreRender(Action<dynamic> modifyAction, SvgDocument svgDocument)
        {
            _ElementsClickEvent.Clear();
            foreach (var element in svgDocument.Descendants())
            {
                var el = WireComponent(element);
                modifyAction?.Invoke(el); // allow element modification before render
            }
           
        }

        private object WireComponent(SvgElement element)
        { 
            dynamic el = element;
          

            if (element is SvgVisualElement rect)
            {
                element.CustomAttributes.TryGetValue($"{NamespaceUri}:checkbox", out var checkbox);

                if (checkbox != null)
                {
                    el = new SVGCheckBox(element);
                    el.Name = checkbox;
                    _ElementsClickEvent[el as ISVGComponent] = rect.Bounds;
                }

                element.CustomAttributes.TryGetValue($"{NamespaceUri}:button", out var button);

                if (button != null)
                {
                    el = new SVGButton(element);
                    el.Name = button;
                    _ElementsClickEvent[el as ISVGComponent] = rect.Bounds;
                }

            }
           
            return el;
        }


        public void HandleSvgClick(PointF clickPoint)
        {
            foreach (var element in _ElementsClickEvent)
            {
                if (element.Value.Contains(clickPoint)) // Check if click is inside an element
                {
                    element.Key.RaiseClickEvent();
                    svgClickHandler?.Invoke(element.Key);
                    return;
                }
            }
        }


        public bool IsMouseOverInteractiveElement(PointF svgPoint)
        {
            foreach (var element in _ElementsClickEvent)
            {
                if (element.Value.Contains(svgPoint))
                {
                    element.Key.RaiseMouseOverEvent(); 
                    return true; // Mouse is over a clickable element
                }
                element.Key.RaiseMouseOutEvent();
            }
            return false;
        }

        public void LoadSvgFiles(List<string> filePaths)
        {
            if (filePaths == null) throw new ArgumentNullException(nameof(filePaths));

            _SVGDocuments.Clear();

            foreach (var filePath in filePaths)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            var svgDocument = SvgDocument.Open<SvgDocument>(stream);
                            _SVGDocuments.Add(svgDocument);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"Error reading file {filePath}: {ex.Message}");
                        throw ex;
                    }
                }
                else
                {
                    Trace.TraceError($"File not found: {filePath}");
                }
            }
        }

        public void TweenAnimation(SvgImage element, int fps)
        {
            if (!element.CustomAttributes.TryGetValue($"{NamespaceUri}:targetx", out var targetx) ||
                !element.CustomAttributes.TryGetValue($"{NamespaceUri}:targety", out var targety) ||
                !element.CustomAttributes.TryGetValue($"{NamespaceUri}:tweenspeed", out var tweenSpeed))
            {
                return;
            }

            if (!float.TryParse(targetx, out float targetX) ||
                !float.TryParse(targety, out float targetY) ||
                !float.TryParse(tweenSpeed, out float duration))
            {
                return;
            }

            float t = 1f / (duration * fps); // fps is controlled externally

            // Apply easing to the interpolation factor (t)
            float easedT = EaseInOutQuad(t);

            element.X = Lerp(element.X, targetX, easedT);
            element.Y = Lerp(element.Y, targetY, easedT);

            if (Math.Abs(element.X - targetX) <= precisionThreshold &&
                Math.Abs(element.Y - targetY) <= precisionThreshold)
            {
                element.X = targetX;
                element.Y = targetY;
            }
        }

        private float Lerp(float start, float end, float t)
        {
            return start + (end - start) * t;
        }

        private float EaseInOutQuad(float t)
        {
            // Easing function: smooth acceleration and deceleration
            return t < 0.5f ? 2 * t * t : 1 - (float)Math.Pow(-2 * t + 2, 2) / 2;
        }
    }
}
