using System.Text.Json;
using InfoTrack.API.Data;
using InfoTrack.API.DTOs;
using InfoTrack.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InfoTrack.API.Services;

public class SearchHistoryService : ISearchHistoryService
{
    private readonly AppDbContext _db;

    public SearchHistoryService(AppDbContext db) => _db = db;

    public async Task<List<SearchHistoryItemDto>> GetHistoryAsync() =>
        await _db.SearchRecords
            .OrderByDescending(r => r.SearchedAt)
            .Select(r => new SearchHistoryItemDto(
                r.Id,
                r.SearchedAt,
                JsonSerializer.Deserialize<List<string>>(r.LocationsJson) ?? new List<string>(),
                r.TotalResults
            ))
            .ToListAsync();

    public async Task<SearchResultDto?> GetSearchResultAsync(int searchRecordId)
    {
        var record = await _db.SearchRecords
            .Include(r => r.Solicitors)
            .FirstOrDefaultAsync(r => r.Id == searchRecordId);

        if (record is null) return null;

        var locations = JsonSerializer.Deserialize<List<string>>(record.LocationsJson) ?? new();

        return new SearchResultDto(
            record.Id,
            record.SearchedAt,
            locations,
            record.TotalResults,
            record.Solicitors.Select(ToDto).ToList()
        );
    }

    public async Task<ReportDto?> GetReportAsync(int searchRecordId)
    {
        var record = await _db.SearchRecords
            .Include(r => r.Solicitors)
            .FirstOrDefaultAsync(r => r.Id == searchRecordId);

        if (record is null) return null;

        var locations = JsonSerializer.Deserialize<List<string>>(record.LocationsJson) ?? new();
        var solicitors = record.Solicitors.ToList();

        var locationSummaries = solicitors
            .GroupBy(s => s.Location)
            .Select(g => new LocationSummaryDto(
                g.Key,
                g.Count(),
                g.Where(s => s.Rating.HasValue).Select(s => s.Rating!.Value).DefaultIfEmpty().Average(),
                g.Where(s => s.ReviewCount.HasValue).Sum(s => s.ReviewCount!.Value)
            ))
            .OrderByDescending(l => l.Count)
            .ToList();

        var topRated = solicitors
            .Where(s => s.Rating.HasValue)
            .OrderByDescending(s => s.Rating)
            .ThenByDescending(s => s.ReviewCount)
            .Take(10)
            .Select(ToDto)
            .ToList();

        return new ReportDto(
            record.Id,
            record.SearchedAt,
            locations,
            record.TotalResults,
            locationSummaries,
            topRated,
            solicitors.Select(ToDto).ToList()
        );
    }

    private static SolicitorDto ToDto(Models.Solicitor s) => new(
        s.Id, s.Name, s.Location, s.Address, s.Phone,
        s.Email, s.Website, s.Rating, s.ReviewCount,
        s.Description, s.SourceUrl, s.ScrapedAt
    );
}
