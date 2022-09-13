using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using Ensek.Data.Repository;
using Microsoft.Extensions.Logging;

namespace Ensek.Business.Parser;

public class MeterReadingCSVParser : IMeterReadingCSVParser
{
    private static readonly int MAX_READING = 99999;

    private readonly IAccountRepository _repository;

    private readonly ILogger<MeterReadingCSVParser> _logger;

    public MeterReadingCSVParser(IAccountRepository repository, ILogger<MeterReadingCSVParser> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<MeterReadingCSVParseResult> Parse(GetCSVStream getCSVStream)
    {
        var updatedAccounts = new HashSet<int>();
        var result = new MeterReadingCSVParseResult();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            MissingFieldFound = null,
            HeaderValidated = null,
            IgnoreBlankLines = true
        };

        using (var stream = getCSVStream())
        using (StreamReader reader = new StreamReader(stream))
        using (var csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<MeterReadingLineMap>();
            foreach (var meterReading in csv.GetRecords<MeterReadingLine>())
            {
                if (!IsLineValid(meterReading))
                {
                    result.Failed++;
                    continue;
                }

                var account = await _repository.GetAccountById(meterReading.AccountId.Value);

                if (account == null)
                {
                    result.Failed++;
                    continue;
                }

                if (updatedAccounts.Contains(meterReading.AccountId.Value))
                {
                    result.Failed++;
                    continue;
                }

                if (meterReading.Reading <= account.LatestMeterReading?.Reading)
                {
                    result.Failed++;
                    continue;
                }

                try
                {
                    var toInsert = new MeterReading()
                    {
                        AccountId = meterReading.AccountId.Value,
                        DateTime = meterReading.DateTime.Value,
                        Reading = meterReading.Reading.Value
                    };
                    await _repository.InsertMeterReading(toInsert);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save meter reading to database", meterReading);
                    result.Failed++;
                    continue;
                }

                updatedAccounts.Add(meterReading.AccountId.Value);
                result.Successful++;
            }
        }

        return result;
    }

    private static bool IsLineValid(MeterReadingLine meterReading)
    {
        if (meterReading.AccountId == null || meterReading.DateTime == null || meterReading.Reading == null)
        {
            return false;
        }

        return meterReading.Reading > 0 && meterReading.Reading < MAX_READING;
    }
}

class MeterReadingLine
{
    public int? AccountId { get; set; }

    public DateTime? DateTime { get; set; }

    public int? Reading { get; set; }
}

class NullIntConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (int.TryParse(text, out int value))
        {
            return value;
        }

        return null;
    }
}

class NullDateTimeConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (DateTime.TryParse(text, out DateTime value))
        {
            return value;
        }

        return null;
    }
}

class MeterReadingLineMap : ClassMap<MeterReadingLine>
{
    public MeterReadingLineMap()
    {
        Map(m => m.AccountId).Index(0).TypeConverter<NullIntConverter>(); ;
        Map(m => m.DateTime).Index(1).TypeConverter<NullDateTimeConverter>();
        Map(m => m.Reading).Index(2).TypeConverter<NullIntConverter>();
    }
}
