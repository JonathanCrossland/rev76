using Rev76.Windows.Components;
using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace Rev76.Windows.Rendering
{
    public class DirectXRenderer : IDisposable
    {
        private List<SvgDocument> _SVGDocuments = new List<SvgDocument>();
        private Dictionary<ISVGComponent, RectangleF> _ElementsClickEvent = new Dictionary<ISVGComponent, RectangleF>();
        private Action<ISVGComponent> _svgClickHandler = null;

        // DirectX resources
        private SharpDX.Direct2D1.Factory1 _d2dFactory;
        private SharpDX.Direct3D11.Device _d3dDevice;
        private SharpDX.Direct3D11.DeviceContext _d3dContext;
        // These fields are not used in the current implementation but kept for future use
        // private SharpDX.DXGI.SwapChain1 _swapChain;
        // private SharpDX.Direct2D1.Bitmap1 _targetBitmap;
        private SharpDX.Direct2D1.SolidColorBrush _defaultBrush;
        private SharpDX.DXGI.Device _dxgiDevice;
        private SharpDX.Direct2D1.Device _d2dDevice;
        private SharpDX.Direct2D1.DeviceContext _d2dContext;

        // Cached resources
        private Dictionary<string, SharpDX.Direct2D1.Geometry> _geometryCache = new Dictionary<string, SharpDX.Direct2D1.Geometry>();
        private Dictionary<string, SharpDX.Direct2D1.Brush> _brushCache = new Dictionary<string, SharpDX.Direct2D1.Brush>();

        private Dictionary<string, List<SvgElement>> _processedElementsCache = new Dictionary<string, List<SvgElement>>();
        private bool _isLeaderboard = false;

        private System.Drawing.Bitmap _cachedBitmap = null;
        private int _cachedWidth = 0;
        private int _cachedHeight = 0;
        private int _cachedDocumentIndex = -1;

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public DirectXRenderer()
        {
            InitializeDirectX();
        }

        private void InitializeDirectX()
        {
            try
            {
                // Create Direct2D factory
                _d2dFactory = new SharpDX.Direct2D1.Factory1(SharpDX.Direct2D1.FactoryType.SingleThreaded);
                System.Diagnostics.Debug.WriteLine("Direct2D factory created.");

                // Create Direct3D device
                using (var adapter = GetAdapter())
                {
                    if (adapter == null)
                    {
                        throw new InvalidOperationException("No suitable adapter found.");
                    }

                _d3dDevice = new SharpDX.Direct3D11.Device(
                    adapter,
                    SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport | SharpDX.Direct3D11.DeviceCreationFlags.Debug
                );

                    _d3dContext = _d3dDevice.ImmediateContext;
                    System.Diagnostics.Debug.WriteLine("Direct3D device and context created.");
                }

                // Get DXGI device from D3D device
                _dxgiDevice = _d3dDevice.QueryInterface<SharpDX.DXGI.Device>();

                // Validate DXGI device
                if (_dxgiDevice == null)
                {
                    throw new InvalidOperationException("Failed to get DXGI device.");
                }
                System.Diagnostics.Debug.WriteLine("DXGI device obtained.");

                // Create Direct2D device
                _d2dDevice = new SharpDX.Direct2D1.Device(_d2dFactory, _dxgiDevice);
                _d2dContext = new SharpDX.Direct2D1.DeviceContext(_d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None);
                System.Diagnostics.Debug.WriteLine("Direct2D device and context created.");

                // Create default brush
                _defaultBrush = new SharpDX.Direct2D1.SolidColorBrush(_d2dContext, new SharpDX.Mathematics.Interop.RawColor4(1, 1, 1, 1));
                System.Diagnostics.Debug.WriteLine("Default brush created.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DirectX initialization failed: {ex.Message}");
                throw;
            }
        }


        // Update the GetAdapter method to use the correct namespace for AdapterFlags
        private SharpDX.DXGI.Adapter1 GetAdapter()
        {
            using (var factory = new SharpDX.DXGI.Factory1())
            {
                foreach (var adapter in factory.Adapters1)
                {
                    var desc = adapter.Description1;
                    // Check if it's not a software adapter
                    if ((desc.Flags & SharpDX.DXGI.AdapterFlags.Software) != 0) // Software flag is 0x1
                        continue;
                    System.Diagnostics.Debug.WriteLine($"Adapter found: {desc.Description}");
                    return adapter;
                }
            }
            throw new Exception("No suitable adapter found");
        }

        public void LoadSvgFiles(List<string> svgFiles)
        {
            foreach (var file in svgFiles)
            {
                if (File.Exists(file))
                {
                    try
                    {
                        // Load SVG with high quality settings
                        var doc = SvgDocument.Open(file);
                        
                        // Set SVG rendering quality
                        doc.ShapeRendering = SvgShapeRendering.GeometricPrecision;
                       
                        _SVGDocuments.Add(doc);
                        System.Diagnostics.Debug.WriteLine($"Loaded SVG file: {file}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to load SVG file {file}: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"SVG file not found: {file}");
                }
            }
        }

        public bool DrawSvg(System.Drawing.Graphics graphics, int documentIndex, float x, float y, float width, float height, Func<dynamic, bool> preRenderCallback, Action<ISVGComponent> clickHandlerCallback = null)
        {
            if (graphics == null) throw new ArgumentNullException(nameof(graphics));
            if (documentIndex < 0 || documentIndex >= _SVGDocuments.Count) throw new ArgumentOutOfRangeException(nameof(documentIndex));

            _svgClickHandler = clickHandlerCallback;
            X = x;
            Y = y;
            Width = width;
            Height = height;

            var svgDocument = _SVGDocuments[documentIndex];
            
            // Process elements before rendering - this may modify the SVG content
            PreRender(preRenderCallback, svgDocument);

            try
            {
                // Get the original SVG dimensions
                var originalSize = svgDocument.GetDimensions();
                
                // Calculate scale factors to fit the SVG within the specified dimensions
                float scaleX = width / originalSize.Width;
                float scaleY = height / originalSize.Height;
                
                // Set the DPI to match the graphics context
                svgDocument.Ppi = (int)graphics.DpiX;
                
                // Save the current graphics state
                var state = graphics.Save();
                
                // Set high quality rendering
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                
                // Create a new bitmap for rendering
                if (_cachedBitmap != null)
                {
                    _cachedBitmap.Dispose();
                }
                
                _cachedBitmap = new System.Drawing.Bitmap((int)width, (int)height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (var g = System.Drawing.Graphics.FromImage(_cachedBitmap))
                {
                    // Set high quality rendering
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    
                    // Clear the bitmap with transparent background
                    g.Clear(System.Drawing.Color.Transparent);
                    
                    // Apply transformations
                    g.TranslateTransform(0, 0);
                    g.ScaleTransform(scaleX, scaleY);
                    
                    try
                    {
                        // Draw the SVG document to the bitmap
                        svgDocument.Draw(g);
                    }
                    catch (ArgumentException ex)
                    {
                        // Handle ColorBlend error specifically
                        if (ex.Message.Contains("ColorBlend"))
                        {
                            System.Diagnostics.Debug.WriteLine($"ColorBlend error: {ex.Message}");
                            // Continue with the bitmap as is
                        }
                        else
                        {
                            // Re-throw if it's not a ColorBlend error
                            throw;
                        }
                    }
                }
                
                // Update cache properties
                _cachedWidth = (int)width;
                _cachedHeight = (int)height;
                _cachedDocumentIndex = documentIndex;
                
                // Draw the bitmap to the graphics context
                graphics.DrawImage(_cachedBitmap, x, y, width, height);
                
                // Restore the graphics state
                graphics.Restore(state);
                
                return true; // Return true to indicate successful rendering
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DrawSvg failed: {ex.Message}");
                return false; // Return false to indicate rendering failure
            }
        }

        private System.Drawing.Bitmap ConvertD2DBitmapToGdiBitmap(SharpDX.Direct2D1.Bitmap1 d2dBitmap)
        {
            // Get the bitmap properties
            var size = d2dBitmap.PixelSize;
            var width = size.Width;
            var height = size.Height;

            // Create a GDI bitmap
            var gdiBitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            // Lock the bitmap for writing
            var bitmapData = gdiBitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, width, height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            try
            {
                // Copy the bitmap data using CopyFromMemory instead of CopyToMemory
                d2dBitmap.CopyFromMemory(bitmapData.Scan0, bitmapData.Stride);
            }
            finally
            {
                // Unlock the bitmap
                gdiBitmap.UnlockBits(bitmapData);
            }

            return gdiBitmap;
        }

        private SharpDX.Direct2D1.Bitmap1 CreateD2DBitmap(System.Drawing.Bitmap bitmap)
        {
            try
            {
                // Create bitmap properties
                var bitmapProperties = new SharpDX.Direct2D1.BitmapProperties1(
                    new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    SharpDX.Direct2D1.BitmapOptions.None);

                // Create the bitmap
                var size = new SharpDX.Size2(bitmap.Width, bitmap.Height);
                var d2dBitmap = new SharpDX.Direct2D1.Bitmap1(_d2dContext, size, bitmapProperties);

                // Lock the bitmap for reading
                var bitmapData = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                try
                {
                    // Copy the bitmap data
                    d2dBitmap.CopyFromMemory(bitmapData.Scan0, bitmapData.Stride);
                }
                finally
                {
                    // Unlock the bitmap
                    bitmap.UnlockBits(bitmapData);
                }

                return d2dBitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateD2DBitmap failed: {ex.Message}");
                throw;
            }
        }

        private void PreRender(Func<dynamic, bool> preRenderCallback, SvgDocument svgDocument)
        {
            if (preRenderCallback != null)
            {
                // Process all elements recursively
                ProcessElements(svgDocument.Children, preRenderCallback);
            }
        }

        private void ProcessElements(IEnumerable<SvgElement> elements, Func<dynamic, bool> preRenderCallback)
        {
            try
            {
                foreach (var element in elements)
                {
                    try
                    {
                        // Process the current element
                        if (element != null)
                        {
                            // Call the callback for this element
                            bool shouldContinue = preRenderCallback(element);
                            
                            // Process child elements regardless of the callback result
                            if (element.Children != null && element.Children.Count > 0)
                            {
                                ProcessElements(element.Children, preRenderCallback);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Error processing element {element?.ID}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in ProcessElements: {ex.Message}");
            }
        }

        public bool IsMouseOverInteractiveElement(PointF position)
        {
            foreach (var element in _ElementsClickEvent)
            {
                if (element.Value.Contains(position))
                {
                    return true;
                }
            }
            return false;
        }

        public void HandleSvgClick(PointF position)
        {
            foreach (var element in _ElementsClickEvent)
            {
                if (element.Value.Contains(position))
                {
                    _svgClickHandler?.Invoke(element.Key);
                    break;
                }
            }
        }

        public void Dispose()
        {
            _defaultBrush?.Dispose();
            // _targetBitmap?.Dispose(); // Commented out as it's not used
            _d2dContext?.Dispose();
            _d2dDevice?.Dispose();
            // _swapChain?.Dispose(); // Commented out as it's not used
            _d3dContext?.Dispose();
            _d3dDevice?.Dispose();
            _dxgiDevice?.Dispose();
            _d2dFactory?.Dispose();
            _cachedBitmap?.Dispose();

            foreach (var geometry in _geometryCache.Values)
            {
                geometry.Dispose();
            }
            _geometryCache.Clear();

            foreach (var brush in _brushCache.Values)
            {
                brush.Dispose();
            }
            _brushCache.Clear();
        }
    }
}
