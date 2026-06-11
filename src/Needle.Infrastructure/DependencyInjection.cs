using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Needle.Application.Albums;
using Needle.Infrastructure.Persistence;
using Needle.Infrastructure.Persistence.Repositories;

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
            options.UseNpgsql(connectionString));

        services.AddScoped<IAlbumRepository, AlbumRepository>();

        return services;
    }
}