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

        public async Task<(int,int)> ProcessCsv(Stream inputStream)
        {
            var recordObj = new List<MeterRecord>();
            int failedCount = 0;
            int successfulWrites = 0;

            using (var reader = new StreamReader(inputStream))
            {
                var index = 0;

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    index++;

                    // skips the header, also find a way too see if the header matches the data struct
                    if (index == 1 || line == null)
                    {
                        continue;
                    }

                    var values = line.Split(',');

                    var validationResult = await ValidateMeterReadingAsync(values, _ensekDbContext, _logger);

                    if (validationResult>0)
                    {
                        failedCount++;
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
                
                _logger.LogCritical(ex.Message);
                throw new Exception("Error saving data to the database.", ex);
            }

            return (successfulWrites, failedCount);
        }


        public async Task<int> ValidateMeterReadingAsync(string[] values, EnsekDbContext _ensek, ILogger _logger)
        {
            int numberOfErrors = 0; 
            // Checks to see if the accountid can be converted to integer
            if (!int.TryParse(values[0], out var accountId))
            {
                numberOfErrors++;
                _logger.LogError($"Invalid AccountId: {values[0]}");
                return numberOfErrors;
            }

            var meterReadValueString = values[2];
            // check to see if the meter value is valid
            if (meterReadValueString.Length != 5 || !meterReadValueString.All(char.IsDigit))
            {
                numberOfErrors++;
                _logger.LogError($"Invalid meter reading value: {meterReadValueString}.");
                return numberOfErrors;
            }

            var readingDateTime = DateTime.Parse(values[1]);
            // check for duplicates by the composite key
            if (await _ensek.MeterReadings.AnyAsync(m => m.AccountId == accountId && m.MeterReadingDateTime == readingDateTime))
            {
                numberOfErrors++;
                _logger.LogError($"Duplicate entry found for AccountId {accountId} and DateTime {readingDateTime}.");
                return numberOfErrors;
            }

            var existingRead = await _ensek.MeterReadings
                              .Where(m => m.AccountId == accountId)
                              .OrderByDescending(m => m.MeterReadingDateTime)
                              .FirstOrDefaultAsync();
            // looks for existing reading later in time than current one for the same account id
            if (existingRead != null && readingDateTime <= existingRead?.MeterReadingDateTime)
            {
                numberOfErrors++;
                _logger.LogError($"Existing entry {existingRead?.MeterReadingDateTime} is older than most recent entry for Accountid {accountId} and DateTime {readingDateTime}");
                return numberOfErrors;
            }

            return numberOfErrors;
        }


    }
}
