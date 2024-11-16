using Ensek.Model;

namespace Ensek.Functions
{
    public class helperFunctions
    {
        private readonly EnsekDbContext _ensekDContext;
        private readonly ILogger<helperFunctions> _logger;
        public helperFunctions(EnsekDbContext ensek,
                               ILogger<helperFunctions> logger
                              )
        {
            _ensekDContext = ensek;
            _logger = logger;
        }
        public static string ValidateFileFormat(IFormFile file)
        {
            string fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (fileExtension != ".csv" && fileExtension != ".xlsx")
            {
                return "Invalid file type. Only .csv or .xlsx files are allowed.";
            }

            return string.Empty;
        }

        public static string ValidateFileHeader(IFormFile file)
        {
            string[] expectedHeaders = typeof(MeterRecord)
                .GetProperties()
                .Select(p => p.Name)
                .ToArray();

            using (MemoryStream stream = new MemoryStream())
            {
                file.CopyToAsync(stream);
                stream.Position = 0;

                using (StreamReader reader = new StreamReader(stream))
                {
                    string? headerLine = reader.ReadLine();
                    if (headerLine == null)
                    {
                        return ("No Headers has been found for file.");
                    }

                    string[] headers = headerLine.Split(',');

                    if (!headers.SequenceEqual(expectedHeaders, StringComparer.OrdinalIgnoreCase))
                    {
                        return ("headers do not match the expected format.");
                    }
                }

            }

            return string.Empty;
        }

        public async Task<int> SaveBatchAsync(List<MeterRecord> batch)
        {
            try
            {
                await _ensekDContext.MeterReadings.AddRangeAsync(batch);
                await _ensekDContext.SaveChangesAsync();
                return batch.Count;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());
                throw new Exception("Error saving batch to the database.", ex);
            }
        }
    }
}
