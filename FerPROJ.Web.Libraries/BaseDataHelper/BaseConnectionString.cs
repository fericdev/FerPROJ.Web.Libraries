using System;
using System.Collections.Generic;
using System.Text;

namespace FerPROJ.Web.Libraries.BaseDataHelper {
    public static class BaseConnectionString {
        private static string BASE_ENTITY_CONNECTION_STRING { get; set; } =
            $"server=localhost;" +
            $"port=3309;" +
            $"database=peoplesbank;" +
            $"uid=adminserver;" +
            $"pwd=admin123!@#;" +
            $"SSLMode=None;";

        public static void Set(string connectionString) {
            BASE_ENTITY_CONNECTION_STRING = connectionString;
        }
        public static void Set(string server, int port, string database, string username, string password) {
            BASE_ENTITY_CONNECTION_STRING = 
                $"server={server};" +
                $"port={port};" +
                $"database={database};" +
                $"uid={username};" +
                $"pwd={password};" +
                $"SSLMode=None;"; 
        }
        public static string Get() => BASE_ENTITY_CONNECTION_STRING;
    }
}
