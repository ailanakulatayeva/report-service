using API.Domain.Entities;
using API.Domain.Enums;
using API.DTOs.Requests;
using API.DTOs.Responses;
using API.Errors.Exceptions;
using API.Services.Interfaces;
using Contracts.Messages;
using Contracts.Parameters;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using static System.Net.Mime.MediaTypeNames;


namespace API.Controllers;

public class ReportsController : BaseController<ReportsController>
{
    private readonly IReportService _service;

    public ReportsController(IReportService service, ILogger<ReportsController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpGet("/reports")]
    [ProducesResponseType(typeof(SearchResult<ReportDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAll([FromQuery] Pagination pagination)
    {
        var reports = await _service.GetAllAsync(pagination);
        return Result.Ok(reports).Respond();
    }

    [HttpGet("/reports/{id:int}")]
    [ProducesResponseType(typeof(ReportDto), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var report = await _service.GetByIdAsync(id);

        if (report == null)
            throw new NotFoundException("Отчет не найден.");

        return Result.Ok(report).Respond();
    }

    [HttpPost("/reports")]
    [ProducesResponseType(typeof(ReportDto), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> Create([FromBody] CreateReportDto report)
    {
        var created = await _service.CreateAsync(report);
        return Result.Ok(created).Respond();
    }

    [HttpPut("/reports/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReportDto input)
    {
        var updated = await _service.UpdateAsync(id, input);

        if (updated == null)
            throw new NotFoundException("Отчет не найден.");

        return Result.Ok(updated).Respond();
    }

    [HttpPut("/reports/{id:int}/parameters")]
    [ProducesResponseType(typeof(ReportDto), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> UpdateParameters(int id)
    {
        var updated = await _service.UpdateParametersAsync(id);
        return Result.Ok(updated).Respond();
    }

    [HttpDelete("/reports/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            throw new NotFoundException("Отчет не найден.");

        return Result.Ok().Respond();
    }
}
