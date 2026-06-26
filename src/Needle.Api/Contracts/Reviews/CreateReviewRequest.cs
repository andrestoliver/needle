namespace Needle.Api.Contracts.Reviews;

public sealed record CreateReviewRequest(
    decimal Rating,
    string? Text);