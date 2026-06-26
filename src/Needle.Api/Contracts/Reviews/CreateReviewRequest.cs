namespace Needle.Api.Contracts.Reviews;

public sealed record CreateReviewRequest(
    Guid UserId,
    decimal Rating,
    string? Text);