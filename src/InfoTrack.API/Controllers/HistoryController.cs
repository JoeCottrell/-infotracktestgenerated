using InfoTrack.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InfoTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly ISearchHistoryService _history;

    public HistoryController(ISearchHistoryService history) => _history = history;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _history.GetHistoryAsync());

    [HttpGet("{searchRecordId:int}/report")]
    public async Task<IActionResult> GetReport(int searchRecordId)
    {
        var report = await _history.GetReportAsync(searchRecordId);
        return report is null ? NotFound() : Ok(report);
    }
}
