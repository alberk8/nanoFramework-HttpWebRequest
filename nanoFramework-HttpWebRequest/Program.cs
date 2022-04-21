using nanoFramework.Hardware.Esp32;
using nanoFramework.Networking;
using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace nanoFramework_HttpWebRequest
{
    public class Program
    {
        private static string MySsid = "ssid";
        private static string MyPassword = "password";

      

        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            Debug.WriteLine("Waiting for network up and IP address...");
            bool success;
            CancellationTokenSource cs = new(60000);
            
            success =    WiFiNetworkHelper.ConnectDhcp(MySsid, MyPassword, requiresDateTime: true, token: cs.Token);


            if (!success)
            {
                Debug.WriteLine($"Can't get a proper IP address and DateTime, error: {WiFiNetworkHelper.Status}.");
              
                return;
            }

            IPHostEntry hostEntry = Dns.GetHostEntry("example.com");
            var hosts = hostEntry.AddressList;
            if (hosts.Length > 0)
                Debug.WriteLine($"IP Address: {hosts[0].ToString()}");

            //HTTPS - uncomment next two lines
            string url = "https://global-root-ca.chain-demos.digicert.com";
            X509Certificate rootCACert = new X509Certificate(Resource.GetBytes(Resource.BinaryResources.DigiCertGlobalRootCA));

            //HTTP
            //string url = "http://example.com";

            for (int i = 0; i < 20; i++)
            {
                try
                {
                    Debug.WriteLine($"Performing Http request to: {url}");

                    // perform the request as a HttpWebRequest
                    using var httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
                    httpWebRequest.KeepAlive = false;
                    httpWebRequest.Method = "GET";

                    Debug.WriteLine(httpWebRequest.RequestUri.Host);
                    Debug.WriteLine(httpWebRequest.RequestUri.AbsolutePath);
                  

                    // For HTTPS - uncomment next two lines
                    httpWebRequest.SslProtocols = System.Net.Security.SslProtocols.Tls12;
                    httpWebRequest.HttpsAuthentCert = rootCACert;

                    // get the response as a HttpWebResponse
                    // wrap the response object with a using statement to make sure that it's disposed


                    uint totalMem, totalFree, freeBlock;
                    NativeMemory.GetMemoryInfo(NativeMemory.MemoryType.Internal, out totalMem, out totalFree, out freeBlock);
                    Debug.WriteLine($"Internal Free Memory {totalFree}");

                    using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                    {

                        // wrap the response stream on a using statement to make sure that it's disposed
                        using (var stream = httpWebResponse.GetResponseStream())
                        {

                            // read response in chunks of 1k

                            byte[] buffer = new byte[1024];
                            int bytesRead = 0;
                            //int streamLength = (int)stream.Length;

                            Debug.WriteLine("Http response follows");
                            Debug.WriteLine(">>>>>>>>>>>>>");


                            do
                            {
                                bytesRead = stream.Read(buffer, 0, buffer.Length);
                                Debug.Write(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                            }
                            while (bytesRead >= buffer.Length);
                        }

                        Debug.WriteLine(">>>>>>>>>>>>>");
                        Debug.WriteLine("End of Http response");

                        NativeMemory.GetMemoryInfo(NativeMemory.MemoryType.Internal, out totalMem, out totalFree, out freeBlock);
                        Debug.WriteLine($"Internal Free Memory {totalFree}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("\n#######################\n");
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.InnerException.Message);
                }


                Thread.Sleep(10_000);
            }

            Thread.Sleep(Timeout.Infinite);

            
        }
    }
}
