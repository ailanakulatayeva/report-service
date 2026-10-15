using API.DTOs.Responses;
using API.Services.Implementations;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace API.Controllers;

public class ReportExecutionController(ISSRSService ssrsService, ILogger<ReportExecutionController> logger)
    : BaseController<ReportExecutionController>(logger)
{

    [HttpGet("{id:int}/render")]
    [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> Render(
        int id,
        [FromQuery] Dictionary<string, string>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ssrsService.RenderReportAsync(id, parameters ?? [], cancellationToken);
        return File(result.FileContent, result.ContentType, result.FileName);
    }

    [HttpGet("ssrs")]
    [ProducesResponseType(typeof(List<SsrsReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSsrs()
    {
        var result = await ssrsService.GetSsrsList();
        return Ok(result);
    }

    [HttpGet("ssrs/parameters")]
    [ProducesResponseType(typeof(List<SsrsParameterDefinition>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSsrsParameters([FromQuery] string reportPath)
    {
        var result = await ssrsService.GetReportParameter(reportPath);
        return Ok(result);
    }

}