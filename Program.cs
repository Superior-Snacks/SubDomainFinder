using DnsClient;
using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace subDomainFinder
{
class Program
    {
        static async Task Main()
        {
            // Example Range: A subset of the Siminn range you mentioned
            string baseIp = "89.104.145.";
            var tasks = new List<Task>();

            Console.WriteLine("Scanning IPs for SSL Certificates...\n");

            for (int i = 150; i <= 155; i++)
            {
                string ip = baseIp + i;
                tasks.Add(GetCertificateInfoAsync(ip));
            }

            await Task.WhenAll(tasks);
            Console.WriteLine("\nScan Complete.");
        }

        static async Task GetCertificateInfoAsync(string ip)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    // 1. Connect to the IP on Port 443 with a short timeout
                    var connectTask = client.ConnectAsync(ip, 443);
                    if (await Task.WhenAny(connectTask, Task.Delay(2000)) != connectTask)
                    {
                        // Connection timed out
                        return;
                    }

                    // 2. Wrap the stream in SSL
                    // The callback (sender, cert, chain, errors) => true  
                    // tells C# to IGNORE certificate errors (like name mismatch)
                    using (var sslStream = new SslStream(client.GetStream(), false, (s, c, ch, e) => true))
                    {
                        // 3. Handshake (We pass the IP as the target host)
                        await sslStream.AuthenticateAsClientAsync(ip);

                        // 4. Extract the Certificate
                        if (sslStream.RemoteCertificate is X509Certificate2 cert)
                        {
                            // Get the Common Name (CN)
                            string subject = cert.Subject; // Looks like "CN=example.com, O=Company..."
                            string commonName = ParseCommonName(subject);

                            // Get Alternative Names (SANs) - extremely useful!
                            // This often lists ALL domains on this specific certificate
                            var sans = GetSan(cert);

                            Console.WriteLine($"[SUCCESS] {ip}");
                            Console.WriteLine($"    -> Common Name: {commonName}");
                            Console.WriteLine($"    -> Issuer:      {cert.Issuer}");
                            if (!string.IsNullOrEmpty(sans))
                                Console.WriteLine($"    -> ALT Names:   {sans}");
                            Console.WriteLine("------------------------------------------------");
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If the port is closed or not SSL, we just ignore it
            }
        }

        // Helper to extract just the CN= part from the subject string
        static string ParseCommonName(string subject)
        {
            if (string.IsNullOrEmpty(subject)) return "Unknown";

            foreach (var part in subject.Split(','))
            {
                var trimmed = part.Trim();
                if (trimmed.StartsWith("CN="))
                {
                    return trimmed.Substring(3);
                }
            }
            return subject;
        }

        // Helper to get Subject Alternative Names (DNS Names)
        static string GetSan(X509Certificate2 cert)
        {
            foreach (var extension in cert.Extensions)
            {
                // OID 2.5.29.17 is the ID for Subject Alternative Name
                if (extension.Oid.Value == "2.5.29.17")
                {
                    // The data is ASN.1 encoded, extracting it purely as string is messy
                    // but .Format(true) usually gives a readable list
                    return extension.Format(true);
                }
            }
            return "";
        }
    }
}
}