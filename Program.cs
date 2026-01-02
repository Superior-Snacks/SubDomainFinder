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
                Thread.Sleep(400);
                Console.WriteLine(item);
            }
        }
    }
}