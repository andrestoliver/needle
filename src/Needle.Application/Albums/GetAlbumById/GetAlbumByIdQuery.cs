namespace Needle.Application.Albums.GetAlbumById;

public sealed record GetAlbumByIdQuery
{
    public GetAlbumByIdQuery(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Album id cannot be empty.",
                nameof(id));
        }

        Id = id;
    }

    public Guid Id { get; }
}