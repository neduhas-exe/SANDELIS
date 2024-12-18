// WarehouseSystem/Infrastructure/Config/CsvConfig.cs
using System;
using System.IO;

namespace Infrastructure.Config
{
    public static class CsvConfig
    {
        private static readonly string BaseDirectory;

        static CsvConfig()
        {
            // Get the executing assembly's directory
            string executingDir = AppDomain.CurrentDomain.BaseDirectory;

            // Navigate up to the Presentation folder
            DirectoryInfo directory = new DirectoryInfo(executingDir);
            while (directory != null && !directory.Name.Equals("Presentation", StringComparison.OrdinalIgnoreCase))
            {
                directory = directory.Parent;
            }

            // Set base directory for data inside Presentation folder
            BaseDirectory = Path.Combine(directory?.FullName ?? executingDir, "Data");
            EnsureDirectoryExists();
        }

        private static void EnsureDirectoryExists()
        {
            if (!Directory.Exists(BaseDirectory))
            {
                Directory.CreateDirectory(BaseDirectory);
            }
        }

        public static class Paths
        {
            public static string Products => GetFilePath("products.csv");
            public static string Users => GetFilePath("users.csv");
            public static string Sites => GetFilePath("sites.csv");
            public static string Customers => GetFilePath("customers.csv");
        }

        private static string GetFilePath(string fileName)
        {
            string filePath = Path.Combine(BaseDirectory, fileName);
            EnsureFileExists(filePath);
            return filePath;
        }

        private static void EnsureFileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                // Create directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // Create empty file
                using (File.Create(filePath)) { }
            }
        }
    }
}