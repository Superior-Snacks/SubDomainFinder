using System.Net;
using System;
using DnsClient;
using System.Threading.Tasks;

namespace subDomainFinder
{
    class Project
    {
        static async Task Main(string[] args)
        {
            string domain = "youtube.com";
            IPAddress[] ips = await Dns.GetHostAddressesAsync(domain);
            IPHostEntry hostInfo = Dns.GetHostEntry(domain);
            foreach (IPAddress ip in hostInfo.AddressList)
            {
                Thread.Sleep(200);
                Console.WriteLine($" - IP: {ip}");
            }
            Console.ReadLine();
        }
        static void printSlow(string sentance)
        {
            for (int i = 0; i < sentance.Length; i++)
            {
                Thread.Sleep(10);
                Console.WriteLine(sentance[i]);
                Thread.Sleep(10);
            }
        }
    }
}