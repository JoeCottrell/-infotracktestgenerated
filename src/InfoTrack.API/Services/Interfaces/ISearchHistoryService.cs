using InfoTrack.API.DTOs;

namespace InfoTrack.API.Services.Interfaces;

public interface ISearchHistoryService
{
    Task<List<SearchHistoryItemDto>> GetHistoryAsync();
    Task<SearchResultDto?> GetSearchResultAsync(int searchRecordId);
    Task<ReportDto?> GetReportAsync(int searchRecordId);
}
