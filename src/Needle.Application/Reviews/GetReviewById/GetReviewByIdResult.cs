namespace Needle.Application.Reviews.GetReviewById;

public sealed record GetReviewByIdResult(
    GetReviewByIdStatus Status,
    ReviewDetails? Review);