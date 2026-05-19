using System.Text.Json;
using InfoTrack.API.Data;
using InfoTrack.API.DTOs;
using InfoTrack.API.Models;
using InfoTrack.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InfoTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SolicitorsController : ControllerBase
{
    private readonly ISolicitorScraperService _scraper;
    private readonly ILocationService _locationService;
    private readonly ISearchHistoryService _historyService;
    private readonly AppDbContext _db;

    public SolicitorsController(
        ISolicitorScraperService scraper,
        ILocationService locationService,
        ISearchHistoryService historyService,
        AppDbContext db)
    {
        _scraper = scraper;
        _locationService = locationService;
        _historyService = historyService;
        _db = db;
    }

    /// <summary>
    /// Trigger a new scrape. Optionally pass a custom list of locations;
    /// defaults to all active locations configured in the database.
    /// </summary>
    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchRequest? request)
    {
        List<string> locations;

        if (request?.Locations is { Count: > 0 })
            locations = request.Locations;
        else
            locations = await _locationService.GetActiveNamesAsync();

        if (locations.Count == 0)
            return BadRequest("No locations configured. Add locations first.");

        // Create the search record shell so we have an ID for FK references.
        var record = new SearchRecord
        {
            SearchedAt   = DateTime.UtcNow,
            LocationsJson = JsonSerializer.Serialize(locations),
        };
        _db.SearchRecords.Add(record);
        await _db.SaveChangesAsync();

        var solicitors = await _scraper.ScrapeAsync(locations, record.Id);

        record.TotalResults = solicitors.Count;
        _db.Solicitors.AddRange(solicitors);
        await _db.SaveChangesAsync();

        var result = await _historyService.GetSearchResultAsync(record.Id);
        return Ok(result);
    }

    /// <summary>Returns the full results for a previous search.</summary>
    [HttpGet("{searchRecordId:int}")]
    public async Task<IActionResult> GetResult(int searchRecordId)
    {
        var result = await _historyService.GetSearchResultAsync(searchRecordId);
        return result is null ? NotFound() : Ok(result);
    }
}
