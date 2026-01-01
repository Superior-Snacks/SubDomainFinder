using System.Net;
using System;
using System.Threading.Tasks;

namespace subDomainFinder
{
    class Project
    {
        static async Task Main(string[] args)
        {
            string domain = "islandsbanki.is";
            IPAddress[] ips = await Dns.GetHostAddressesAsync(domain);
            foreach (var item in ips)
            {
                Console.WriteLine(item);
            }
        }
        public async Task ResolveHostnameAsync(string domain)
        {
            try
            {
                // Get the host entry for the domain
                IPHostEntry entry = await Dns.GetHostEntryAsync(domain);

                Console.WriteLine($"Host Name: {entry.HostName}");

                foreach (IPAddress ip in entry.AddressList)
                {
                    // You can filter by AddressFamily to separate IPv4 and IPv6
                    string type = ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? "IPv4" : "IPv6";
                    Console.WriteLine($"{type}: {ip}");
                }
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Console.WriteLine($"DNS Error: {e.Message}"); // e.g., Host not found
            }
        }
    }
}