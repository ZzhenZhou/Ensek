using Ensek.Model;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace Ensek.Functions
{
    public class CsvProcessing
    {
        private readonly EnsekDbContext _ensekDbContext;
        private readonly helperFunctions _helperFunctions;
        public CsvProcessing(EnsekDbContext context,
                             helperFunctions helperFunctions)
        {
            _ensekDbContext = context;
            _helperFunctions = helperFunctions;
        }

        public async Task<(int, List<(int, string)>)> ProcessCsv(Stream inputStream)
        {
            List<MeterRecord> recordObj = new List<MeterRecord>();
            List<(int, string)> failedIndex = new List<(int, string)>();
            int successfulWrites = 0;
            const int batchSize = 10;

            List<int> customerAccountIDs = await _ensekDbContext
                                        .accountrecords
                                        .Select(a => a.AccountId)
                                        .ToListAsync();

            using (StreamReader reader = new StreamReader(inputStream))
            {
                int index = 1;
                await reader.ReadLineAsync(); // Don't read the header as its already been checked to be valid.

                while (!reader.EndOfStream)
                {
                    string? line = await reader.ReadLineAsync();

                    if (line == null) continue;

                    string[] values = line.Split(',');
                    index++;

                    (int numberOfErrors, string errorMessage) = await ValidateMeterReadingAsync(values, customerAccountIDs,  _ensekDbContext); //possible issues with customer being created after readings have been put in

                    if (numberOfErrors > 0)
                    {
                        failedIndex.Add((index, errorMessage));
                        continue;
                    }

                    MeterRecord record = new MeterRecord()
                    {
                        AccountId = int.Parse(values[0]),
                        MeterReadingDateTime = DateTime.Parse(values[1]),
                        MeterReadValue = int.Parse(values[2])
                    };

                    recordObj.Add(record);

                    
                    if (recordObj.Count >= batchSize)
                    {
                        successfulWrites += await _helperFunctions.SaveBatchAsync(recordObj);
                        recordObj.Clear();
                    }
                }
            }

            // if less than batchsize, don't batch.
            if (recordObj.Any())
            {
                successfulWrites += await _helperFunctions.SaveBatchAsync(recordObj);
            }

            return (successfulWrites, failedIndex);
        }

        public async Task<(int numberOfErrors, string errorMessage)> ValidateMeterReadingAsync(string[] values, List<int> accounts, EnsekDbContext _ensek)
        {
            int numberOfErrors = 0;
            string error = string.Empty;
            // Checks to see if the accountid can be converted to integer
            if (!int.TryParse(values[0], out int accountId) || !accounts.Contains(accountId))
            {
                numberOfErrors++;
                error = ($"Invalid AccountId: {values[0]}");
                return (numberOfErrors,error);
            }

            string meterReadValueString = values[2];
            // check to see if the meter value is valid
            if (meterReadValueString.Length != 5 || !meterReadValueString.All(char.IsDigit))
            {
                numberOfErrors++;
                error=($"Invalid meter reading value: {meterReadValueString}.");
                return (numberOfErrors,error);
            }

            DateTime currentReadingDateTime = DateTime.Parse(values[1]);
            // check for duplicates by the composite key
            if (await _ensek.MeterReadings.AnyAsync(m => m.AccountId == accountId && m.MeterReadingDateTime == currentReadingDateTime))
            {
                numberOfErrors++;
                error = ($"Duplicate entry found for AccountId {accountId} and DateTime {currentReadingDateTime}.");
                return (numberOfErrors,error);
            }

            MeterRecord? existingRead = await _ensek.MeterReadings
                              .Where(m => m.AccountId == accountId)
                              .OrderByDescending(m => m.MeterReadingDateTime)
                              .FirstOrDefaultAsync();

            // looks for existing reading later in time than current one for the same account id
            if (existingRead != null && currentReadingDateTime <= existingRead?.MeterReadingDateTime)
            {
                numberOfErrors++;
                error=($"Existing entry {existingRead?.MeterReadingDateTime} is older than most recent entry for Accountid {accountId} and DateTime {currentReadingDateTime}");
                return (numberOfErrors, error);
            }

            return (numberOfErrors, error);
        }


    }
}
