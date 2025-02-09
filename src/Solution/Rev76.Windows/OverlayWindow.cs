﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using static Rev76.Windows.Win32;

namespace Rev76.Windows
{
    public abstract class OverlayWindow : IDisposable
    {
        private WndProcDelegate _wndProcDelegate;

        public IntPtr HWND;
        private IntPtr _HDC;
        private System.Drawing.Graphics _Graphics;
        private Bitmap _BufferBitmap;
        private System.Drawing.Graphics _BufferGraphics;
        private bool _IsRunning = true;
        private bool _NeedsRedraw = true;
        private string _ClassName;

        protected Dictionary<string, Brush> _Brushes = new Dictionary<string, Brush>();
        protected readonly Dictionary<string, Font> _Fonts = new Dictionary<string, Font>();
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        
        public Icon Icon { get;  }

        private int _FPS = 4;
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

        protected abstract string Title { get; }
        protected abstract bool Visible { get; }

        public OverlayWindow(int x, int y, int width, int height, Icon icon)
        {
            Icon = icon;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _ClassName = $"OverlayWindow_{Guid.NewGuid()}";

            RegisterWindowClass();
            CreateLayeredWindow();
            InitializeGraphics();

            //Render();
        }

        private void RegisterWindowClass()
        {

            _wndProcDelegate = WndProc;
            IntPtr iconHandle = Icon!= null ? Icon.Handle : IntPtr.Zero;

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
            //Win32.ShowWindow(HWND, 5);
            //Win32.UpdateWindow(HWND);

            Render();
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
            PointF svgPoint = new PointF(0, 0);
            if (WindowManager.Windows.TryGetValue(hwnd, out OverlayWindow instance))
            {
                switch (msg)
                {
                    case WM_DESTROY:
                        Win32.PostQuitMessage(0);
                        WindowManager.Windows.Remove(hwnd);

                        return IntPtr.Zero;

                    case Win32.WM_NCHITTEST:
                        instance.DrawWindowOutline();
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
                        return IntPtr.Zero;

                }
            }

            return DefWindowProc(hwnd, msg, wParam, lParam);
        }

        private void InitializeGraphics()
        {
            _HDC = GetDC(HWND);
            _BufferBitmap = new Bitmap(Width * 2, Height * 2, PixelFormat.Format32bppArgb);
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

            OnGraphicsSetup(_BufferGraphics);

            while (_IsRunning && Win32.IsWindow(HWND))
            {
                long currentTime = stopwatch.ElapsedMilliseconds;
                long timeDiff = currentTime - lastFrameTime;

                System.Threading.Thread.Sleep(1000 / FPS);

                if (timeDiff >= frameTime)
                {
                    lastFrameTime = currentTime;

                    _BufferGraphics.Clear(Color.Transparent);

                    MSG msg;
                    while (PeekMessage(out msg, IntPtr.Zero, 0, 0, Win32.PM_REMOVE))
                    {

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
                    else
                    {
                        Thread.Sleep(100);
                    }

                    UpdateTheLayeredWindow();
                }
            }
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

                using (Pen pen = new Pen(Color.FromArgb(225, 255, 255, 255), 1))
                {
                    pen.DashStyle = DashStyle.Dot;
                    _BufferGraphics.DrawPath(pen, path);
                }
            }
        }

        protected virtual void OnGraphicsSetup(System.Drawing.Graphics gfx)
        {
            _Brushes["background"] = new SolidBrush(Color.FromArgb(60, 0, 0, 0));
        }

        protected virtual void OnRender(System.Drawing.Graphics gfx)
        {
            DrawWindowBackground();
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

        public void Dispose()
        {
            _IsRunning = false;

            OnGraphicsDestroyed(_BufferGraphics);

            if (_Graphics != null) _Graphics.Dispose();
            if (_BufferGraphics != null) _BufferGraphics.Dispose();
            if (_BufferBitmap != null) _BufferBitmap.Dispose();

            WindowManager.Remove(this);

            if (_HDC != IntPtr.Zero)
            {
                ReleaseDC(HWND, _HDC);
                _HDC = IntPtr.Zero;
            }

            if (HWND != IntPtr.Zero)
            {
                DestroyWindow(HWND);
                HWND = IntPtr.Zero;
            }
        }
    }
}
