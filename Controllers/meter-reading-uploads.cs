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
                                            ILogger<CsvProcessing> logger
                                            )
        {
            _ensekDbContext = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> UploadMeterReading(IFormFile file)
        {
            string isFileValid = helperFunctions.ValidateFileFormat(file);

            if (!string.IsNullOrEmpty(isFileValid))
            {
                BadRequest(isFileValid);
            }
            
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                var csvProcessing = new CsvProcessing(_ensekDbContext, _logger);

                // Process the CSV file
                (int Successes,int failes) = await csvProcessing.ProcessCsv(stream);

                return Ok(new
                {
                    Message = "File uploaded and processed.",
                    SuccessfulWrites = Successes,
                    FailedWrites = failes
                });
            }
        }
    }
}
