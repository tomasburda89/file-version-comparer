using DTO;
using FluentAssertions;
using Xunit;

namespace DTOTests
{
    public class ErrorMessageDTOTests
    {
        private ErrorMessageDTO dto;

        public ErrorMessageDTOTests()
        {
            dto = new ErrorMessageDTO();
        }

        [Fact]
        public void TestConstructorShouldInitializeMessageToDefaultNull()
        {
            // Assert native .NET default state
            dto.Message.Should().BeNull("because string properties default to null in .NET");
        }

        [Theory]
        [InlineData("An error occurred.")]                  // Standard error message
        [InlineData("")]                                     // Corner case: Empty string
        [InlineData("   ")]                                  // Corner case: Whitespace string
        [InlineData("A")]                                    // Boundary case: Single character
        [InlineData(null)]                                   // Corner case: Null assignment
        [InlineData("Error code: 500\nStacktrace: Line 42")] // Multi-line text handling
        [InlineData("⚠️ Fatal System Exception!")]            // Unicode / Emoji character handling
        public void TestMessageShouldAcceptAndGetValuesCorrectly(string inputMessage)
        {
            // Act
            dto.Message = inputMessage;

            // Assert
            dto.Message.Should().Be(inputMessage);
        }

        [Fact]
        public void TestMessageShouldSupportExtremelyLargeString()
        {
            // Arrange - Create a 10,000 character string boundary test
            string hugeMessage = new string('X', 10000);

            // Act
            dto.Message = hugeMessage;

            // Assert
            dto.Message.Should().HaveLength(10000);
            dto.Message.Should().Be(hugeMessage);
        }

        [Fact]
        public void TestObjectStateShouldBeMutableRepeatedly()
        {
            // Arrange & Act - Step 1
            dto.Message = "First Error";
            dto.Message.Should().Be("First Error");

            // Act - Step 2 (Overwrite existing value)
            dto.Message = "Second Error";

            // Assert Step 2
            dto.Message.Should().Be("Second Error");
        }
    }
}
