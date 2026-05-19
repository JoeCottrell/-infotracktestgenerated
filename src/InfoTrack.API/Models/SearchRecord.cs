namespace InfoTrack.API.Models;

public class SearchRecord
{
    public int Id { get; set; }
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    public string LocationsJson { get; set; } = string.Empty;
    public int TotalResults { get; set; }
    public ICollection<Solicitor> Solicitors { get; set; } = new List<Solicitor>();
}
