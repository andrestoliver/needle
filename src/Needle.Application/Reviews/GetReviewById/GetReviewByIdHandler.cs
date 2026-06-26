namespace Needle.Application.Reviews.GetReviewById;

public sealed class GetReviewByIdHandler
{
    private readonly IReviewRepository _reviewRepository;

    public GetReviewByIdHandler(IReviewRepository reviewRepository)
    {
        ArgumentNullException.ThrowIfNull(reviewRepository);

        _reviewRepository = reviewRepository;
    }

    public async Task<GetReviewByIdResult> HandleAsync(
        GetReviewByIdQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var review = await _reviewRepository.GetDetailsByIdAsync(
            query.ReviewId,
            cancellationToken);

        if (review is null || review.AlbumId != query.AlbumId)
        {
            return new GetReviewByIdResult(
                GetReviewByIdStatus.ReviewNotFound,
                null);
        }

        return new GetReviewByIdResult(
            GetReviewByIdStatus.Found,
            review);
    }
}