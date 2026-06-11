using Microsoft.Extensions.DependencyInjection;
using Needle.Application.Albums.CreateAlbum;

namespace Needle.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateAlbumHandler>();

        return services;
    }
}