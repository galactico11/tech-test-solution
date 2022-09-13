namespace Ensek.Data.Repository;

public interface IAccountRepository
{
    Task<Account> GetAccountById(int accountId);
    Task InsertMeterReading(MeterReading meterReading);
}
