using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Needle.Application.Albums;
using Needle.Application.Albums.SearchExternalAlbums;
using Needle.Application.Common.Time;
using Needle.Application.Reviews;
using Needle.Infrastructure.ExternalCatalog.MusicBrainz;
using Needle.Infrastructure.Persistence;
using Needle.Infrastructure.Persistence.Repositories;
using Needle.Infrastructure.Time;

namespace Needle.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration
                                   .GetConnectionString("NeedleDatabase") 
                               ?? throw new InvalidOperationException(
                                   "Connection string 'NeedleDatabase' was not found.");

        services.AddDbContext<NeedleDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IAlbumRepository, AlbumRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();

        services.AddSingleton<IClock, SystemClock>();

        services.AddHttpClient<IExternalAlbumCatalog, MusicBrainzAlbumCatalog>(
            client =>
            {
                client.BaseAddress = new Uri("https://musicbrainz.org");
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Needle/1.0 (https://github.com/andrestoliver/needle)");
                client.Timeout = TimeSpan.FromSeconds(10);
            });

        return services;
    }
}