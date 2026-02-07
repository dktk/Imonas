using Microsoft.Extensions.Configuration;

namespace System
{
    public static class ConfigurationExtensions
    {
        private static Dictionary<string, string> connectionStrings = new Dictionary<string, string>();

        public static string ConnectionString(this IConfiguration configuration, string connectionName = "DefaultConnection")
        {
            if (connectionStrings.ContainsKey(connectionName))
            {
                return connectionStrings[connectionName];
            }

            return configuration.GetConnectionString(connectionName);
        }

        public static string[] GetUploadedFiles(this IConfiguration configuration, string pspName)
        {
            var path = configuration["CsvUploadFolderPath"] + pspName;

            return Directory.GetFiles(path);
        }
    }
}
