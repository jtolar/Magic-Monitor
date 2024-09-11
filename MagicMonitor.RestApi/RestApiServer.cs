using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using nanoFramework.Networking;
using nanoFramework.WebServer;
#if HAS_WIFI
using System.Device.Wifi;
#endif

namespace MagicMonitor.RestApi
{
    public class RestApiServer
    {

#if HAS_WIFI
        private static string MySsid = "SCC Inmate Wifi";
        private static string MyPassword = "mj41paasaap14";
#endif

        //private static bool _isConnected = false;

        public static void Start()
        {
            Debug.WriteLine("Hello from a webserver!");

            try
            {

                int connectRetry = 0;

                Debug.WriteLine("Waiting for network up and IP address...");
                bool success;
                CancellationTokenSource cs = new(60000);
#if HAS_WIFI
                success = WifiNetworkHelper.ConnectDhcp(MySsid, MyPassword, requiresDateTime: true, token: cs.Token);
#else
                success = NetworkHelper.SetupAndConnectNetwork(cs.Token, true);
#endif
                if (!success)
                {
                    Debug.WriteLine($"Can't get a proper IP address and DateTime, error: {WifiNetworkHelper.Status}.");
                    if (WifiNetworkHelper.HelperException != null)
                    {
                        Debug.WriteLine($"Exception: {WifiNetworkHelper.HelperException}");
                    }

                    return;
                }

                var allTypes = Assembly.GetAssembly(typeof(IRestApiController)).GetTypes();
                var controllersList = new ArrayList();
                
                foreach (Type type in allTypes)
                {
                    if (type.IsInstanceOfType(typeof(IRestApiController)))
                    {
                        controllersList.Add(type);
                    }
                }

                // Instantiate a new web server on port 80.
                using WebServer server = new WebServer(80, HttpProtocol.Http, (Type[])controllersList.ToArray(typeof(Type)));

                // Start the server.
                server.Start();

                Thread.Sleep(Timeout.Infinite);

            }
            catch (Exception ex)
            {

                Debug.WriteLine($"{ex}");
            }
        }
    }
}