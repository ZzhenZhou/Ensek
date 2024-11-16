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

}
}
