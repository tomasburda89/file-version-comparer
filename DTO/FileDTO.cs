namespace DTO
{
    /// <summary>
    /// Class representing a file with its name, version, last updated date, and ID, used for data transfer between layers of the application.
    /// </summary>
    public class FileDTO
    {
        public required string FileName { get; set; }
        public int FileVersion { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? FileHashCode { get; set; }
    }
}
