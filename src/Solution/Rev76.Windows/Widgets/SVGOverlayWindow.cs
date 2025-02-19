using Rev76.Windows.Components;
using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Rev76.Windows.Widgets
{
    public class SVGOverlayWindow
    {
        public List<SvgDocument> _SVGDocuments = new List<SvgDocument>();
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        private Dictionary<ISVGComponent, RectangleF> _ElementsClickEvent = new Dictionary<ISVGComponent, RectangleF>();
        
        private Action<ISVGComponent> svgClickHandler = null;
        


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
            object el = element;

          

            if (element is SvgVisualElement rect)
            {
                element.CustomAttributes.TryGetValue("https://www.lucidocean.com/svgui:checkbox", out var checkbox);

                if (checkbox == "")
                {
                    el = new SVGCheckBox(element);
                    _ElementsClickEvent[el as ISVGComponent] = rect.Bounds;
                }

                element.CustomAttributes.TryGetValue("https://www.lucidocean.com/svgui:button", out var button);

                if (button == "")
                {
                    el = new SVGButton(element);
                    _ElementsClickEvent[el as ISVGComponent] = rect.Bounds;
                }

            }
           
            return el;
        }


        public void HandleSvgClick(PointF clickPoint)
        {
            foreach (var kvp in _ElementsClickEvent)
            {
                if (kvp.Value.Contains(clickPoint)) // Check if click is inside an element
                {
                    kvp.Key.RaiseClickEvent();
                    svgClickHandler?.Invoke(kvp.Key);
                    return;
                }
            }
        }


        public bool IsMouseOverInteractiveElement(PointF svgPoint)
        {
            foreach (var kvp in _ElementsClickEvent)
            {
                if (kvp.Value.Contains(svgPoint))
                {
                    return true; // Mouse is over a clickable element
                }
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
                        Console.WriteLine($"Error reading file {filePath}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"File not found: {filePath}");
                }
            }
        }


        public PointF ConvertToSvgSpace(float screenX, float screenY)
        {
            // Convert screen space to local window coordinates
            float localX = screenX - this.X;
            float localY = screenY - this.Y;

            // Scale from window size to SVG size
            float scaleX = (float)_SVGDocuments[0].Width / this.Width;
            float scaleY = (float)_SVGDocuments[0].Height / this.Height;

            return new PointF(localX * scaleX, localY * scaleY);
        }

    }
}
