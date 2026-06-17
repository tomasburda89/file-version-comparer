namespace Utils
{
    // This class holds application-wide configuration settings.
    // It provides static properties that can be accessed and modified throughout the application.
    public static class AppConfiguration
    {
        public static string? SnapshotDirectoryPath { get; set; }

        public static bool BrowseFolderRecursively { get; set; }
    }
}
