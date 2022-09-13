using Ensek.Business.Parser;
using Microsoft.AspNetCore.Mvc;

namespace Ensek.Web.Controllers;

[ApiController]
[Route("meter-reading-uploads")]
public class MeterReadingController : ControllerBase
{
    private readonly IMeterReadingCSVParser _parser;
    private readonly ILogger<MeterReadingController> _logger;

    public MeterReadingController(IMeterReadingCSVParser parser, ILogger<MeterReadingController> logger)
    {
        _parser = parser;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<MeterReadingUploadResult>> Post(IFormFile file)
    {
        if (file.ContentType != "text/csv")
        {
            return BadRequest("Uploaded file must be in text/csv format");
        }

        try
        {
            var result = await _parser.Parse(() => file.OpenReadStream());
            return new MeterReadingUploadResult()
            {
                Successful = result.Successful,
                Failed = result.Failed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred parsing CSV");
            throw ex;
        }
    }
}