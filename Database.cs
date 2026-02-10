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
    }
}
