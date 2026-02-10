using DnsClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dapper;
using Microsoft.Data.Sqlite;


namespace subDomainFinder
{
class Program
    {
        static async Task Main(string[] args)
        {
            Database.Initialize();
            /*if (args.Length != 1)
            {
                Console.WriteLine("usage subDomainFinder.exe domains");
                return;
            }
            string domain = args[0];*/ //get domain from args text
            string domain = "geoguessr.com";
            Console.WriteLine($"[+] Starting scan for: {domain}");

            var discovered = await GetSubdomainsFromCrtSh(domain); //main call in file gets list from the function

            var knownSubdomains = Database.GetExistingSubdomains(domain);

            var newDomains = new List<string>();
            foreach (var item in discovered)
            {
                if (!knownSubdomains.Contains(item))
                {
                    newDomains.Add(item);
                }
            }
            if (newDomains.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[+] Found {newDomains.Count} new subdomains!");

                // Print to console
                foreach (var sub in newDomains)
                {
                    Console.WriteLine($"[NEW] {sub}");
                }
                Console.ResetColor();

                // Send to Discord
                await DiscordReporter.SendReportAsync(domain, newDomains);

                // 6. Save to SQL
                // REPLACED: File.WriteAllLines(...)
                Console.WriteLine("[+] Saving to database...");
                Database.InsertSubdomains(domain, newDomains);
            }
            else
            {
                Console.WriteLine("[-] No new subdomains found.");
            }
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

        public static async Task SendReportAsync(string targetDomain, List<string> newSubdomains)
        {
            if (newSubdomains.Count == 0) return;
            var embed = new
            {
                title = $"🚨 New Targets Detected: {targetDomain}",
                description = $"Found **{newSubdomains.Count}** new subdomain(s).",
                color = 5763719,
                fields = new[] 
                {
                    new
                    {
                    name = "Subdomains",
                    value = FormatList(newSubdomains), // Discord has a 1024 char limit per field
                    inline = false
                    }
                },
                footer = new
                {
                    text = $"Scanner Bot • {DateTime.Now:HH:mm:ss}"
                }
            };

            var payload = new
            {
                username = "Recon Bot",
                avatar_url = "https://i.imgur.com/4M34hi2.png", // Optional: Custom bot icon
                embeds = new[] { embed }
            };
            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(webHookUrl, content);
                if (response.IsSuccessStatusCode)
                    Console.WriteLine("[+] Discord notification sent!");
                else
                    Console.WriteLine($"[-] Discord Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[-] Failed to send to Discord: {ex.Message}");
            }
        }
        private static string FormatList(List<string> subs)
        {
            var sb = new StringBuilder("```diff\n");

            int limit = Math.Min(subs.Count, 20);

            for (int i = 0; i < limit; i++)
            {
                sb.AppendLine($"+ {subs[i]}");
            }
            if (subs.Count > 20)
                sb.AppendLine($"... and {subs.Count - 20} more");

            sb.Append("```");
            return sb.ToString();
        }
    }
}
