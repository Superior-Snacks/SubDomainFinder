using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using static System.Net.WebRequestMethods;

namespace subDomainFinder
{
    public static class Database
    {
        private static string DbFile = "recon.db";
        private static string ConnectionString = $"Data Source={DbFile};Version=3;";

        public static void Initialize()
        {
            if (!File.Exists(DbFile))
            {
                SQLiteConnection.CreateFile(DbFile);
            }

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                // We create a table with a UNIQUE constraint on the Subdomain column
                // This prevents duplicates automatically!
                string sql = @"
                CREATE TABLE IF NOT EXISTS Subdomains (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    RootDomain TEXT NOT NULL,
                    Subdomain TEXT NOT NULL UNIQUE,
                    FirstSeen DATE DEFAULT CURRENT_TIMESTAMP
                );";

                connection.Execute(sql);
            }
        }

        public static HashSet<string> GetExistingSubdomains(string rootDomain)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                // Dapper maps the result straight to a string
                var results = connection.Query<string>(
                    "SELECT Subdomain FROM Subdomains WHERE RootDomain = @RootDomain",
                    new { RootDomain = rootDomain }
                );

                return new HashSet<string>(results);
            }
        }

        public static void InsertSubdomains(string rootDomain, IEnumerable<string> newSubs)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    // We use 'INSERT OR IGNORE' so if the subdomain exists, SQL does nothing.
                    string sql = "INSERT OR IGNORE INTO Subdomains (RootDomain, Subdomain) VALUES (@RootDomain, @Subdomain)";

                    foreach (var sub in newSubs)
                    {
                        connection.Execute(sql, new { RootDomain = rootDomain, Subdomain = sub });
                    }

                    transaction.Commit();
                }
            }
        }
    }
}
