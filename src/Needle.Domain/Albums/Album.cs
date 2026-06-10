namespace Needle.Domain.Albums;

public class Album
{
    public Album(Guid id, string title, string artistName, int releaseYear)
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
    }

    public Guid Id { get; }
    public string Title { get; }
    public string ArtistName { get; }
    public int ReleaseYear { get; }
}
