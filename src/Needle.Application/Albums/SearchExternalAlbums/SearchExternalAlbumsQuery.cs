namespace Needle.Application.Albums.SearchExternalAlbums;

public sealed record SearchExternalAlbumsQuery
{
    public SearchExternalAlbumsQuery(string query, int limit = 10)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(limit, 20);

        Query = query.Trim();
        Limit = limit;
    }

    public string Query { get; }
    public int Limit { get; }
}
