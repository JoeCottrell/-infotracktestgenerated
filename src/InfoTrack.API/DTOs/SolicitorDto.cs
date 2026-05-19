namespace InfoTrack.API.DTOs;

public record SolicitorDto(
    int Id,
    string Name,
    string Location,
    string? Address,
    string? Phone,
    string? Email,
    string? Website,
    double? Rating,
    int? ReviewCount,
    string? Description,
    string SourceUrl,
    DateTime ScrapedAt
);

public record SearchRequest(List<string>? Locations = null);

public record SearchResultDto(
    int SearchRecordId,
    DateTime SearchedAt,
    List<string> Locations,
    int TotalResults,
    List<SolicitorDto> Solicitors
);

public record SearchHistoryItemDto(
    int Id,
    DateTime SearchedAt,
    List<string> Locations,
    int TotalResults
);

public record ReportDto(
    int SearchRecordId,
    DateTime SearchedAt,
    List<string> Locations,
    int TotalResults,
    List<LocationSummaryDto> LocationSummaries,
    List<SolicitorDto> TopRated,
    List<SolicitorDto> AllSolicitors
);

public record LocationSummaryDto(
    string Location,
    int Count,
    double? AverageRating,
    int TotalReviews
);
