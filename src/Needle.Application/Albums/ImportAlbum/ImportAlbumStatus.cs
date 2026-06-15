namespace Needle.Application.Albums.ImportAlbum;

public enum ImportAlbumStatus
{
    Imported,
    AlreadyImported,
    ExternalAlbumNotFound,
    MissingReleaseYear
}