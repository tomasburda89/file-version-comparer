namespace DTO
{
    /// <summary>
    /// Class representing an error message, used for data transfer between layers of the application when an error occurs.
    /// </summary>
    public class ErrorMessageDTO
    {
        public required string Message { get; set; }
    }
}
