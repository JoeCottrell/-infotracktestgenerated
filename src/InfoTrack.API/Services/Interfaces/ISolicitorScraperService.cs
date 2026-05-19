using InfoTrack.API.Models;

namespace InfoTrack.API.Services.Interfaces;

public interface ISolicitorScraperService
{
    Task<List<Solicitor>> ScrapeAsync(IEnumerable<string> locations, int searchRecordId);
}
