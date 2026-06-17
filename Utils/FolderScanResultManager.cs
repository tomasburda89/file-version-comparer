using DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utils
{
    public class FolderScanResulManager
    {
        [JsonPropertyName("FolderSnapshot")]
        public Dictionary<string, FolderScanResultDTO> FolderSnapshot { set; get;}

        public FolderScanResulManager() {
        }

        public FolderScanResulManager(bool loadFromJson)
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
        /// <returns></returns>
        public List<string> GetScannedFolders()
        {
            return FolderSnapshot.Keys.ToList();
        }

        /// <summary>
        /// Adds a new folder to the folder snapshot with an empty scan result.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddFolder(string folderPath)
        {
            if (FolderSnapshot.ContainsKey(folderPath)) {
                throw new InvalidOperationException($"Folder '{folderPath}' already exists in the snapshot.");
            }

            FolderSnapshot[folderPath] = new FolderScanResultDTO();
            SaveAsJson();
        }

        /// <summary>
        /// Retrieves the latest scan result for the specified folder path from the folder snapshot.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns>Latest scan result for the specified folder path</returns>
        /// <exception cref="KeyNotFoundException"></exception>
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
        /// If the folder path already exists in the snapshot, it retrieves the existing list of files; otherwise, it initializes an empty list.
        /// It then calls the FileVersionCheckerUtility to get the updated scan result and updates the FolderSnapshot dictionary accordingly.
        /// Finally, it saves the updated snapshot to a JSON file.
        /// </summary>
        /// <param name="folderPath">The path of the folder to update.</param>
        /// <returns>Latest scan result for the specified folder path</returns>
        public FolderScanResultDTO ScanFolder(string folderPath)
        {
            var allFiles = new List<FileDTO>();
            var allDirectories = new List<FileDTO>();
            string absoluteFolderPath = Path.GetFullPath(folderPath);

            if (FolderSnapshot.ContainsKey(absoluteFolderPath)) { 
                var snap = FolderSnapshot[absoluteFolderPath];
                allFiles = [.. snap.NewFiles, .. snap.UpdatedFiles, .. snap.UnchangedFiles];
                allDirectories = [.. snap.NewDirectories, .. snap.UpdatedDirectories, .. snap.UnchangedDirectories];
            }

            var newScanResult = new FileVersionCheckerUtility(folderPath)
                .GetFolderUpdates(allFiles, allDirectories);
            FolderSnapshot[absoluteFolderPath] = newScanResult;
            SaveAsJson();
            return newScanResult;
        }


        /// <summary>
        /// Saves the current state of the folder snapshot to a JSON file named "snapshot.json" in a human-readable format with indentation.
        /// </summary>
        private void SaveAsJson()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(this, options);
            File.WriteAllText(GetSnapshotFilePath(), jsonString);
        }

        /// Loads the folder snapshot from a JSON file named "snapshot.json" if it exists.
        private void LoadFromJson()
        {

            if (File.Exists(GetSnapshotFilePath()))
            {
                string jsonString = File.ReadAllText(GetSnapshotFilePath());
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var loadedSnapshot = JsonSerializer.Deserialize<FolderScanResulManager>(jsonString, options);
                if (loadedSnapshot != null)
                {
                    FolderSnapshot = loadedSnapshot.FolderSnapshot;
                }
            }
        }

        /// <summary>
        /// Retrieves the file path for the snapshot JSON file based on the application's configuration.
        /// </summary>
        /// <returns></returns>
        private static string GetSnapshotFilePath()
        {
            return AppConfiguration.GetInstance().SnapshotDirectoryPath;
        }

    }
}
