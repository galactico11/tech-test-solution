using Xunit;
using Moq;
using Ensek.Business.Parser;
using Microsoft.Extensions.Logging;
using Shouldly;
using System.Threading.Tasks;
using Ensek.Data.Repository;
using System.IO;
using System.Text;
using System;

namespace Ensek.Tests;

public class MeterReadingCSVParserTests
{
    [Fact]
    public async Task UploadingWithEmptyAccount_ShouldReturnFailure()
    {
        // Assign
        var repository = new Mock<IAccountRepository>();
        var sut = new MeterReadingCSVParser(repository.Object, Mock.Of<ILogger<MeterReadingCSVParser>>());
        var data = @"
AccountId,MeterReadingDateTime,MeterReadValue,,
,4/22/19 9:24,1002,,
";

        // Act
        var result = await sut.Parse(() => new MemoryStream(new UTF8Encoding().GetBytes(data)));

        // Assert
        result.Successful.ShouldBeEquivalentTo(0);
        result.Failed.ShouldBeEquivalentTo(1);
    }

    [Fact]
    public async Task UploadingWithInvalidAccount_ShouldReturnFailure()
    {
        // Assign
        var repository = new Mock<IAccountRepository>();
        var sut = new MeterReadingCSVParser(repository.Object, Mock.Of<ILogger<MeterReadingCSVParser>>());
        var data = @"
AccountId,MeterReadingDateTime,MeterReadValue,,
DATA,4/22/19 9:24,1002,,
";

        // Act
        var result = await sut.Parse(() => new MemoryStream(new UTF8Encoding().GetBytes(data)));

        // Assert
        result.Successful.ShouldBeEquivalentTo(0);
        result.Failed.ShouldBeEquivalentTo(1);
    }

    [Fact]
    public async Task UploadingWithNonExistentAccount_ShouldReturnFailure()
    {
        // Assign
        var repository = new Mock<IAccountRepository>();
        var sut = new MeterReadingCSVParser(repository.Object, Mock.Of<ILogger<MeterReadingCSVParser>>());
        var data = @"
AccountId,MeterReadingDateTime,MeterReadValue,,
2344,4/22/19 9:24,1002,,
";

        // Act
        var result = await sut.Parse(() => new MemoryStream(new UTF8Encoding().GetBytes(data)));

        // Assert
        result.Successful.ShouldBeEquivalentTo(0);
        result.Failed.ShouldBeEquivalentTo(1);
    }

    [Fact]
    public async Task UploadingWithValidAccount_ShouldReturnSuccess()
    {
        // Assign
        var account = new Account() { Id = 2344 };
        var repository = new Mock<IAccountRepository>();
        repository.Setup(m => m.GetAccountById(2344)).Returns(Task.FromResult(account));
        var sut = new MeterReadingCSVParser(repository.Object, Mock.Of<ILogger<MeterReadingCSVParser>>());
        var data = @"
AccountId,MeterReadingDateTime,MeterReadValue,,
2344,4/22/19 9:24,1002,,
";

        // Act
        var result = await sut.Parse(() => new MemoryStream(new UTF8Encoding().GetBytes(data)));

        // Assert
        result.Successful.ShouldBeEquivalentTo(1);
        result.Failed.ShouldBeEquivalentTo(0);
        var expectedDate = DateTime.Parse("2019-04-22 09:24:00");
        repository
            .Verify(
                m => m.InsertMeterReading(
                    Moq.It.Is<MeterReading>(input => input.Reading == 1002 && input.AccountId == 2344 && input.DateTime == expectedDate)
                ),
                Moq.Times.Once()
            );
    }

    // I've used the sub AccountRepository here for simplicity/time, but wouldn't do this normally!
    [Fact]
    public async Task UploadingTestFile_ShouldCorrectSuccessAndFailures()
    {
        // Assign
        var sut = new MeterReadingCSVParser(new AccountRepository(), Mock.Of<ILogger<MeterReadingCSVParser>>());
        var data = @"
AccountId,MeterReadingDateTime,MeterReadValue,,
2344,4/22/19 9:24,1002,,
2233,4/22/19 12:25,323,,
8766,4/22/19 12:25,3440,,
2344,4/22/19 12:25,1002,,
2345,4/22/19 12:25,45522,,
2346,4/22/19 12:25,999999,,
2347,4/22/19 12:25,54,,
2348,4/22/19 12:25,123,,
2349,4/22/19 12:25,VOID,,
2350,4/22/19 12:25,5684,,
2351,4/22/19 12:25,57579,,
2352,4/22/19 12:25,455,,
2353,4/22/19 12:25,1212,,
2354,4/22/19 12:25,889,,
2355,5/6/19 9:24,1,,
2356,5/7/19 9:24,0,,
2344,5/8/19 9:24,0X765,,
6776,5/9/19 9:24,-6575,,
6776,5/10/19 9:24,23566,,
4534,5/11/19 9:24,,,
1234,5/12/19 9:24,9787,,
1235,5/13/19 9:24,,,
1236,4/10/19 19:34,8898,,
1237,5/15/19 9:24,3455,,
1238,5/16/19 9:24,0,,
1239,5/17/19 9:24,45345,,
1240,5/18/19 9:24,978,,
1241,4/11/19 9:24,436,X,
1242,5/20/19 9:24,124,,
1243,5/21/19 9:24,77,,
1244,5/25/19 9:24,3478,,
1245,5/25/19 14:26,676,,
1246,5/25/19 9:24,3455,,
1247,5/25/19 9:24,3,,
1248,5/26/19 9:24,3467,,
";

        // Act
        var result = await sut.Parse(() => new MemoryStream(new UTF8Encoding().GetBytes(data)));

        // Assert
        result.Successful.ShouldBeEquivalentTo(23);
        result.Failed.ShouldBeEquivalentTo(12);
    }

    // TODO - add individual tests for possible variations of data
}