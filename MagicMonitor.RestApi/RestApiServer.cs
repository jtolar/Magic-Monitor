#define  HAS_WIFI


using System;
using System.Collections;
using System.Diagnostics;
using System.Net.NetworkInformation;
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
        private static string WifiSsid = "SCC Inmate Wifi";
        private static string WifiPassword = "mj41paasaap14";
#endif

//private static bool _isConnected = false
//

#if HAS_WIFI
        public static void SetupWifi(string ssid, string password)
        {
            WifiSsid = ssid;
            WifiPassword = password;
        }
#endif
        public static void Start()
        {
            try
            {

                int connectRetry = 0;

                Debug.WriteLine("Waiting for network up and IP address...");
                bool success;
                CancellationTokenSource cs = new(60000);
#if HAS_WIFI
                success = WifiNetworkHelper.ConnectDhcp(WifiSsid, WifiPassword, requiresDateTime: true, token: cs.Token);
                Debug.WriteLine("Has WIFI: True");
#else
                success = NetworkHelper.SetupAndConnectNetwork(cs.Token, true);
#endif
                if (!success)
                {
                    Debug.WriteLine($"Can't get a proper IP address and DateTime, error: {WifiNetworkHelper.Status}.");
#if HAS_WIFI
                    if (WifiNetworkHelper.HelperException != null)
                    {
                        Debug.WriteLine($"Exception: {WifiNetworkHelper.HelperException}");
                    }
#else
                    if (NetworkHelper.HelperException != null)
                    {
                        Debug.WriteLine($"Exception: {NetworkHelper.HelperException}");
                    }
#endif
                    return;
                }
                else
                { 
                    Debug.WriteLine($"IP Address Acquired: {IPGlobalProperties.GetIPAddress().MapToIPv4()}");
                }

                try
                {
                    var allTypes = Assembly.GetAssembly(typeof(IRestApiController)).GetTypes();
                    var controllersList = new ArrayList();

                    foreach (var type in allTypes)
                    {
                        if (type.IsInstanceOfType(typeof(IRestApiController)))
                        {
                            Debug.WriteLine($"Found Controller: {type.Name}");
                            controllersList.Add(type);
                        }
                    }

                    // Instantiate a new web server on port 80.
                    using var server = new WebServer(80, HttpProtocol.Http,
                        (Type[])controllersList.ToArray(typeof(Type)));

                    // Start the server.
                    server.Start();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                Thread.Sleep(Timeout.Infinite);

            }
            catch (Exception ex)
            {

                Debug.WriteLine($"{ex}");
            }
        }
    }
}