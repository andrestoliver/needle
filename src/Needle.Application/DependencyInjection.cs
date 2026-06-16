using Microsoft.Extensions.DependencyInjection;
using Needle.Application.Albums.CreateAlbum;
using Needle.Application.Albums.GetAlbumById;
using Needle.Application.Albums.ImportAlbum;
using Needle.Application.Albums.SearchExternalAlbums;
using Needle.Application.Reviews.CreateReview;

namespace Needle.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateAlbumHandler>();
        services.AddScoped<GetAlbumByIdHandler>();
        services.AddScoped<SearchExternalAlbumsHandler>();
        services.AddScoped<ImportAlbumHandler>();
        services.AddScoped<CreateReviewHandler>();

        return services;
    }
}