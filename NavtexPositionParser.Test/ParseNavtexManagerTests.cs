using NUnit.Framework;
using Moq;
using NavtexPositionParser.Managers;
using NavtexPositionParser.Commands;
using NavtexPositionParser.Dtos;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NavtexPositionParser.Tests
{
    [TestFixture]
    public class ParseNavtexManagerTests
    {
        private ILogger<ParseNavtexManager> _logger;
        private ParseNavtexManager _parseManager;

        [SetUp]
        public void Setup()
        {
            var loggerMock = new Mock<ILogger<ParseNavtexManager>>();
            _logger = loggerMock.Object;
            _parseManager = new ParseNavtexManager(_logger);
        }

        [Test]
        public async Task ProcessAsync_ValidFile_NoCoordinates_ReturnsParsedData()
        {
            // setup
            var fileContent = "ZCZC Valid Navtex Message NNNN";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(fileContent);
            writer.Flush();
            stream.Position = 0;

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            var parseCommand = new ParseNavtexCommand { file = fileMock.Object };

            // Act
            var result = await _parseManager.ProcessAsync(parseCommand);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.validMessage);
            Assert.IsNotNull(result.coordinates);
            Assert.AreEqual(result.coordinates.Count, 0);
            Assert.AreEqual("Valid Navtex Message", result.validMessage);
        }

        [Test]
        public async Task ProcessAsync_ValidFile_SomeCoordinates_ReturnsParsedData()
        {
            // setup
            var fileContent = "ZCZC Valid Navtex Message 61 3757 N - 006 4906 W Additional Test Message NNNN";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(fileContent);
            writer.Flush();
            stream.Position = 0;

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            var parseCommand = new ParseNavtexCommand { file = fileMock.Object };

            // Act
            var result = await _parseManager.ProcessAsync(parseCommand);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.validMessage);
            Assert.AreEqual("Valid Navtex Message 61 3757 N - 006 4906 W Additional Test Message", result.validMessage);
            Assert.IsNotNull(result.coordinates);
            Assert.AreEqual(result.coordinates.Count, 1);
            Assert.AreEqual(result.coordinates[0], "61 3757 N - 006 4906 W");
        }


        [Test]
        public void ProcessAsync_InvalidFile_ThrowsException()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.OpenReadStream()).Throws<FileNotFoundException>();
            var parseCommand = new ParseNavtexCommand { file = fileMock.Object };

            // Act & Assert
            Assert.ThrowsAsync<FileNotFoundException>(async () => await _parseManager.ProcessAsync(parseCommand));
        }
    }
}
