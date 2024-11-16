using Ensek.Model;

namespace Ensek.Functions
{
    public class helperFunctions
    {
        public static string ValidateFileFormat(IFormFile file)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (fileExtension != ".csv" && fileExtension != ".xlsx")
            {
                return "Invalid file type. Only .csv or .xlsx files are allowed.";
            }

            return string.Empty;
        }

        public static string ValidateFileHeader(IFormFile file)
        {
            var expectedHeaders = typeof(MeterRecord)
                .GetProperties()
                .Select(p => p.Name)
                .ToArray();

            using (var stream = new MemoryStream())
            {
                file.CopyToAsync(stream);
                stream.Position = 0;

                using (var reader = new StreamReader(stream))
                {
                    var headerLine = reader.ReadLine();
                    if (headerLine == null)
                    {
                        return ("No Headers has been found for file.");
                    }

                    var headers = headerLine.Split(',');

                    if (!headers.SequenceEqual(expectedHeaders, StringComparer.OrdinalIgnoreCase))
                    {
                        return ("headers do not match the expected format.");
                    }
                }

            }

            return string.Empty;
        }
    }
}
