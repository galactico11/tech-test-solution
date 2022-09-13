namespace Ensek.Data.Repository;

public class MeterReading
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public DateTime DateTime { get; set; }

    public int Reading { get; set; }
}
