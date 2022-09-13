using Ensek.Web.Controllers;
using Xunit;
using Moq;
using Ensek.Business.Parser;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Shouldly;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ensek.Tests;

public class MeterReadingControllerTests
{
    [Fact]
    public async Task UploadingNonCSV_ShouldReturnBadRequest()
    {
        // Assign
        var sut = new MeterReadingController(Mock.Of<IMeterReadingCSVParser>(), Mock.Of<ILogger<MeterReadingController>>());
        var file = new Mock<IFormFile>();
        file.Setup(m => m.ContentType).Returns("text/html");

        // Act
        var result = await sut.Post(file.Object);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadingCSV_ShouldReturnUploadResult()
    {
        // Assign
        var parser = new Mock<IMeterReadingCSVParser>();
        var parseResult = new MeterReadingCSVParseResult() { Successful = 10, Failed = 20 };
        parser.Setup(m => m.Parse(Moq.It.IsAny<GetCSVStream>())).Returns(Task.FromResult(parseResult));
        var sut = new MeterReadingController(parser.Object, Mock.Of<ILogger<MeterReadingController>>());
        var file = new Mock<IFormFile>();
        file.Setup(m => m.ContentType).Returns("text/csv");
        file.Setup(m => m.OpenReadStream()).Returns(new System.IO.MemoryStream());

        // Act
        var result = await sut.Post(file.Object);

        // Assert
        ((MeterReadingUploadResult)result.Value)?.Successful.ShouldBeEquivalentTo(10);
        ((MeterReadingUploadResult)result.Value).Failed.ShouldBeEquivalentTo(20);
    }
}