using Needle.Application.Albums;

namespace Needle.Application.Reviews.ListReviewsByAlbum;

public sealed class ListReviewsByAlbumHandler
{
    private readonly IAlbumRepository _albumRepository;
    private readonly IReviewRepository _reviewRepository;

    public ListReviewsByAlbumHandler(
        IAlbumRepository albumRepository,
        IReviewRepository reviewRepository)
    {
        ArgumentNullException.ThrowIfNull(albumRepository);
        ArgumentNullException.ThrowIfNull(reviewRepository);

        _albumRepository = albumRepository;
        _reviewRepository = reviewRepository;
    }

    public async Task<ListReviewsByAlbumResult> HandleAsync(
        ListReviewsByAlbumQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var album = await _albumRepository.GetByIdAsync(
            query.AlbumId,
            cancellationToken);

        if (album is null)
        {
            return new ListReviewsByAlbumResult(
                ListReviewsByAlbumStatus.AlbumNotFound,
                []);
        }

        var reviews = await _reviewRepository.ListByAlbumAsync(
            query.AlbumId,
            cancellationToken);

        return new ListReviewsByAlbumResult(
            ListReviewsByAlbumStatus.Found,
            reviews);
    }
}