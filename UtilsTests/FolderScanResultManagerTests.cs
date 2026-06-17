using FluentAssertions;
using Utils;

namespace UtilsTests
{
    public class FolderScanResultManagerTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly string _snapshotFile;

        public FolderScanResultManagerTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
            _snapshotFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            AppConfiguration.SnapshotDirectoryPath = _snapshotFile;
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
            if (File.Exists(_snapshotFile)) File.Delete(_snapshotFile);
        }

        [Fact]
        public void AddFolderShouldAddAndListFolder()
        {
            var manager = new FolderScanResultManager(false);
            manager.AddFolder(_tempDir);

            var folders = manager.GetScannedFolders();
            folders.Should().Contain(Path.GetFullPath(_tempDir));

            var latest = manager.GetLatestScanResult(Path.GetFullPath(_tempDir));
            latest.Should().NotBeNull();
        }

        [Fact]
        public void AddFolderShouldThrowInvalidOperationExceptionWhenAlreadyExists()
        {
            var manager = new FolderScanResultManager(false);
            manager.AddFolder(_tempDir);

            FluentActions.Invoking(() => manager.AddFolder(_tempDir))
                .Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetLatestScanResultShouldThrowWhenMissing()
        {
            var manager = new FolderScanResultManager(false);
            FluentActions.Invoking(() => manager.GetLatestScanResult(_tempDir))
                .Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void ScanFolderShouldProduceSnapshotFileAndDetectNewFiles()
        {
            // Arrange - create a file in the directory
            string filename = "file1.txt";
            string filePath = Path.Combine(_tempDir, filename);
            File.WriteAllText(filePath, "content");
            var manager = new FolderScanResultManager(false);
            AppConfiguration.SnapshotDirectoryPath = _snapshotFile;


            // Act - execute scan
            var result = manager.ScanFolder(_tempDir);

            // Assert - new file detected
            result.NewFiles.Should().ContainSingle(f => f.FileName == Path.GetRelativePath(Path.GetFullPath(_tempDir), filePath));

            // Snapshot file should be written
            File.Exists(_snapshotFile).Should().BeTrue();
            var json = File.ReadAllText(_snapshotFile);
            json.Should().Contain(filename);
        }
    }
}
