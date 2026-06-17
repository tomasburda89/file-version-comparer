using System;
using System.Collections.Generic;
using System.Text;

namespace Utils
{
    public class AppConfiguration
    {
        private static AppConfiguration? _instance;

        private AppConfiguration()
        {
        }

        public static void SetInstance()
        {
            throw new NotImplementedException();
        }

        public static AppConfiguration GetInstance()
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("AppConfiguration instance has not been initialized. Call SetInstance() first.");
            }
            return _instance;
        }

        public static void SetInstance(string? enableRecursiveBrowsing, string? snapshotPath)
        {
            _instance = new AppConfiguration
            {
                BrowseFolderRecursively = bool.Parse(enableRecursiveBrowsing?.ToLower() ?? "true"),
                SnapshotDirectoryPath = snapshotPath ?? "Data/snapshot.json"
            };
        }

        public string SnapshotDirectoryPath { get; set; }

        public bool BrowseFolderRecursively { get; set; }
    }
}
