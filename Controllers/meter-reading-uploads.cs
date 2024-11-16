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

        public MeterReadingUploadController(EnsekDbContext context
                                            , ILogger<CsvProcessing> logger)
        {
            _ensekDbContext = context;
            _logger = logger;
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

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                var csvProcessing = new CsvProcessing(_ensekDbContext, _logger);

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
