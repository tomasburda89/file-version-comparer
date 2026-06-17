namespace DTO
{
    /// <summary>
    /// Class that holds the updates of a folder, including lists of updated, new, deleted, and unchanged files.
    /// This DTO is used to track changes in a directory by comparing the current state of the folder
    /// with a previous state represented by a list of FileDTO objects.
    /// </summary>
    public class FolderScanResultDTO
    {
        /// <summary>
        /// Files that were updated since the last scan.
        /// </summary>
        public List<FileDTO> UpdatedFiles { get; set; } = new List<FileDTO>();

        /// <summary>
        /// Files that are new since the last scan.
        /// </summary>
        public List<FileDTO> NewFiles { get; set; } = new List<FileDTO>();

        /// <summary>
        /// Files that existed previously but are now deleted.
        /// </summary>
        public List<FileDTO> DeletedFiles { get; set; } = new List<FileDTO>();

        /// <summary>
        /// Files that remained unchanged since the last scan.
        /// </summary>
        public List<FileDTO> UnchangedFiles { get; set; } = new List<FileDTO>();

        /// <summary>
        /// Directories that were updated since the last scan.
        /// </summary>
        public List<FileDTO> UpdatedDirectories { get; set; } = new List<FileDTO>();

        /// <summary>
        /// Directories that are new since the last scan.
        /// </summary>
        public List<FileDTO> NewDirectories { get; set; } = new List<FileDTO>();

        /// <summary>
        /// Directories that existed previously but are now deleted.
        /// </summary>
        public List<FileDTO> DeletedDirectories { get; set; } = new List<FileDTO>();

        /// <summary>
        /// Directories that remained unchanged since the last scan.
        /// </summary>
        public List<FileDTO> UnchangedDirectories { get; set; } = new List<FileDTO>();
    }
}
