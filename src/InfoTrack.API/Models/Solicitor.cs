namespace InfoTrack.API.Models;

public class Solicitor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public double? Rating { get; set; }
    public int? ReviewCount { get; set; }
    public string? Description { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public DateTime ScrapedAt { get; set; } = DateTime.UtcNow;

    public int SearchRecordId { get; set; }
    public SearchRecord SearchRecord { get; set; } = null!;
}
