using Needle.Domain.Reviews;

namespace Needle.Application.Reviews.CreateReview;

public sealed record CreateReviewResult(
    CreateReviewStatus Status,
    Review? Review);