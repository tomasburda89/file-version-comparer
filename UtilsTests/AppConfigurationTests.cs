using FluentAssertions;
using Utils;

namespace UtilsTests
{
    public class AppConfigurationTests
    {
        [Fact]
        public void SetAndGetProperties_ShouldWorkCorrectly()
        {
            AppConfiguration.SnapshotDirectoryPath = "my/snapshot.json";
            AppConfiguration.BrowseFolderRecursively = true;

            AppConfiguration.SnapshotDirectoryPath.Should().Be("my/snapshot.json");
            AppConfiguration.BrowseFolderRecursively.Should().BeTrue();

            AppConfiguration.BrowseFolderRecursively = false;
            AppConfiguration.BrowseFolderRecursively.Should().BeFalse();
        }
    }
}
