using Microsoft.AspNetCore.Mvc;
using Ensek.Functions;
using Microsoft.EntityFrameworkCore;

namespace Ensek.Controllers
{
    [Route("api/meter-reading-uploads")]
    [ApiController]
    public class MeterReadingUploadController : ControllerBase
    {
        private readonly EnsekDbContext _ensekDbContext;
        private readonly ILogger<CsvProcessing> _logger;
        private readonly helperFunctions _helperFunctions;

        public MeterReadingUploadController(EnsekDbContext context,
                                            ILogger<CsvProcessing> logger,
                                            helperFunctions helperFunctions)
        {
            _ensekDbContext = context;
            _logger = logger;
            _helperFunctions = helperFunctions;
        }

        [HttpPost]
        public async Task<IActionResult> UploadMeterReading(IFormFile file)
        {
            string isValidFileFormat = helperFunctions.ValidateFileFormat(file);
            string isValidFileHeader = helperFunctions.ValidateFileHeader(file);


            if (!string.IsNullOrEmpty(isValidFileFormat))
            {
                return BadRequest(isValidFileFormat);
            }

            if (!string.IsNullOrEmpty(isValidFileHeader))
            {
                return BadRequest(isValidFileHeader);
            }

            using (MemoryStream stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                CsvProcessing csvProcessing = new CsvProcessing(_ensekDbContext,_helperFunctions);

                (int Successes,List<(int Rownumber, string ErrorMessage)> FailedRows) = await csvProcessing.ProcessCsv(stream);

                return Ok(new
                {
                    Message = "File uploaded and processed.",
                    SuccessfulWrites = Successes,
                    FailedWrites = FailedRows.Select(f => new
                    {
                        TotalFailed = FailedRows.Count(),
                        RowNumber = f.Rownumber.ToString(),
                        ErrorMessage = f.ErrorMessage
                    })
                });
            }
        }
    }
}
