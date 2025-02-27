﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;

namespace Rev76.Windows
{
    public class SystemTrayIcon
    {

        private static IntPtr _hwnd;
        private static int _messageId;
        private static Win32.NOTIFYICONDATA _nid;
        
        private static IntPtr _hMenu;
        private Win32.WndProcDelegate _wndProcDelegate; // Keep a reference to the delegate

        private static Action _onClickAction;
        private static Dictionary<string, (Action, bool)> _menuItems = new Dictionary<string, (Action, bool)>();


        public void AddIcon(Icon icon, string tooltip, Action onClick)
        {

            _messageId = Win32.WM_USER + 100;

            Trace.TraceInformation("Creating system tray.");

            // Register the hidden window class
            RegisterWindowClass();

            // Create a hidden window
            _hwnd = Win32.CreateWindowEx(
                0,
                "TrayAppWndClass",   // Class name
                "TrayApp",           // Window name
                0,                   // Style
                0, 0, 0, 0,          // Position and size (hidden)
                IntPtr.Zero,         // Parent window
                IntPtr.Zero,         // Menu
                Win32.GetModuleHandle(null), // Instance handle
                IntPtr.Zero          // Additional parameters
            );

            if (_hwnd == IntPtr.Zero)
            {
                Trace.TraceError("Failed to create window.");
                return;
            }

            _nid = new Win32.NOTIFYICONDATA
            {
                cbSize = Marshal.SizeOf(typeof(Win32.NOTIFYICONDATA)),
                hWnd = _hwnd,
                uID = 1,
                uFlags = Win32.NIF_ICON | Win32.NIF_MESSAGE | Win32.NIF_TIP,
                uCallbackMessage = _messageId,
                hIcon = icon.Handle,
                szTip = tooltip
            };

            if (!Win32.Shell_NotifyIcon(Win32.NIM_ADD, ref _nid))
            {
                Trace.TraceError("Error adding icon to system tray.");
                return;
            }

            _onClickAction = onClick;

        }

        public void Show()
        {
            MessageLoop();
        }

        public void AddMenuSeparator()
        {
            _menuItems["-"] = (null, false);
         
        }

        public void AddMenuItem(string text, bool isChecked, Action clickAction)
        {
            _menuItems[text] = (clickAction, isChecked);
            
        }

        public void SetMenuItemChecked(string text, bool isChecked)
        {
            if (_menuItems.ContainsKey(text))
            {
                _menuItems[text] = (_menuItems[text].Item1, isChecked);
            }
        }


        private void RegisterWindowClass()
        {
            _wndProcDelegate = new Win32.WndProcDelegate(WndProc); 

            Win32.WNDCLASS wndClass = new Win32.WNDCLASS
            {
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
                lpszClassName = "TrayAppWndClass",
                hInstance = Win32.GetModuleHandle(null),
                hIcon = IntPtr.Zero,
                hCursor = Win32.LoadCursor(IntPtr.Zero, Win32.IDC_ARROW),
                hbrBackground = IntPtr.Zero
            };

            if (Win32.RegisterClass(ref wndClass) == IntPtr.Zero)
            {
                Trace.TraceError("Failed to register window class.");
            }
        }

        private static IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case Win32.WM_USER + 100:
                    switch ((uint)lParam)
                    {
                        case Win32.WM_CLOSE:
                            // Ignore the close button to prevent app termination
                            Win32.ShowWindow(hwnd, Win32.SW_HIDE);
                            return IntPtr.Zero;
                        case Win32.WM_DESTROY:
                            Win32.PostQuitMessage(0);
                            return IntPtr.Zero;
                        case Win32.WM_LBUTTONUP:
                            _onClickAction?.Invoke();
                            return IntPtr.Zero;
                        case Win32.WM_RBUTTONUP:
                            ShowContextMenu();
                            return IntPtr.Zero;
                    }
                    break;
            }

            return Win32.DefWindowProc(hwnd, msg, wParam, lParam);
        }

        private static void ShowContextMenu()
        {
            if (_menuItems.Count == 0) return;

            _hMenu = Win32.CreatePopupMenu();
            int id = 1;

            foreach (var item in _menuItems)
            {
                if (item.Key == "-") // If it's a separator
                {
                    Win32.AppendMenu(_hMenu, Win32.MF_SEPARATOR, 0, "");
                }
                else
                {
                    uint flags = item.Value.Item2 ? Win32.MF_STRING | Win32.MF_CHECKED : Win32.MF_STRING;
                    Win32.AppendMenu(_hMenu, flags, (uint)id, item.Key);
                }
                id++;
            }

            Win32.POINT pt;
            Win32.GetCursorPos(out pt);
            Win32.SetForegroundWindow(_hwnd);

            int cmd = (int)Win32.TrackPopupMenu(_hMenu, Win32.TPM_RETURNCMD | Win32.TPM_NONOTIFY, pt.X, pt.Y, 0, _hwnd, IntPtr.Zero);

            if (cmd > 0 && cmd <= _menuItems.Count)
            {
                string selectedText = _menuItems.Keys.ToArray()[cmd - 1]; // Retrieve menu item text
                _menuItems[selectedText].Item1?.Invoke(); // Call the action
            }

            Win32.PostMessage(_hwnd, Win32.WM_NULL, IntPtr.Zero, IntPtr.Zero);
            Win32.DestroyMenu(_hMenu);
        }




        private static void MessageLoop()
        {
            try
            {
                Win32.MSG msg;
                while (Win32.GetMessage(out msg, IntPtr.Zero, 0, 0))
                {
                    Win32.TranslateMessage(ref msg);
                    Win32.DispatchMessage(ref msg);
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        public static void RemoveIcon()
        {
            Win32.Shell_NotifyIcon(Win32.NIM_DELETE, ref _nid);
            Win32.DestroyWindow(_hwnd);
        }
    }
}
