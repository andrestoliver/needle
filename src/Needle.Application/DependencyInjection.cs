using Microsoft.Extensions.DependencyInjection;
using Needle.Application.Albums.CreateAlbum;
using Needle.Application.Albums.GetAlbumById;
using Needle.Application.Albums.SearchExternalAlbums;

namespace Needle.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateAlbumHandler>();
        services.AddScoped<GetAlbumByIdHandler>();
        services.AddScoped<SearchExternalAlbumsHandler>();

        return services;
    }
}