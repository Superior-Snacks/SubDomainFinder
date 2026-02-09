using DnsClient;
using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;


namespace subDomainFinder
{
class Program
    {
        static async Task Main()
        {
            string baseIp = "89.104.145.151";
            Console.WriteLine("\nScan Complete.");
        }
        static async Task<HashSet<string>> GetSubdomainsFromCrtSh(string domain)
        {
            return null;
        }


    }
}
