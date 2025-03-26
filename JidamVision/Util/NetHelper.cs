using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Policy;

namespace JidamVision.Util
{
    public static class NetHelper
    {
        public static bool IsIPAddress(string ipString)
        {
            IPAddress ipAddress;
            return IPAddress.TryParse(ipString, out ipAddress);
        }

        public static bool Ping(string ipString, int timeout = 1000)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            // Use the default Ttl value which is 128, but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);

            try
            {
                PingReply reply = pingSender.Send(ipString, timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                {
                    //Console.WriteLine("Address: {0}", reply.Address.ToString());
                    //Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
                    //Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
                    //Console.WriteLine("Don't fragment: {0}", reply.Options.DontFragment);
                    //Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
                    return true;
                }
            }
            catch (Exception)
            {
                // Silent catching
            }
            return false;
        }

        /// <summary>
        /// Determine whether IP address is local iP or not
        /// </summary>
        /// <param name="host">IP address to check</param>
        /// <returns>True if local IP, otherwise false</returns>
        public static bool IsLocalIpAddress(string host)
        {
            try
            {
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // Test if any host IP equals to any local IP or to localhost
                foreach (IPAddress hostIP in hostIPs)
                {
                    // Is localhost
                    if (IPAddress.IsLoopback(hostIP))
                        return true;

                    // Is local address
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP))
                            return true;
                    }
                }
            }
            catch (Exception)
            {
                // Silent catching
            }

            return false;
        }

        public static IEnumerable<string> GetLocalIpAddress()
        {
            List<string> ipAddresses = new List<string>();

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddresses.Add(ip.ToString());
                }
            }
            return ipAddresses;
        }
    }
}
