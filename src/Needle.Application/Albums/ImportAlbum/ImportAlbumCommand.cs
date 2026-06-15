namespace Needle.Application.Albums.ImportAlbum;

public sealed record ImportAlbumCommand
{
    public ImportAlbumCommand(string externalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);
        
        var normalizedExternalId = externalId.Trim();

        if (!Guid.TryParseExact(normalizedExternalId, "D", out var parsedExternalId))
        {
            throw new ArgumentException(
                "External id must be a valid UUID.",
                nameof(externalId));
        }

        ExternalId = parsedExternalId.ToString("D");
    }

    public string ExternalId { get; }
}