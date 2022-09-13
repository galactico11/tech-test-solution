namespace Ensek.Business.Parser;

public delegate Stream GetCSVStream();
public interface IMeterReadingCSVParser
{
    Task<MeterReadingCSVParseResult> Parse(GetCSVStream getCSVStream);
}
