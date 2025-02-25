using Rev76.DataModels;
using Rev86.Core.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static Rev76.Windows.Win32;

namespace Rev76.Windows
{
    public abstract class OverlayWindow : IDisposable
    {
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

        public IntPtr HWND;
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Icon Icon { get; }

        public Rectangle Size
        {
            get
            {
                return new Rectangle(X, Y, Width, Height);
            }
        }

        public int FPS
        {
            get
            {
                return _FPS;
            }
            set
            {
                if (value < 1)
                {
                    _FPS = 1;
                }
                else
                {
                    _FPS = value;
                }
            }
        }

        public float Scale { get; set; }

        private bool _ShowInTaskbar;

        public bool ShowInTaskbar
        {
            get { return _ShowInTaskbar; }
            set
            {
                _ShowInTaskbar = value;
                ShowWindowInTaskbar(value);
            }
        }

        public GameData Data { get; set; }

        public abstract string Title { get; }
        public abstract bool Visible { get; }
        protected virtual void OnMouseClick(PointF position) { }
        protected virtual bool HitTest(PointF position) => false;

        private WndProcDelegate _wndProcDelegate;
        private string _ClassName;

        private IntPtr _HDC;
        private System.Drawing.Graphics _Graphics;
        private Bitmap _BufferBitmap;
        private System.Drawing.Graphics _BufferGraphics;

        private bool _IsRunning = true;
        private int _FPS = 10;
        private bool _NeedsRedraw = true;

        private bool _Disposing = false;

        protected readonly Dictionary<string, Brush> _Brushes = new Dictionary<string, Brush>();
        protected readonly Dictionary<string, Font> _Fonts = new Dictionary<string, Font>();

        public OverlayWindow(int x, int y, int width, int height, float scale, Icon icon)
        {
            Icon = icon;
            X = x;
            Y = y;

            Width = (int)(width * scale);
            Height = (int)(height * scale);

            Scale = scale;
            _ClassName = $"OverlayWindow_{Guid.NewGuid()}";

            RegisterWindowClass();
            CreateLayeredWindow();
            InitializeGraphics();
        }

        private void RegisterWindowClass()
        {
            _wndProcDelegate = WndProc;
            IntPtr iconHandle = Icon != null ? Icon.Handle : IntPtr.Zero;

            Win32.WNDCLASSEX wndClass = new Win32.WNDCLASSEX
            {
                cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX)),
                style = 0,
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = IntPtr.Zero,
                hCursor = Win32.LoadCursor(IntPtr.Zero, IDC_ARROW),
                hbrBackground = IntPtr.Zero,
                lpszMenuName = null,
                lpszClassName = _ClassName,
                hIconSm = iconHandle
            };

            if (RegisterClassEx(ref wndClass) == IntPtr.Zero)
            {
                throw new Exception("Failed to register window class.");
            }
        }

        private void CreateLayeredWindow()
        {
            HWND = Win32.CreateWindowEx(
                Win32.WS_EX_LAYERED | Win32.WS_EX_TOPMOST,
                _ClassName,
                this.Title,
                Win32.WS_POPUP,
                X, Y, Width, Height,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            if (HWND == IntPtr.Zero)
            {
                throw new Exception("Failed to create layered window.");
            }

            WindowManager.AddWindow(this);

            Win32.ShowWindow(HWND, 1);
            Win32.UpdateWindow(HWND);
        }

        public void Show()
        {
            _IsRunning = true;
            Render();
        }

        public void Close()
        {
            _IsRunning = false;
            OnClose();
        }

        public virtual void OnClose()
        {
        }

        private void UpdateTheLayeredWindow()
        {
            IntPtr hBitmap = _BufferBitmap.GetHbitmap(Color.FromArgb(0, 0, 0, 0));

            try
            {
                if (!WindowManager.Windows.ContainsKey(HWND))
                {
                    this.Dispose();
                    return;
                }
                Win32.SIZE size = new Win32.SIZE { CX = Width, CY = Height };
                Win32.POINT pointSource = new Win32.POINT { X = 0, Y = 0 };
                Win32.POINT pointDest = new Win32.POINT { X = X, Y = Y };

                Win32.BLENDFUNCTION blend = new Win32.BLENDFUNCTION
                {
                    BlendOp = AC_SRC_OVER,
                    BlendFlags = 0,
                    SourceConstantAlpha = 255,
                    AlphaFormat = AC_SRC_ALPHA
                };

                IntPtr screenDC = Win32.GetDC(IntPtr.Zero);
                IntPtr memDC = Win32.CreateCompatibleDC(screenDC);
                IntPtr oldBitmap = Win32.SelectObject(memDC, hBitmap);

                try
                {
                    bool result = Win32.UpdateLayeredWindow(
                        HWND,
                        screenDC,
                        ref pointDest,
                        ref size,
                        memDC,
                        ref pointSource,
                        0,
                        ref blend,
                        Win32.LWA_ALPHA
                    );

                    if (!result)
                    {
                        throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "UpdateLayeredWindow (LWA_ALPHA) failed.");
                    }
                }
                finally
                {
                    Win32.SelectObject(memDC, oldBitmap);
                    Win32.DeleteDC(memDC);
                    Win32.ReleaseDC(IntPtr.Zero, screenDC);
                }
            }
            finally
            {
                Win32.DeleteObject(hBitmap);
            }
        }

        private static IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            int mouseX;
            int mouseY;
            PointF point = new PointF(0, 0);

            if (WindowManager.Windows.TryGetValue(hwnd, out OverlayWindow instance))
            {
                switch (msg)
                {
                    case WM_DESTROY:
                        instance._IsRunning = false;
                        WindowManager.Windows.Remove(hwnd);
                        Win32.PostQuitMessage(0);
                        return IntPtr.Zero;

                    case Win32.WM_NCHITTEST:
                        mouseX = lParam.ToInt32() & 0xFFFF;
                        mouseY = (lParam.ToInt32() >> 16) & 0xFFFF;
                        point = new PointF(mouseX - instance.X, mouseY - instance.Y);

                        if (instance.HitTest(point))
                        {
                            return (IntPtr)Win32.HTCLIENT;
                        }

                        return (IntPtr)Win32.HTCAPTION;

                    case WM_WINDOWPOSCHANGED:
                        Win32.WINDOWPOS pos = Marshal.PtrToStructure<Win32.WINDOWPOS>(lParam);
                        instance.X = pos.x;
                        instance.Y = pos.y;
                        instance.Width = pos.cx;
                        instance.Height = pos.cy;

                        Win32.SetWindowPos(hwnd, Win32.HWND_TOPMOST, 0, 0, 0, 0, Win32.SWP_NOACTIVATE | Win32.SWP_NOMOVE | Win32.SWP_NOSIZE);
                        return IntPtr.Zero;

                    case WM_EXITSIZEMOVE:
                        instance.OnHandOverToGame();
                        RevConfig.Instance.UpdateWidgetPosition(instance.GetType().Name, instance.X, instance.Y);
                        return IntPtr.Zero;

                    case WM_LBUTTONDOWN:
                        mouseX = lParam.ToInt32() & 0xFFFF;
                        mouseY = (lParam.ToInt32() >> 16) & 0xFFFF;
                        point = new PointF(mouseX, mouseY);
                        instance.OnMouseClick(point);
                        return IntPtr.Zero;
                }
            }

            return DefWindowProc(hwnd, msg, wParam, lParam);
        }

        private void InitializeGraphics()
        {
            _HDC = GetDC(HWND);
            if (_BufferBitmap == null)
            {
                _BufferBitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
                _BufferBitmap.SetResolution(90, 90);
            }
         
            _BufferGraphics = System.Drawing.Graphics.FromImage(_BufferBitmap);

            _BufferGraphics.SmoothingMode = SmoothingMode.AntiAlias;
            _BufferGraphics.CompositingQuality = CompositingQuality.HighQuality;
            _BufferGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            _BufferGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            _Graphics = System.Drawing.Graphics.FromHdc(_HDC);
        }

        private void Render()
        {

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            long frameTime = 1000 / FPS;  // Time per frame in milliseconds
            long lastFrameTime = 0;
            int frameCount = 0;
            long lastFpsUpdateTime = 0;
            int currentFps = 0;

            OnGraphicsSetup(_BufferGraphics);

            while (_IsRunning && Win32.IsWindow(HWND))
            {
                if (!_IsRunning) return;
                long currentTime = stopwatch.ElapsedMilliseconds;
                long timeDiff = currentTime - lastFrameTime;

                Task.Delay(1000 / FPS);

                if (timeDiff >= frameTime)
                {
                    lastFrameTime = currentTime;
                    frameCount++;

                    _BufferGraphics.Clear(Color.Transparent);

                    MSG msg;
                    while (PeekMessage(out msg, IntPtr.Zero, 0, 0, Win32.PM_REMOVE))
                    {
                        if (!_IsRunning) return;

                        TranslateMessage(ref msg);

                        if (msg.message == WM_QUIT)
                        {
                            _IsRunning = false;
                            break;
                        }

                        if (msg.message == WM_PAINT)
                        {
                            _NeedsRedraw = true;
                            break;
                        }

                        DispatchMessage(ref msg);
                    }


                    if (_NeedsRedraw && Visible)
                    {
                        OnRender(_BufferGraphics);
                    }

                    // Calculate and display FPS
                    if (currentTime - lastFpsUpdateTime >= 1000)
                    {
                        currentFps = frameCount;
                        frameCount = 0;
                        lastFpsUpdateTime = currentTime;
                    }

                    DrawFps(_BufferGraphics, currentFps);

                    UpdateTheLayeredWindow();

                    // Calculate the remaining time until the next frame
                    long renderTime = stopwatch.ElapsedMilliseconds - currentTime;
                    long remainingTime = frameTime - renderTime;

                    if (remainingTime > 0)
                    {
                        Thread.Sleep((int)remainingTime);
                    }
                }
                else
                {
                    Thread.Sleep(1); // yield CPU to other processes.
                }
            }
        }

        private void DrawFps(System.Drawing.Graphics graphics, int fps)
        {
          
           
            graphics.DrawString($"FPS: {fps}", _Fonts["consolas"], _Brushes["fps"], 10, 10); // Draw FPS at top-left
            
        }

        private void DrawWindowBackground()
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                int cornerRadius = 20;
                path.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90);
                path.AddArc(Width - cornerRadius, 0, cornerRadius, cornerRadius, 270, 90);
                path.AddArc(Width - cornerRadius, Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
                path.AddArc(0, Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
                path.CloseFigure();

                _BufferGraphics.FillPath(_Brushes["background"], path);
            }
        }

        private void DrawWindowOutline()
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                int cornerRadius = 20;
                path.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90);
                path.AddArc(Width - cornerRadius, 0, cornerRadius, cornerRadius, 270, 90);
                path.AddArc(Width - cornerRadius, Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
                path.AddArc(0, Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
                path.CloseFigure();

                using (Pen pen = new Pen(Color.FromArgb(20, 0, 0, 0), 2))
                {
                    pen.DashStyle = DashStyle.Solid;
                    _BufferGraphics.DrawPath(pen, path);
                }
            }
        }

        protected virtual void OnGraphicsSetup(System.Drawing.Graphics gfx)
        {
            _Brushes["background"] = new SolidBrush(Color.FromArgb(60, 20, 20, 0));
            _Brushes["fps"] = new SolidBrush(Color.FromArgb(255, 255, 0, 0));
            _Fonts["consolas"] = new Font("Consolas", 9, FontStyle.Regular);
        }

        protected virtual void OnRender(System.Drawing.Graphics gfx)
        {
            DrawWindowBackground();
            //DrawWindowOutline();
        }

        protected virtual void OnHandOverToGame()
        {
            if (Visible)
            {
                WindowManager.HandOverToGame();
            }
        }

        protected virtual void OnGraphicsDestroyed(System.Drawing.Graphics gfx)
        {
            foreach (var pair in _Brushes) pair.Value.Dispose();
        }

        private void ShowWindowInTaskbar(bool show)
        {
            int exStyle = Win32.GetWindowLong(this.HWND, Win32.GWL_EXSTYLE);

            if (show)
            {
                exStyle |= Win32.WS_EX_APPWINDOW;
                exStyle &= ~Win32.WS_EX_TOOLWINDOW; // Remove tool window style
                Win32.SetParent(this.HWND, Win32.GetDesktopWindow()); // Ensure no hidden owner
            }
            else
            {
                exStyle &= ~Win32.WS_EX_APPWINDOW;
                exStyle |= Win32.WS_EX_TOOLWINDOW; // Make it a tool window
                Win32.SetParent(this.HWND, IntPtr.Zero); // Hide from taskbar
            }

            Win32.SetWindowLong(this.HWND, Win32.GWL_EXSTYLE, exStyle);
        }

        public void Dispose()
        {
            _IsRunning = false;

            if (_Disposing) return;

            _Disposing = true;

            Task.Delay(1000).Wait();

            OnGraphicsDestroyed(_BufferGraphics);

            if (_Graphics != null) _Graphics.Dispose();
            if (_BufferGraphics != null) _BufferGraphics.Dispose();
            if (_BufferBitmap != null) _BufferBitmap.Dispose();

            WindowManager.RemoveWindow(this);

            if (_HDC != IntPtr.Zero)
            {
                ReleaseDC(HWND, _HDC);
                _HDC = IntPtr.Zero;
            }

            if (HWND != IntPtr.Zero && Win32.IsWindow(HWND))
            {
                DestroyWindow(HWND);
                HWND = IntPtr.Zero;
            }
        }
    }
}
