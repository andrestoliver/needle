namespace Needle.Api.Contracts.Reviews;

public sealed record UpdateReviewRequest(
    Guid UserId,
    decimal Rating,
    string? Text);