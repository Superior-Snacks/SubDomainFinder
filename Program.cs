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
            /*if (args.Length != 1)
            {
                Console.WriteLine("usage subDomainFinder.exe domains");
                return;
            }
            string domain = args[0];*/ //get domain from args text
            string domain = "landsbankinn.is";
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
            var masterList = previous.Union(discovered).OrderBy(x => x).ToList();
            File.WriteAllLines(filePath, masterList); // add all to file since they are no longer new

            Console.WriteLine($"Total subdomains found: {discovered.Count}"); //logging
            Console.WriteLine($"New subdomains discovered: {newDomains.Count}");

            foreach (var sub in newDomains) // show the new domains
                Console.WriteLine($"[NEW] {sub}");
        }
        static async Task<HashSet<string>> GetSubdomainsFromCrtSh(string domain) //static(only used in this script) async lest me use await task(async method)
        {
            var subs = new HashSet<string>(); //return group
            string url = $"https://crt.sh/?q=%25.{domain}&output=json"; // url encoding for the api
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("subDomainFinder/1.0");

            var response = await client.GetStringAsync(url);
            var json = JsonDocument.Parse(response); //json spesific reader

            foreach (var entry in json.RootElement.EnumerateArray())
            {
                if (!entry.TryGetProperty("name_value", out var nameValue)) // maybe bad formatting must protect
                    continue;
                var names = nameValue.GetString().Split('\n'); //may be multiple names per
                foreach (var name in names)
                {
                    if (name.EndsWith(domain))
                    {
                        subs.Add(name.Trim().ToLower());
                    }
                }
            }
            return subs;
        }
    }
public static class DiscordReporter
    {
        private static readonly string webHookUrl = "https://discord.com/api/webhooks/1470497653417836605/3LbZAkJTydoE5uNl-NEwxSTNvNszuZRTkAy3oJY3QQEU2YQwiEegpCW2cAmCyKHgihE7";

        private static readonly HttpClient client = new HttpClient();
    }
}
