using Needle.Domain.Reviews;

namespace Needle.Application.Reviews.UpdateReview;

public sealed record UpdateReviewResult(
    UpdateReviewStatus Status,
    Review? Review);