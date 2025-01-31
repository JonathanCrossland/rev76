using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;

namespace Rev76.Windows.Widgets
{ 
    public class SVGOverlayWindow 
    {
        public List<string> _SVG = new List<string>();
        // Store interactive elements
        Dictionary<SvgElement, RectangleF> interactiveElements = new Dictionary<SvgElement, RectangleF>();
        SvgDocument svgDocument = new SvgDocument();
        Action<SvgElement> svgClickHandler = null;

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public void HandleSvgClick(PointF clickPoint)
        {
            foreach (var kvp in interactiveElements)
            {
                if (kvp.Value.Contains(clickPoint)) // Check if click is inside an element
                {
                    svgClickHandler?.Invoke(kvp.Key);
                    return;
                }
            }
        }

        public bool IsMouseOverInteractiveElement(PointF svgPoint)
        {
            foreach (var kvp in interactiveElements)
            {
                if (kvp.Value.Contains(svgPoint))
                {
                    return true; // Mouse is over a clickable element
                }
            }
            return false;
        }

        public PointF ConvertToSvgSpace(int screenX, int screenY)
        {
            // Convert screen space to local window coordinates
            int localX = screenX - this.X;
            int localY = screenY - this.Y;

            // Scale from window size to SVG size
            float scaleX = (float)svgDocument.Width / this.Width;
            float scaleY = (float)svgDocument.Height / this.Height;

            return new PointF(localX * scaleX, localY * scaleY);
        }


        public void DrawSvg(System.Drawing.Graphics graphics, string svgString, float x, float y, float width, float height,
                    Action<SvgElement> modifyAction, Action<SvgElement> clickHandler = null)
        {
            if (graphics == null) throw new ArgumentNullException(nameof(graphics));
            if (string.IsNullOrWhiteSpace(svgString)) throw new ArgumentException("SVG string cannot be null or empty.", nameof(svgString));

            svgClickHandler = clickHandler;

            // Load SVG string into an XmlDocument to preserve namespaces
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(svgString);

            // Create and register XmlNamespaceManager
            var xmlnsManager = new XmlNamespaceManager(xmlDoc.NameTable);

            foreach (XmlAttribute attr in xmlDoc.DocumentElement.Attributes)
            {
                if (attr.Prefix == "xmlns")
                {
                    xmlnsManager.AddNamespace(attr.LocalName, attr.Value);
                }
            }

            // Convert XmlDocument to MemoryStream for SVG.NET
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                xmlDoc.Save(writer);
                writer.Flush();
                stream.Position = 0;

                // Load SVG document with preserved namespaces
                svgDocument = SvgDocument.Open<SvgDocument>(stream);

                interactiveElements.Clear();

                // Apply modifications and detect interactable elements
                foreach (var element in svgDocument.Descendants())
                {
                    modifyAction?.Invoke(element); // Modify the element

                    if (element is SvgRectangle rect)
                    {
                        // Look for lucid:interacts="click"
                        if (element.CustomAttributes.TryGetValue("https://www.lucidocean.com/svgui:interacts", out var interactValue) && interactValue == "click")
                        {
                            interactiveElements[element] = rect.Bounds; // Store its bounds for hit-testing
                        }
                    }
                }

                // Scale and translate the SVG to fit within the given dimensions
                var originalSize = svgDocument.GetDimensions();
                var scaleX = width / originalSize.Width;
                var scaleY = height / originalSize.Height;

                // Save the current graphics state
                var state = graphics.Save();

                // Apply transformations for scaling and positioning
                graphics.TranslateTransform(x, y);
                graphics.ScaleTransform(scaleX, scaleY);

                // Render the modified SVG
                svgDocument.Draw(graphics);

                // Restore the graphics state
                graphics.Restore(state);


            }
        }


        public void LoadSvgFiles(List<string> filePaths)
        {
            if (filePaths == null) throw new ArgumentNullException(nameof(filePaths));

            var svgContents = new List<string>();

            foreach (var filePath in filePaths)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        // Read the SVG file into a string and add to the list
                        var svgString = File.ReadAllText(filePath);
                        svgContents.Add(svgString);
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

            _SVG = svgContents;
        }

    }
}
