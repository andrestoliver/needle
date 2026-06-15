using Needle.Domain.Albums;

namespace Needle.Application.Albums.ImportAlbum;

public sealed record ImportAlbumResult(
    ImportAlbumStatus Status,
    Album? Album);