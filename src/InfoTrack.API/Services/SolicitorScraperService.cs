using HtmlAgilityPack;
using InfoTrack.API.Models;
using InfoTrack.API.Services.Interfaces;
using System.Text.RegularExpressions;
using System.Web;

namespace InfoTrack.API.Services;

/// <summary>
/// Scrapes solicitor listings from solicitors.com/conveyancing.html.
/// Uses HttpClient + HtmlAgilityPack for HTML traversal; all extraction
/// logic is written explicitly (no scraping framework).
/// </summary>
public partial class SolicitorScraperService : ISolicitorScraperService
{
    private const string BaseUrl = "https://www.solicitors.com/conveyancing.html";
    private const int MaxPagesPerLocation = 5;

    private readonly HttpClient _httpClient;
    private readonly ILogger<SolicitorScraperService> _logger;

    public SolicitorScraperService(HttpClient httpClient, ILogger<SolicitorScraperService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<Solicitor>> ScrapeAsync(IEnumerable<string> locations, int searchRecordId)
    {
        var all = new List<Solicitor>();

        foreach (var location in locations)
        {
            try
            {
                var results = await ScrapeLocationAsync(location, searchRecordId);
                all.AddRange(results);
                _logger.LogInformation("Scraped {Count} solicitors for {Location}", results.Count, location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to scrape location {Location}", location);
            }

            // Be polite — small delay between location requests
            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        return all;
    }

    // ─── Per-location pagination loop ─────────────────────────────────────

    private async Task<List<Solicitor>> ScrapeLocationAsync(string location, int searchRecordId)
    {
        var solicitors = new List<Solicitor>();

        for (var page = 1; page <= MaxPagesPerLocation; page++)
        {
            var url = BuildUrl(location, page);
            var html = await FetchHtmlAsync(url);

            if (string.IsNullOrWhiteSpace(html))
                break;

            var (pageSolicitors, hasNextPage) = ParsePage(html, location, searchRecordId, url);
            solicitors.AddRange(pageSolicitors);

            if (!hasNextPage || pageSolicitors.Count == 0)
                break;

            await Task.Delay(TimeSpan.FromMilliseconds(300));
        }

        return solicitors;
    }

    // ─── URL builder ───────────────────────────────────────────────────────

    private static string BuildUrl(string location, int page)
    {
        var query = HttpUtility.UrlEncode(location);
        return page == 1
            ? $"{BaseUrl}?location={query}"
            : $"{BaseUrl}?location={query}&page={page}";
    }

    // ─── HTTP fetch ────────────────────────────────────────────────────────

    private async Task<string?> FetchHtmlAsync(string url)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/120.0.0.0 Safari/537.36");
            request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "HTTP request failed for {Url}", url);
            return null;
        }
    }

    // ─── HTML parsing ──────────────────────────────────────────────────────

    private (List<Solicitor> solicitors, bool hasNextPage) ParsePage(
        string html, string location, int searchRecordId, string sourceUrl)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var solicitors = new List<Solicitor>();

        // solicitors.com wraps each listing in a <div class="solicitor-result"> or similar.
        // We try multiple selector strategies to be resilient to markup changes.
        var listingNodes = FindListingNodes(doc);

        foreach (var node in listingNodes)
        {
            var solicitor = ExtractSolicitor(node, location, searchRecordId, sourceUrl);
            if (solicitor is not null)
                solicitors.Add(solicitor);
        }

        var hasNextPage = HasNextPage(doc);
        return (solicitors, hasNextPage);
    }

    // Tries a cascade of XPath selectors to locate result card nodes.
    private static IEnumerable<HtmlNode> FindListingNodes(HtmlDocument doc)
    {
        string[] selectors =
        [
            "//div[contains(@class,'solicitor-result')]",
            "//div[contains(@class,'search-result')]",
            "//article[contains(@class,'listing')]",
            "//div[contains(@class,'firm-listing')]",
            "//li[contains(@class,'result-item')]",
            "//div[contains(@class,'result')]",
        ];

        foreach (var xpath in selectors)
        {
            var nodes = doc.DocumentNode.SelectNodes(xpath);
            if (nodes is { Count: > 0 })
                return nodes;
        }

        return Enumerable.Empty<HtmlNode>();
    }

    // ─── Field extraction helpers ──────────────────────────────────────────

    private Solicitor? ExtractSolicitor(HtmlNode node, string location, int searchRecordId, string sourceUrl)
    {
        var name = ExtractName(node);
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return new Solicitor
        {
            Name          = name,
            Location      = location,
            Address       = ExtractAddress(node),
            Phone         = ExtractPhone(node),
            Email         = ExtractEmail(node),
            Website       = ExtractWebsite(node),
            Rating        = ExtractRating(node),
            ReviewCount   = ExtractReviewCount(node),
            Description   = ExtractDescription(node),
            SourceUrl     = sourceUrl,
            ScrapedAt     = DateTime.UtcNow,
            SearchRecordId = searchRecordId,
        };
    }

    private static string? ExtractName(HtmlNode node)
    {
        string[] nameXPaths =
        [
            ".//h2[contains(@class,'name')]",
            ".//h3[contains(@class,'name')]",
            ".//a[contains(@class,'firm-name')]",
            ".//span[contains(@class,'firm-name')]",
            ".//h2", ".//h3",
        ];

        foreach (var xpath in nameXPaths)
        {
            var n = node.SelectSingleNode(xpath);
            if (n is not null)
            {
                var text = CleanText(n.InnerText);
                if (!string.IsNullOrWhiteSpace(text))
                    return text;
            }
        }
        return null;
    }

    private static string? ExtractAddress(HtmlNode node)
    {
        var candidates = node.SelectNodes(
            ".//*[contains(@class,'address') or contains(@class,'location') or contains(@itemprop,'streetAddress')]");

        if (candidates is { Count: > 0 })
            return CleanText(candidates[0].InnerText);

        // Fallback: look for lines that look like postcodes
        var allText = node.InnerText;
        var match = PostcodeRegex().Match(allText);
        return match.Success ? null : null; // can't reliably extract without structure
    }

    private static string? ExtractPhone(HtmlNode node)
    {
        // Try tel: links first
        var telLink = node.SelectSingleNode(".//a[starts-with(@href,'tel:')]");
        if (telLink is not null)
            return CleanText(telLink.GetAttributeValue("href", "").Replace("tel:", ""));

        // Try elements with phone-related classes
        var phoneNode = node.SelectSingleNode(
            ".//*[contains(@class,'phone') or contains(@class,'tel') or contains(@itemprop,'telephone')]");
        if (phoneNode is not null)
            return CleanText(phoneNode.InnerText);

        // Regex fallback within the card text
        var match = PhoneRegex().Match(node.InnerText);
        return match.Success ? CleanText(match.Value) : null;
    }

    private static string? ExtractEmail(HtmlNode node)
    {
        var mailLink = node.SelectSingleNode(".//a[starts-with(@href,'mailto:')]");
        if (mailLink is not null)
            return CleanText(mailLink.GetAttributeValue("href", "").Replace("mailto:", "").Split('?')[0]);

        var emailNode = node.SelectSingleNode(
            ".//*[contains(@class,'email') or contains(@itemprop,'email')]");
        return emailNode is not null ? CleanText(emailNode.InnerText) : null;
    }

    private static string? ExtractWebsite(HtmlNode node)
    {
        // Explicit website links (not tel/mailto)
        var link = node.SelectSingleNode(
            ".//a[contains(@class,'website') or contains(@rel,'external') or contains(text(),'Visit')]");
        if (link is not null)
            return link.GetAttributeValue("href", null);

        return null;
    }

    private static double? ExtractRating(HtmlNode node)
    {
        // Try itemprop=ratingValue or aria-label="X out of 5"
        var ratingNode = node.SelectSingleNode(
            ".//*[@itemprop='ratingValue' or contains(@class,'rating-value') or contains(@class,'stars')]");

        if (ratingNode is not null)
        {
            var raw = ratingNode.GetAttributeValue("content",
                        ratingNode.GetAttributeValue("aria-label", ratingNode.InnerText));
            if (double.TryParse(ExtractFirstDecimal(raw), out var r))
                return Math.Round(r, 1);
        }

        return null;
    }

    private static int? ExtractReviewCount(HtmlNode node)
    {
        var reviewNode = node.SelectSingleNode(
            ".//*[contains(@class,'review-count') or contains(@class,'reviews') or contains(@itemprop,'reviewCount')]");

        if (reviewNode is not null)
        {
            var match = Regex.Match(reviewNode.InnerText, @"\d+");
            if (match.Success && int.TryParse(match.Value, out var count))
                return count;
        }

        // Regex fallback: "123 reviews" or "(123)"
        var textMatch = Regex.Match(node.InnerText, @"(\d+)\s*review", RegexOptions.IgnoreCase);
        if (textMatch.Success && int.TryParse(textMatch.Groups[1].Value, out var fallback))
            return fallback;

        return null;
    }

    private static string? ExtractDescription(HtmlNode node)
    {
        var desc = node.SelectSingleNode(
            ".//*[contains(@class,'description') or contains(@class,'summary') or contains(@class,'about')]");
        return desc is not null ? CleanText(desc.InnerText) : null;
    }

    private static bool HasNextPage(HtmlDocument doc)
    {
        var nextLink = doc.DocumentNode.SelectSingleNode(
            "//a[contains(@class,'next') or contains(@rel,'next') or contains(text(),'Next')]");
        return nextLink is not null;
    }

    // ─── Utility ───────────────────────────────────────────────────────────

    private static string CleanText(string? raw) =>
        HttpUtility.HtmlDecode(raw ?? string.Empty).Trim();

    private static string? ExtractFirstDecimal(string text)
    {
        var m = Regex.Match(text, @"\d+(\.\d+)?");
        return m.Success ? m.Value : null;
    }

    [GeneratedRegex(@"[A-Z]{1,2}\d{1,2}[A-Z]?\s*\d[A-Z]{2}", RegexOptions.IgnoreCase)]
    private static partial Regex PostcodeRegex();

    [GeneratedRegex(@"(\+44|0)[\s\-]?(\d[\s\-]?){9,10}")]
    private static partial Regex PhoneRegex();
}
