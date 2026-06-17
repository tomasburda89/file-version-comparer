using DTO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utils
{
    /// <summary>
    /// Manager class used to store and persist scan results for multiple folders.
    /// It provides methods to read and update the snapshot and to persist it as JSON.
    /// </summary>
    public class FolderScanResultManager
    {
        [JsonPropertyName("FolderSnapshot")]
        public Dictionary<string, FolderScanResultDTO> FolderSnapshot { get; set; } = new Dictionary<string, FolderScanResultDTO>();

        public FolderScanResultManager() { }

        public FolderScanResultManager(bool loadFromJson)
        {
            FolderSnapshot = new Dictionary<string, FolderScanResultDTO>();
            if (loadFromJson)
            {
                LoadFromJson();
            }
        }

        /// <summary>
        /// Retrieves a list of folder paths currently stored in the folder snapshot.
        /// </summary>
        /// <returns>List of folder paths</returns>
        public List<string> GetScannedFolders()
        {
            return FolderSnapshot.Keys.ToList();
        }

        /// <summary>
        /// Adds a new folder to the folder snapshot with an empty scan result.
        /// </summary>
        /// <param name="folderPath">Absolute folder path to add</param>
        /// <exception cref="InvalidOperationException">Thrown when the folder already exists</exception>
        public void AddFolder(string folderPath)
        {
            if (FolderSnapshot.ContainsKey(folderPath))
            {
                throw new InvalidOperationException($"Folder '{folderPath}' already exists in the snapshot.");
            }

            FolderSnapshot[folderPath] = new FolderScanResultDTO();
            SaveAsJson();
        }

        /// <summary>
        /// Retrieves the latest scan result for the specified folder path from the folder snapshot.
        /// </summary>
        /// <param name="folderPath">Absolute folder path</param>
        /// <returns>Latest scan result for the specified folder path</returns>
        /// <exception cref="KeyNotFoundException">If the folder is not tracked</exception>
        public FolderScanResultDTO GetLatestScanResult(string folderPath)
        {
            if (!FolderSnapshot.ContainsKey(folderPath))
            {
                throw new KeyNotFoundException($"Folder '{folderPath}' does not exist in the snapshot.");
            }

            var scanResult = FolderSnapshot[folderPath];
            return scanResult;
        }

        /// <summary>
        /// Updates the folder snapshot for the specified folder path by checking for any changes in the files within that folder.
        /// </summary>
        /// <param name="folderPath">The path of the folder to update.</param>
        /// <returns>Latest scan result for the specified folder path</returns>
        public FolderScanResultDTO ScanFolder(string folderPath)
        {
            var allFiles = new List<FileDTO>();
            var allDirectories = new List<FileDTO>();
            string absoluteFolderPath = Path.GetFullPath(folderPath);

            if (FolderSnapshot.ContainsKey(absoluteFolderPath))
            {
                var snap = FolderSnapshot[absoluteFolderPath];
                allFiles = [.. snap.NewFiles, .. snap.UpdatedFiles, .. snap.UnchangedFiles];
                allDirectories = [.. snap.NewDirectories, .. snap.UpdatedDirectories, .. snap.UnchangedDirectories];
            }

            var newScanResult = new FileVersionCheckerUtility(folderPath,false)
                .GetFolderUpdates(allFiles, allDirectories);
            FolderSnapshot[absoluteFolderPath] = newScanResult;
            SaveAsJson();
            return newScanResult;
        }

        /// <summary>
        /// Saves the current state of the folder snapshot to a JSON file.
        /// </summary>
        private void SaveAsJson()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(this, options);
            File.WriteAllText(GetSnapshotFilePath(), jsonString);
        }

        /// <summary>
        /// Loads the folder snapshot from a JSON file if it exists.
        /// </summary>
        private void LoadFromJson()
        {
            if (File.Exists(GetSnapshotFilePath()))
            {
                string jsonString = File.ReadAllText(GetSnapshotFilePath());
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var loadedSnapshot = JsonSerializer.Deserialize<FolderScanResultManager>(jsonString, options);
                if (loadedSnapshot != null)
                {
                    FolderSnapshot = loadedSnapshot.FolderSnapshot;
                }
            }
        }

        /// <summary>
        /// Retrieves the file path for the snapshot JSON file based on the application's configuration.
        /// </summary>
        /// <returns>Absolute path to the snapshot file</returns>
        private static string GetSnapshotFilePath()
        {
            return AppConfiguration.SnapshotDirectoryPath;
        }

    }
}
