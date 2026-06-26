namespace Needle.Domain.Reviews;

public class Review
{
    private Review(
        Guid id,
        Guid albumId,
        Guid userId,
        Rating rating,
        string? text,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty", nameof(id));
        }

        if (albumId == Guid.Empty)
        {
            throw new ArgumentException("AlbumId cannot be empty", nameof(albumId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        }
        
        if (rating.Value == 0)
        {
            throw new ArgumentException("Rating is required.", nameof(rating));
        }

        var normalizedText = NormalizeText(text);

        if (createdAt == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(createdAt));
        }
        
        Id = id;
        AlbumId = albumId;
        UserId = userId;
        Rating = rating;
        Text = normalizedText;
        CreatedAt = createdAt;
    }
    
    public static Review Create(
        Guid id,
        Guid albumId,
        Guid userId,
        Rating rating,
        string? text,
        DateTimeOffset createdAt)
    {
        return new Review(
            id,
            albumId,
            userId,
            rating,
            text,
            createdAt);
    }
    
    public void Update(
        Rating rating,
        string? text,
        DateTimeOffset updatedAt)
    {
        if (rating.Value == 0)
        {
            throw new ArgumentException("Rating is required.", nameof(rating));
        }

        var normalizedText = NormalizeText(text);

        if (updatedAt == default)
        {
            throw new ArgumentException("UpdatedAt cannot be default.", nameof(updatedAt));
        }

        Rating = rating;
        Text = normalizedText;
        UpdatedAt = updatedAt;
    }
    
    private static string? NormalizeText(string? text)
    {
        var normalizedText = string.IsNullOrWhiteSpace(text)
            ? null
            : text.Trim();

        if (normalizedText is not null && normalizedText.Length > 2000)
        {
            throw new ArgumentException(
                "Text cannot be longer than 2000 characters.",
                nameof(text));
        }

        return normalizedText;
    }
    
    public Guid Id { get; }
    public Guid AlbumId { get; }
    public Guid UserId { get; }
    public Rating Rating { get; private set; }
    public string? Text { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? UpdatedAt { get; private set; }
}