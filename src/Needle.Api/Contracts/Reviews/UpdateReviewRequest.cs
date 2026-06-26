namespace Needle.Api.Contracts.Reviews;

public sealed record UpdateReviewRequest(
    decimal Rating,
    string? Text);