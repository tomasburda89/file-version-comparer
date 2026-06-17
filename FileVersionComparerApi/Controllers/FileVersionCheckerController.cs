using DTO;
using Microsoft.AspNetCore.Mvc;
using Utils;

/// <summary>
/// API controller providing endpoints to list tracked directories and to get/execute folder scans.
/// </summary>
[ApiController]
[Route("api/[controller]/directories")]
public class FileVersionCheckerController : ControllerBase
{

    /// <summary>
    /// Creates a new FileVersionCheckerController and initializes the folder snapshot manager.
    /// </summary>
    public FileVersionCheckerController()
    {
        _folderScanResultManager = new FolderScanResultManager(true);
    }

    private FolderScanResultManager _folderScanResultManager;

    /// <summary>
    /// Returns all folder paths that are currently tracked in the snapshot store.
    /// </summary>
    /// <returns>List of tracked folder paths.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> GetAllDirectories()
    {
        object list = _folderScanResultManager.GetScannedFolders();
        return Ok(list);
    }

    /// <summary>
    /// Returns the last stored scan result for the specified folder path.
    /// </summary>
    /// <param name="path">Absolute or relative folder path to query.</param>
    /// <returns>Folder scan result or BadRequest if path is invalid or folder not tracked.</returns>
    [HttpGet("scan/files/latest")]
    public async Task<ActionResult<FolderScanResultDTO>> GetFilesForScan([FromQuery] string path)
    {
        var fileCheck = FileVersionCheckerUtility.IsDirectoryValid(path);
        if (!fileCheck.IsValid)
        {
            var message = new ErrorMessageDTO { Message = fileCheck.Message };
            return BadRequest(message);
        }

        try {
            FolderScanResultDTO item = _folderScanResultManager.GetLatestScanResult(path);
            return Ok(item);
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorMessageDTO { Message = ex.Message });
        }
    }

    /// <summary>
    /// Executes a fresh scan for the specified folder path, persists the result and returns it.
    /// </summary>
    /// <param name="path">Absolute or relative folder path to scan.</param>
    /// <returns>Newly produced FolderScanResultDTO or BadRequest if path is invalid.</returns>
    [HttpGet("scan/files/execute")]
    public async Task<ActionResult<FolderScanResultDTO>> ExecuteScan([FromQuery] string path)
    {
        var fileCheck = FileVersionCheckerUtility.IsDirectoryValid(path);
        if (!fileCheck.IsValid)
        {
            var message = new ErrorMessageDTO { Message = fileCheck.Message };
            return BadRequest(message);
        }

        FolderScanResultDTO item = _folderScanResultManager.ScanFolder(path);
        return Ok(item);
    }
}

