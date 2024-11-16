using Ensek.Model;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace Ensek.Functions
{
    public class CsvProcessing
    {
        private readonly EnsekDbContext _ensekDbContext;
        private readonly ILogger<CsvProcessing> _logger;
        public CsvProcessing(EnsekDbContext context,ILogger<CsvProcessing> logger)
        {
            _ensekDbContext = context;
            _logger= logger;
        }

        public async Task<(int, List<(int, string)>)> ProcessCsv(Stream inputStream)
        {
            List<MeterRecord> recordObj = new List<MeterRecord>();
            List<(int, string)> failedindex = new List<(int,string)>();
            int successfulWrites = 0;

            using (var reader = new StreamReader(inputStream))
            {
                var index = 1;
                await reader.ReadLineAsync();

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    
                    if (line == null)
                    {
                        continue;
                    }

                    var values =  line.Split(',');
                    index++;

                    var (numberOfErrors, errorMessage) = await ValidateMeterReadingAsync(values, _ensekDbContext);

                    if (numberOfErrors>0)
                    {
                        failedindex.Add((index, errorMessage));
                        continue; 
                    }

                    var record = new MeterRecord()
                    {
                        AccountId = int.Parse(values[0]),
                        MeterReadingDateTime = DateTime.Parse(values[1]),
                        MeterReadValue = int.Parse(values[2])
                    };

                    recordObj.Add(record);
                }
            }

            try
            {
                if (recordObj.Any())
                {

                    await _ensekDbContext.MeterReadings.AddRangeAsync(recordObj);
                    await _ensekDbContext.SaveChangesAsync();
                    successfulWrites = recordObj.Count;
                }
            }
            catch (Exception ex)
            {
                
                _logger.LogCritical(ex.ToString());
                throw new Exception("Error saving data to the database.", ex);
            }

            return (successfulWrites, failedindex);
        }


        public async Task<(int numberOfErrors, string errorMessage)> ValidateMeterReadingAsync(string[] values, EnsekDbContext _ensek)
        {
            int numberOfErrors = 0;
            string error = string.Empty;
            // Checks to see if the accountid can be converted to integer
            if (!int.TryParse(values[0], out var accountId))
            {
                numberOfErrors++;
                error = ($"Invalid AccountId: {values[0]}");
                return (numberOfErrors,error);
            }

            var meterReadValueString = values[2];
            // check to see if the meter value is valid
            if (meterReadValueString.Length != 5 || !meterReadValueString.All(char.IsDigit))
            {
                numberOfErrors++;
                error=($"Invalid meter reading value: {meterReadValueString}.");
                return (numberOfErrors,error);
            }

            var readingDateTime = DateTime.Parse(values[1]);
            // check for duplicates by the composite key
            if (await _ensek.MeterReadings.AnyAsync(m => m.AccountId == accountId && m.MeterReadingDateTime == readingDateTime))
            {
                numberOfErrors++;
                error = ($"Duplicate entry found for AccountId {accountId} and DateTime {readingDateTime}.");
                return (numberOfErrors,error);
            }

            var existingRead = await _ensek.MeterReadings
                              .Where(m => m.AccountId == accountId)
                              .OrderByDescending(m => m.MeterReadingDateTime)
                              .FirstOrDefaultAsync();
            // looks for existing reading later in time than current one for the same account id
            if (existingRead != null && readingDateTime <= existingRead?.MeterReadingDateTime)
            {
                numberOfErrors++;
                error=($"Existing entry {existingRead?.MeterReadingDateTime} is older than most recent entry for Accountid {accountId} and DateTime {readingDateTime}");
                return (numberOfErrors, error);
            }

            return (numberOfErrors, error);
        }


    }
}
