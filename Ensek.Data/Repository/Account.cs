namespace Ensek.Data.Repository;

public class Account
{
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public MeterReading LatestMeterReading { get; set; }
}
