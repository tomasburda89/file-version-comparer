using DTO;
using FluentAssertions;
using Xunit;

namespace DTOTests
{
    public class FileDTOTests
    {
        private FileDTO dto;

        public FileDTOTests()
        {
            dto = new FileDTO()
            {
                FileName = "dummy_name.txt"
            };
        }


        [Theory]
        [InlineData("document.pdf")]
        [InlineData("")]                                 // Corner case: Empty string
        [InlineData("   ")]                              // Corner case: Whitespace string
        [InlineData("a.txt")]                            // Boundary: Short filename
        [InlineData("very_long_file_name_with_special_chars_$%_#@!.json")]
        [InlineData(null)]                              // Corner case: Null assignment
        public void TestFileNameShouldAcceptAndGetValuesCorrectly(string inputName)
        {
            // Act
            dto.FileName = inputName;

            // Assert
            dto.FileName.Should().Be(inputName);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]                                  // Corner case: Zero version
        [InlineData(-1)]                                 // Corner case: Negative version
        [InlineData(int.MinValue)]                       // Extreme boundary
        [InlineData(int.MaxValue)]                       // Extreme boundary
        public void TestFileVersionAndFileIdShouldAcceptAndGetValuesCorrectly(int inputInt)
        {
            // Act
            dto.FileVersion = inputInt;

            // Assert
            dto.FileVersion.Should().Be(inputInt);
        }

        [Fact]
        public void TestLastUpdatedShouldAcceptAndGetValuesCorrectly()
        {
            // Arrange extreme boundaries and common states
            var now = DateTime.UtcNow;
            var minDate = DateTime.MinValue;
            var maxDate = DateTime.MaxValue;

            // Act & Assert for Now
            dto.LastUpdated = now;
            dto.LastUpdated.Should().Be(now);

            // Act & Assert for Min boundary
            dto.LastUpdated = minDate;
            dto.LastUpdated.Should().Be(minDate);

            // Act & Assert for Max boundary
            dto.LastUpdated = maxDate;
            dto.LastUpdated.Should().Be(maxDate);
        }


        [Fact]
        public void TestObjectStateShouldBeMutableRepeatedly()
        {
            // Arrange & Act - Step 1
            dto.FileName = "v1.txt";
            dto.FileVersion = 1;
            dto.LastUpdated = new DateTime(2020, 1, 1);

            // Assert Step 1
            dto.FileName.Should().Be("v1.txt");
            dto.FileVersion.Should().Be(1);
            dto.LastUpdated.Should().Be(new DateTime(2020, 1, 1));

            // Act - Step 2 (Overwrite existing values)
            dto.FileName = "v2.txt";
            dto.FileVersion = 2;
            dto.LastUpdated = new DateTime(2026, 5, 28);

            // Assert Step 2
            dto.FileName.Should().Be("v2.txt");
            dto.FileVersion.Should().Be(2);
            dto.LastUpdated.Should().Be(new DateTime(2026, 5, 28));
        }
    }
}
