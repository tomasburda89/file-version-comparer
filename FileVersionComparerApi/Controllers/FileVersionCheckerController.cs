using DTO;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Utils;

[ApiController]
[Route("api/[controller]/directories")]
public class FileVersionCheckerController : ControllerBase
{

    public FileVersionCheckerController()
    {
        _folderScanResulManager = new FolderScanResulManager(true);
    }

    private FolderScanResulManager _folderScanResulManager;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> GetAllDirectories()
    {
        object list = _folderScanResulManager.GetScannedFolders();
        return Ok(list);
    }

    [HttpGet("scan/files/latest")]
    public async Task<ActionResult<IEnumerable<FileDTO>>> GetFilesForScan([FromQuery] string path)
    {
        var fileCheck = FileVersionCheckerUtility.IsDirectoryValid(path);
        if (!fileCheck.IsValid)
        {
            var message = new ErrorMessageDTO { Message = fileCheck.Message };
            return BadRequest(message);
        }

        try {
            FolderScanResultDTO item = _folderScanResulManager.GetLatestScanResult(path);
            return Ok(item);
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorMessageDTO { Message = ex.Message });
        }
    }

    [HttpGet("scan/files/execute")]
    public async Task<ActionResult<IEnumerable<FileDTO>>> ExecuteScan([FromQuery] string path)
    {
        var fileCheck = FileVersionCheckerUtility.IsDirectoryValid(path);
        if (!fileCheck.IsValid)
        {
            var message = new ErrorMessageDTO { Message = fileCheck.Message };
            return BadRequest(message);
        }

        FolderScanResultDTO item = _folderScanResulManager.ScanFolder(path);
        return Ok(item);
    }
}

