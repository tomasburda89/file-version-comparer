
# File Version Comparer

This solution compares file states in directories and maintains versioning between scans. It exposes a Web API to manage stored folder snapshots, execute scans and retrieve comparison results.

Projects in the solution

- FileVersionComparerApi - ASP.NET Core Web API that provides REST endpoints for snapshot and scan operations.
- DTO - Data Transfer Objects (FileDTO, FolderScanResultDTO, ErrorMessageDTO).
- Utils - Utility library with configuration (AppConfiguration), file comparison logic (FileVersionCheckerUtility) and snapshot persistence (FolderScanResultManager).
- DTOTests - Unit tests for DTO objects.
- UtilsTests - Unit tests for utility classes.

Configuring and starting the project

1. Open the solution in Visual Studio 2022/2026 or use the .NET CLI.
2. Ensure .NET 10 SDK is installed.
3. Build the solution (Build Solution) or run `dotnet build` in the solution folder.
4. Launch the FileVersionComparerApi project (F5 in Visual Studio or `dotnet run` in the project directory).

Configuration

The application reads configuration via Utils.AppConfiguration. Make sure the SnapshotDirectoryPath is set to a writable directory and BrowseFolderRecursively is configured according to your needs.

Functionality overview

- Validate directory path: FileVersionCheckerUtility.IsDirectoryValid(path)
- Execute folder scan and persist snapshot: FolderScanResultManager.ScanFolder(path)
- Retrieve last stored scan result: FolderScanResultManager.GetLatestScanResult(path)
- List tracked folders: FolderScanResultManager.GetScannedFolders()

API endpoints

- GET /api/FileVersionChecker/directories
  - Returns a list of folder paths currently stored in the snapshot.

- GET /api/FileVersionChecker/scan/files/latest?path={path}
  - Returns the last stored scan result for the specified path. If the path is invalid, the API returns 400 BadRequest with an ErrorMessageDTO.

- GET /api/FileVersionChecker/scan/files/execute?path={path}
  - Executes a fresh scan for the given path, persists the snapshot and returns the scan result.

Running tests

Run tests in Test Explorer in Visual Studio or via CLI using `dotnet test` in the solution folder.

Notes and caveats

- FolderScanResultDTO contains lists for files and directories (new, updated, deleted, unchanged).
- FileDTO.FileName is declared as required. When creating FileDTO instances in code or tests, ensure FileName is assigned before use.

Web application (Blazor)

This repository includes a small Blazor app (FileVersionComparerApp) that can be used to trigger scans and view results.

How to use the web app

1. Start the API (FileVersionComparerApi) and make sure it runs on a reachable URL (e.g. https://localhost:7045/api/FileVersionChecker/directories).
2. Start the Blazor app (FileVersionComparerApp) from Visual Studio or `dotnet run` in the app folder.
3. Open the Blazor app in the browser. The main page contains a textbox to input a folder path, a dropdown with already scanned directories (fetched from the API), and a "Scan now" button.
4. Use the dropdown to select an existing folder to load its latest scan, or type a path and use "Scan now" to execute a scan.

The Blazor app calls the following API endpoints:
- GET api/FileVersionChecker/directories — retrieves tracked directories list
- GET api/FileVersionChecker/directories/scan/files/execute?path={path} — executes a scan and returns the result
- GET api/FileVersionChecker/directories/scan/files/latest?path={path} — retrieves the latest scan result for the specified path

