using Microsoft.AspNetCore.Mvc;
using Ensek.Functions;
using Microsoft.EntityFrameworkCore;

namespace Ensek.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeterReadingUploadController : ControllerBase
    {
        private readonly EnsekDbContext _ensekDbContext;
        private readonly ILogger<CsvProcessing> _logger;

        public MeterReadingUploadController(EnsekDbContext context, ILogger<CsvProcessing> logger)
        {
            _ensekDbContext = context;
            _logger = logger;
        }

        [HttpPost("meter-reading-uploads")]
        public async Task<IActionResult> UploadMeterReading(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Invalid file");
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (fileExtension != ".csv" && fileExtension != ".xlsx")
            {
                return BadRequest("Invalid file type. Only .csv or .xlsx files are allowed.");
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
