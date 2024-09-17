using System;
using System.Collections;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using nanoFramework.Networking;
using nanoFramework.WebServer;
using System.Device.Wifi;

namespace MagicMonitor.RestApi
{
    public class RestApiServer
    {
        private static string WifiSsid = "SCC Inmate Wifi";
        private static string WifiPassword = "mj41paasaap14";

        public static void SetupWifi(string ssid, string password)
        {
            WifiSsid = ssid;
            WifiPassword = password;
        }

        public static void Start()
        {
            try
            {

                Debug.WriteLine("Waiting for network up and IP address...");
                bool success;
                CancellationTokenSource cs = new(60000);
                success = WifiNetworkHelper.ConnectDhcp(WifiSsid, WifiPassword, requiresDateTime: true, token: cs.Token);
                Debug.WriteLine("Has WIFI: True");

                if (!success)
                {
                    Debug.WriteLine($"Can't get a proper IP address and DateTime, error: {WifiNetworkHelper.Status}.");
                    if (WifiNetworkHelper.HelperException != null)
                    {
                        Debug.WriteLine($"Exception: {WifiNetworkHelper.HelperException}");
                    }

                    return;
                }
                else
                { 
                    Debug.WriteLine($"IP Address Acquired: {IPGlobalProperties.GetIPAddress().MapToIPv4()}");
                }

                try
                {
                    var controllersList = GetControllersList();

                    // Instantiate a new web server on port 80.
                    using var server = new WebServer(80, HttpProtocol.Http, controllersList);
                    
                    server.CommandReceived += ServerOnCommandReceived;
                    server.WebServerStatusChanged += ServerOnWebServerStatusChanged;

                    void ServerOnWebServerStatusChanged(object obj, WebServerStatusEventArgs e)
                    {
                        Debug.WriteLine($"Server Status Changed: {(e.Status == WebServerStatus.Running ? "Running" : "Stopped")}");
                    }

                    void ServerOnCommandReceived(object obj, WebServerEventArgs e)
                    {
                        Debug.WriteLine($"Command Received: {e.Context.Request.Url.OriginalString}");
                    }



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

        private static Type[] GetControllersList()
        {
            var controllersList = new ArrayList();

            try
            {
                var allTypes = Assembly.GetAssembly(typeof(IRestApiController)).GetTypes();
                Debug.WriteLine($"List of Types in {Assembly.GetAssembly(typeof(IRestApiController))}");

                foreach (var type in allTypes)
                {
                    Debug.WriteLine($"Type {type.Name}...");
                    if (type.FullName.EndsWith("RestController"))
                    {
                        Debug.WriteLine($"Found Controller: {type.Name}");
                        controllersList.Add(type);
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception getting Rest Controllers: {ex.Message}");
            }

            return (Type[]) controllersList.ToArray(typeof(Type));
        }
    }
}