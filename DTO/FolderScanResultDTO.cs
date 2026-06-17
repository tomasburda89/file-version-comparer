using DTO;

namespace Utils
{
    /// <summary>
    /// Claas that holds the updates of a folder, including lists of updated, new, deleted, and unchanged files.
    /// This class is used to track changes in a directory by comparing the current state of the folder
    /// with a previous state represented by a list of FileDTO objects.
    /// </summary>
    public class FolderScanResultDTO
    {
        public List<FileDTO> UpdatedFiles { get; set; } = new List<FileDTO>();
        public List<FileDTO> NewFiles { get; set; } = new List<FileDTO>();
        public List<FileDTO> DeletedFiles { get; set; } = new List<FileDTO>();
        public List<FileDTO> UnchangedFiles { get; set; } = new List<FileDTO>();
        public List<FileDTO> UpdatedDirectories { get; set; } = new List<FileDTO>();
        public List<FileDTO> NewDirectories { get; set; } = new List<FileDTO>();
        public List<FileDTO> DeletedDirectories { get; set; } = new List<FileDTO>();
        public List<FileDTO> UnchangedDirectories { get; set; } = new List<FileDTO>();
    }
}