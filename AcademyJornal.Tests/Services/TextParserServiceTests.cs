using AcademyJournal.Core.Services;
using Xunit;

namespace AcademyJornal.Tests.Services
{
    public class TextParserServiceTests
    {
        [Fact]
        public void Parse_ValidInput_ReturnsCorrectCount()
        {
            // Arrange
            string input = "Иван Иванов 10 9 8\nПетр Петров 7 8 9";

            // Act
            var result = TextParserService.Parse(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Иван Иванов", result[0].fullName);
            Assert.Equal(3, result[0].grades.Count);
        }

        [Fact]
        public void Parse_GradeGreaterThan12_ThrowsException()
        {
            // Arrange
            string input = "Иван Иванов 13 8";

            // Act & Assert
            Assert.Throws<Exception>(() => TextParserService.Parse(input));
        }

        [Fact]
        public void Parse_GradeZeroOrNegative_ThrowsException()
        {
            // Arrange
            string input = "Иван Иванов 0 5";

            // Act & Assert
            Assert.Throws<Exception>(() => TextParserService.Parse(input));
        }

        [Fact]
        public void Parse_EmptyInput_ReturnsEmptyList()
        {
            // Arrange
            string input = "";

            // Act
            var result = TextParserService.Parse(input);

            // Assert
            Assert.Empty(result);
        }
    }
}