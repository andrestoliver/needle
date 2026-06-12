namespace Needle.Domain.Albums;

public class Album
{
    private Album(Guid id, string? externalId, string title, string artistName, int releaseYear)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Album id cannot be empty", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(artistName))
        {
            throw new ArgumentException("Artist name cannot be empty", nameof(artistName));
        }

        if (releaseYear < 1877)
        {
            throw new ArgumentOutOfRangeException(nameof(releaseYear),
                "Release year must be greater than or equal to 1877");
        }

        Id = id;
        Title = title.Trim();
        ArtistName = artistName.Trim();
        ReleaseYear = releaseYear;
        ExternalId = externalId;
    }

    public Guid Id { get; }
    public string Title { get; }
    public string ArtistName { get; }
    public int ReleaseYear { get; }
    public string? ExternalId { get; }

    public static Album CreateManual(Guid id, 
        string title, 
        string artistName, 
        int releaseYear)
    {
        return new Album(
            id,
            null,
            title,
            artistName,
            releaseYear );
    }

    public static Album ImportFromMusicBrainz(Guid id,
        string externalId,
        string title,
        string artistName,
        int releaseYear)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);
        
        var normalizedExternalId = externalId.Trim();

        if (!Guid.TryParse(normalizedExternalId, out _))
        {
            throw new ArgumentException(
                "MusicBrainz external id must be a valid UUID.",
                nameof(externalId));
        }
        
        return new Album(id, externalId.Trim(), title, artistName, releaseYear);
    }
}
