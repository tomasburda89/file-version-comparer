using DTO;
using FluentAssertions;
using Xunit;

namespace UtilsTests
{
    public class FolderUpdatesTests
    {
        private FolderScanResultDTO folderUpdates;

        public FolderUpdatesTests()
        {
            folderUpdates = new FolderScanResultDTO();
        }

        [Fact]
        public void TestConstructorShouldInitializeListsToEmptyCollections()
        {
            folderUpdates.UpdatedFiles.Should().NotBeNull().And.BeEmpty();
            folderUpdates.NewFiles.Should().NotBeNull().And.BeEmpty();
            folderUpdates.DeletedFiles.Should().NotBeNull().And.BeEmpty();
            folderUpdates.UnchangedFiles.Should().NotBeNull().And.BeEmpty();
            folderUpdates.UpdatedDirectories.Should().NotBeNull().And.BeEmpty();
            folderUpdates.NewDirectories.Should().NotBeNull().And.BeEmpty();
            folderUpdates.DeletedDirectories.Should().NotBeNull().And.BeEmpty();
            folderUpdates.UnchangedDirectories.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void TestPropertiesShouldAcceptAndGetPopulatedListsCorrectly()
        {
            // Arrange
            var mockFiles = new List<FileDTO>
        {
            new FileDTO { FileName = "test.txt", FileVersion = 1, LastUpdated = DateTime.UtcNow }
        };

            // Act
            folderUpdates.UpdatedFiles = mockFiles;
            folderUpdates.NewFiles = mockFiles;
            folderUpdates.DeletedFiles = mockFiles;
            folderUpdates.UnchangedFiles = mockFiles;

            // Assert
            folderUpdates.UpdatedFiles.Should().HaveCount(1).And.Contain(mockFiles[0]);
            folderUpdates.NewFiles.Should().HaveCount(1).And.Contain(mockFiles[0]);
            folderUpdates.DeletedFiles.Should().HaveCount(1).And.Contain(mockFiles[0]);
            folderUpdates.UnchangedFiles.Should().HaveCount(1).And.Contain(mockFiles[0]);
        }

        [Fact]
        public void TestPropertiesShouldAcceptNullValues_CornerCase()
        {
            // Act - Force assigning null to collections (simulating serialization issues)
            folderUpdates.UpdatedFiles = null;
            folderUpdates.NewFiles = null;
            folderUpdates.DeletedFiles = null;
            folderUpdates.UnchangedFiles = null;

            // Assert
            folderUpdates.UpdatedFiles.Should().BeNull();
            folderUpdates.NewFiles.Should().BeNull();
            folderUpdates.DeletedFiles.Should().BeNull();
            folderUpdates.UnchangedFiles.Should().BeNull();
        }

        [Fact]
        public void TestCollectionsShouldAllowItemMutationsDirectly()
        {
            // Arrange
            var targetFile = new FileDTO { FileName = "target.dat", FileVersion = 1, LastUpdated = DateTime.UtcNow };

            // Act - Add item straight to the auto-initialized property collection
            folderUpdates.NewFiles.Add(targetFile);

            // Assert
            folderUpdates.NewFiles.Should().ContainSingle()
                .Which.FileName.Should().Be("target.dat");

            // Act - Remove item
            folderUpdates.NewFiles.Remove(targetFile);

            // Assert
            folderUpdates.NewFiles.Should().BeEmpty();
        }

        [Fact]
        public void TestObjectStateShouldBeMutableRepeatedly()
        {
            // Arrange
            var listA = new List<FileDTO> { new FileDTO { FileName = "file1.txt", FileVersion = 1 } };
            var listB = new List<FileDTO> { new FileDTO { FileName = "file2.txt", FileVersion = 2 }, new FileDTO { FileName = "file3.txt", FileVersion = 3 } };

            // Act - Step 1
            folderUpdates.UpdatedFiles = listA;
            folderUpdates.UpdatedFiles.Should().HaveCount(1);

            // Act - Step 2 (Overwrite layout state)
            folderUpdates.UpdatedFiles = listB;

            // Assert Step 2
            folderUpdates.UpdatedFiles.Should().HaveCount(2).And.Contain(f => f.FileName == "file3.txt");
        }
    }
}
