using DTO;
using FluentAssertions;
using Utils;

namespace UtilsTests
{
    public class FileVersionCheckerUtilityTests : IDisposable
    {
        private readonly string _testDirPath;

        public FileVersionCheckerUtilityTests()
        {
            // Setup a sandboxed temp directory on your system for execution
            _testDirPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirPath);
        }

        public void Dispose()
        {
            // Clean up the sandboxed directory after every single test run
            if (Directory.Exists(_testDirPath))
            {
                Directory.Delete(_testDirPath, true);
            }
        }

        #region IsDirectoryValid Tests

        [Fact]
        public void TestIsDirectoryValid_ShouldReturnTrueWhenDirectoryExists()
        {
            // Act
            var result = FileVersionCheckerUtility.IsDirectoryValid(_testDirPath);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Message.Should().BeNull();
        }

        [Theory]
        [InlineData(@"C:\NonExistentFolder_XYZ_123")]
        public void TestIsDirectoryValidShouldReturnFalseWhenDirectoryDoesNotExistOrIsWhitespace(string invalidPath)
        {
            // Act
            var result = FileVersionCheckerUtility.IsDirectoryValid(invalidPath);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Message.Should().Be("Directory do not exists!");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void TestIsDirectoryValidShouldThrowArgumentNullExceptionWhenPathIsNullOrIsWhitespace(string invalidPath)
        {
            // Act & Assert
            FluentActions.Invoking(() => FileVersionCheckerUtility.IsDirectoryValid(invalidPath))
                .Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region GetFolderUpdates Tests

        [Fact]
        public void TestGetFolderUpdatesShouldReturnEmptyUpdatesWhenDirectoryIsEmptyAndPreviousStateIsEmpty()
        {
            // Arrange
            var previousState = new List<FileDTO>();

            // Act
            var result = new FileVersionCheckerUtility(_testDirPath).GetFolderUpdates(previousState, new List<FileDTO>());

            // Assert
            result.NewFiles.Should().BeEmpty();
            result.UpdatedFiles.Should().BeEmpty();
            result.DeletedFiles.Should().BeEmpty();
            result.UnchangedFiles.Should().BeEmpty();
        }

        [Fact]
        public void TestGetFolderUpdatesShouldIdentifyNewFilesWhenFileIsPresentOnDiskButNotInPreviousState()
        {
            // Arrange
            string filePath = Path.Combine(_testDirPath, "newfile.txt");
            File.WriteAllText(filePath, "Hello World");
            var previousState = new List<FileDTO>();

            // Act
            var result = new FileVersionCheckerUtility(_testDirPath).GetFolderUpdates(previousState, new List<FileDTO>());

            // Assert
            result.NewFiles.Should().ContainSingle();
            result.NewFiles.First().FileName.Should().Be("newfile.txt");
            result.NewFiles.First().FileVersion.Should().Be(1);

            result.UpdatedFiles.Should().BeEmpty();
            result.DeletedFiles.Should().BeEmpty();
            result.UnchangedFiles.Should().BeEmpty();
        }

        [Fact]
        public void TestGetFolderUpdatesShouldIdentifyUnchangedFilesWhenFileTimesMatchOrPreviousIsNewer()
        {
            // Arrange
            string fileName = "unchanged.txt";
            string filePath = Path.Combine(_testDirPath, fileName);
            File.WriteAllText(filePath, "Content");

            // Sync times perfectly
            DateTime currentDiskTime = File.GetLastWriteTime(filePath);

            var previousState = new List<FileDTO>
            {
                new FileDTO { FileName = fileName, FileVersion = 2, LastUpdated = currentDiskTime }
            };

            // Act
            var result = new FileVersionCheckerUtility(_testDirPath).GetFolderUpdates(previousState, new List<FileDTO>());

            // Assert
            result.UnchangedFiles.Should().ContainSingle();
            result.UnchangedFiles.First().FileName.Should().Be(fileName);
            result.UnchangedFiles.First().FileVersion.Should().Be(2);

            result.NewFiles.Should().BeEmpty();
            result.UpdatedFiles.Should().BeEmpty();
            result.DeletedFiles.Should().BeEmpty();
        }

        [Fact]
        public void TestGetFolderUpdatesShouldIdentifyUpdatedFilesAndIncrementVersionWhenDiskFileIsNewer()
        {
            // Arrange
            string fileName = "updated.txt";
            string filePath = Path.Combine(_testDirPath, fileName);
            File.WriteAllText(filePath, "Old Content");

            // Establish an older timestamp for the previous DTO state
            DateTime pastTime = DateTime.UtcNow.AddDays(-5);
            var previousState = new List<FileDTO>
            {
                new FileDTO { FileName = fileName, FileVersion = 4, LastUpdated = pastTime }
            };

            // Force the disk write time to be ahead of the DTO state
            File.SetLastWriteTime(filePath, DateTime.UtcNow);

            // Act
            var result = new FileVersionCheckerUtility(_testDirPath).GetFolderUpdates(previousState, new List<FileDTO>());

            // Assert
            result.UpdatedFiles.Should().ContainSingle();
            var updatedFile = result.UpdatedFiles.First();
            updatedFile.FileName.Should().Be(fileName);
            updatedFile.FileVersion.Should().Be(5); // Verified 4 + 1 increment calculation

            result.NewFiles.Should().BeEmpty();
            result.DeletedFiles.Should().BeEmpty();
            result.UnchangedFiles.Should().BeEmpty();
        }

        [Fact]
        public void TestGetFolderUpdatesShouldIdentifyDeletedFilesWhenPresentInPreviousStateButMissingFromDisk()
        {
            // Arrange - Leave disk completely empty
            var previousState = new List<FileDTO>
            {
                new FileDTO { FileName = "missing.txt", FileVersion = 1, LastUpdated = DateTime.UtcNow }
            };

            // Act
            var result = new FileVersionCheckerUtility(_testDirPath).GetFolderUpdates(previousState, new List<FileDTO>());

            // Assert
            result.DeletedFiles.Should().ContainSingle();
            result.DeletedFiles.First().FileName.Should().Be("missing.txt");

            result.NewFiles.Should().BeEmpty();
            result.UpdatedFiles.Should().BeEmpty();
            result.UnchangedFiles.Should().BeEmpty();
        }

        [Fact]
        public void TestGetFolderUpdatesShouldHandleNullPreviousStateGracefully_CornerCase()
        {
            // Arrange
            string filePath = Path.Combine(_testDirPath, "sample.txt");
            File.WriteAllText(filePath, "Data");

            // Act
            var result = new FileVersionCheckerUtility(_testDirPath).GetFolderUpdates(null!, new List<FileDTO>());

            // Assert
            result.NewFiles.Should().ContainSingle().Which.FileName.Should().Be("sample.txt");
            result.UpdatedFiles.Should().BeEmpty();
            result.DeletedFiles.Should().BeEmpty();
            result.UnchangedFiles.Should().BeEmpty();
        }

        [Fact]
        public void TestGetFolderUpdatesShouldThrowDirectoryNotFoundExceptionWhenFolderPathDoesNotExist()
        {
            // Arrange
            string invalidPath = @"C:\ThisFolderDefinitelyDoesNotExist_999";
            var previousState = new List<FileDTO>();

            // Act & Assert
            FluentActions.Invoking(() => new FileVersionCheckerUtility(invalidPath).GetFolderUpdates(previousState, new List<FileDTO>()))
                .Should().Throw<DirectoryNotFoundException>();
        }

        #endregion
    }
}