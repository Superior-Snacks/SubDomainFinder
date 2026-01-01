using System.Net;
using System;
using System.Threading.Tasks;

namespace subDomainFinder
{
    class Project
    {
        static async Task Main(string[] args)
        {
            string domain = "youtube.com";
            IPAddress[] ips = await Dns.GetHostAddressesAsync(domain);
            foreach (var item in ips)
            {
                Console.WriteLine(item);
            }
        }
    }
}