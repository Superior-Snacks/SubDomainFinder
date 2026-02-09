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
//string baseIp = "89.104.145.151";

namespace subDomainFinder
{
class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("usage subDomainFinder.exe domains");
                return;
            }
            string domain = args[0]; //get domain from args text
            string filePath = $"{domain}_subs.txt"; //create file path string for domain or find the path if domain already found

            var discovered = await GetSubdomainsFromCrtSh(domain); //main call in file gets list from the function

            HashSet<string> previous = File.Exists(filePath) // a cleaner way to check if there is a file for 
                ? new HashSet<string>(File.ReadAllLines(filePath))// if the file exists
                : new HashSet<string>(); //else

            var newDomains = new HashSet<string>(); //empty init
            foreach (var item in discovered)
            {
                if (!previous.Contains(item))//if the item is not in file add it, all will be added if first time
                {
                    newDomains.Add(item);
                }
            }
            File.WriteAllLines(filePath, discovered); // add all to file since they are no longer new
        }
    }
}
