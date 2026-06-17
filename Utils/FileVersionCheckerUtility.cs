using DTO;
using System.Security.Cryptography;

namespace Utils
{
    /// <summary>
    /// Class including functionality which checks the validity of a directory
    /// and compares the current state of a folder with a previous state represented by a list of FileDTO objects
    /// to determine which files are new, updated, deleted, or unchanged.
    /// This utility is used to track changes in a directory and manage file versions effectively.
    /// </summary>
    public class FileVersionCheckerUtility
    {
        private string _folderPath;
        private bool _enableRecursive;

        /// <summary>
        /// Checks if the specified directory exists.
        /// </summary>
        /// <param name="pathToDiretory">The path to the directory to check.</param>
        /// <returns>A tuple containing a boolean indicating if the directory is valid and an optional message.</returns>
        public static (bool IsValid, string? Message) IsDirectoryValid(string pathToDiretory)
        {
            bool isValid = true;
            string? message = null;

            if (string.IsNullOrWhiteSpace(pathToDiretory)) {
                throw new ArgumentNullException(nameof(pathToDiretory), "Path to directory cannot be null or empty.");
            }

            if (!Directory.Exists(pathToDiretory))
            {
                isValid = false;
                message = "Directory do not exists!";
            }

            return (isValid, message);
        }

        public FileVersionCheckerUtility(string folderPath, bool? enableRecursive = null)
        {
            _folderPath = folderPath;
            _enableRecursive = enableRecursive ?? AppConfiguration.BrowseFolderRecursively;
        }

        /// <summary>
        /// Processes the specified folder and compares its current state with a previous state represented by a list of FileDTO objects.
        /// Determines which files are new, updated, deleted, or unchanged.
        /// </summary>
        /// <param name="folderPath">The path to the folder to check.</param>
        /// <param name="files">A collection of FileDTO objects representing the previous state of the folder.</param>
        /// <param name="directories">A collection of FileDTO objects representing the previous state of the directories.</param>
        /// <returns>A FolderUpdates object containing the lists of new, updated, deleted, and unchanged files.</returns>
        public FolderScanResultDTO GetFolderUpdates(IEnumerable<FileDTO> files, IEnumerable<FileDTO> directories)
        {
            FolderScanResultDTO updates = new FolderScanResultDTO();
            DirectoryInfo directoryInfo = new DirectoryInfo(_folderPath);
            var previouslyExistingFiles = files?.ToList() ?? new List<FileDTO>();
            var previouslyExistingDirectories = directories?.ToList() ?? new List<FileDTO>();
            var fileInfos = directoryInfo.EnumerateFiles("*", _enableRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            var directoryInfos = directoryInfo.EnumerateDirectories("*", _enableRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            ScanChanges(previouslyExistingFiles, updates.DeletedFiles, updates.NewFiles, updates.UpdatedFiles, updates.UnchangedFiles, 
                
                previouslyExistingFiles, fileInfos, true);

            ScanChanges(previouslyExistingDirectories, updates.DeletedDirectories, updates.NewDirectories, updates.UpdatedDirectories, updates.UnchangedDirectories,
                previouslyExistingDirectories, directoryInfos, false);

            return updates;
        }

        /// <summary>
        /// Scans the current state of the folder and compares it with the previous state to
        /// determine which files or directories are new, updated, deleted, or unchanged.
        /// </summary>
        /// <param name="files">A collection of FileDTO objects representing the previous state of the folder.</param>
        /// <param name="deletedFiles">A list to store the files that have been deleted.</param>
        /// <param name="newFiles">A list to store the new files.</param>
        /// <param name="updatedFiles">A list to store the updated files.</param>
        /// <param name="unchangedFiles">A list to store the unchanged files.</param>
        /// <param name="previouslyExistingFileNames">A list of file names that existed in the previous state.</param>
        /// <param name="itemsInfo">A collection of FileSystemInfo objects representing the current state of the folder.</param>
        /// <param name="calculateHash">A boolean indicating whether to calculate the hash of each file.</param>
        private void ScanChanges(IEnumerable<FileDTO> files, List<FileDTO> deletedFiles, 
            IList<FileDTO> newFiles,
            IList<FileDTO> updatedFiles,
            IList<FileDTO> unchangedFiles,
            List<FileDTO> previouslyExistingFileNames, IEnumerable<FileSystemInfo> itemsInfo, bool calculateHash)
        {
            foreach (var file in itemsInfo)
            {
                CreateActualFileAndAddToCorrectList(files,
                    newFiles, updatedFiles, unchangedFiles, previouslyExistingFileNames, file, calculateHash);
            }

            if (previouslyExistingFileNames.Count > 0)
            {
                deletedFiles.AddRange(previouslyExistingFileNames);
            }
        }

        /// <summary>
        /// Checks if the specified file exists in the previous state and compares its last updated time to determine if it is new, updated, or unchanged.
        /// </summary>
        /// <param name="files">A collection of FileDTO objects representing the previous state of the folder.</param>
        /// <param name="updates">A FolderUpdates object to store the results of the comparison.</param>
        /// <param name="previouslyExistingFiles">A set of FileDTO objects representing files that existed in the previous state.</param>
        /// <param name="fileInfo">The FileSystemInfo object representing the current file.</param>
        private void CreateActualFileAndAddToCorrectList(IEnumerable<FileDTO> files,
            IList<FileDTO> newFiles,
            IList<FileDTO> updatedFiles,
            IList<FileDTO> unchangedFiles,
            IList<FileDTO> previouslyExistingFiles, FileSystemInfo fileInfo, bool calculateHash)
        {
            string fileName = Path.GetRelativePath(_folderPath, fileInfo.FullName);
            DateTime lastUpdated = fileInfo.LastWriteTime;
            int fileVersion = 1;

            FileDTO actualFile = new FileDTO
            {
                FileName = fileName,
                FileVersion = fileVersion,
                LastUpdated = lastUpdated
            };

            if (previouslyExistingFiles.Any(f => f.FileName == fileName))
            {
                FileDTO previouslyExistingFile = previouslyExistingFiles.First(f => f.FileName == fileName);
                ProcessExistingFile(files, updatedFiles, unchangedFiles, previouslyExistingFile, fileInfo, fileName, actualFile, calculateHash);
                previouslyExistingFiles.Remove(previouslyExistingFile);
            }
            else
            {
                actualFile.FileHashCode = calculateHash ? GetSha256(fileInfo) : string.Empty;
                newFiles.Add(actualFile);
            }
        }

        /// <summary>
        /// Processes a file that exists in the previous state by comparing its last updated time with the 
        /// current file's last updated time to determine if it is updated or unchanged.
        /// </summary>
        /// <param name="files">A collection of FileDTO objects representing the previous state of the folder.</param>
        /// <param name="updates">A FolderUpdates object to store the results of the comparison.</param>
        /// <param name="previouslyExistingFileNames">A set of file names that existed in the previous state.</param>
        /// <param name="fileInfo">The FileSystemInfo object representing the current file.</param>
        /// <param name="fileName">The name of the current file.</param>
        /// <param name="actualFile">The FileDTO object representing the current file.</param>
        /// <param name="calculateHash">A value indicating whether to calculate the file hash.</param>
        private void ProcessExistingFile(IEnumerable<FileDTO> files,
            IList<FileDTO> updatedFiles, IList<FileDTO> unchangedFiles,
            FileDTO previouslyExistingFile,
            FileSystemInfo fileInfo, string fileName, FileDTO actualFile, bool calculateHash)
        {
            FileDTO fileDTO = previouslyExistingFile;

            actualFile.FileVersion = fileDTO.FileVersion;
            string hashCode = calculateHash ? GetSha256(fileInfo) : string.Empty;

            if (fileDTO.LastUpdated < fileInfo.LastWriteTime && fileDTO.FileHashCode != hashCode)
            {
                actualFile.FileVersion = fileDTO.FileVersion + 1;
                actualFile.FileHashCode = hashCode;
                updatedFiles.Add(actualFile);
            }
            else
            {
                unchangedFiles.Add(fileDTO);
            }
        }

        /// <summary>
        /// Calculates the SHA256 hash of the specified file to generate a unique hash code representing the file's content.
        /// </summary>
        /// <param name="fileInfo">The FileSystemInfo object representing the current file.</param>
        /// <returns>The SHA256 hash of the file.</returns>
        private static string GetSha256(FileSystemInfo fileInfo)
        {
            byte[] fileContent = File.ReadAllBytes(fileInfo.FullName);
            var sha256 = SHA256.Create();
            return Convert.ToHexString(sha256.ComputeHash(fileContent));
        }
    }
}
