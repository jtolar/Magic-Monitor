using System;
using System.Diagnostics;
using System.Threading;
using MagicMonitor.RestApi;

namespace MagicMonitor
{
    public class Program
    {
        public static void Main()
        {
            RestApiServer.Start();

            Debug.WriteLine("Hello from nanoFramework!");

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
