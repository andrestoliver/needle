using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Needle.Domain.Albums;

namespace Needle.Infrastructure.Persistence.Configurations;

public sealed class AlbumConfiguration : IEntityTypeConfiguration<Album>
{
    public void Configure(EntityTypeBuilder<Album> builder)
    {
        builder.ToTable("albums");

        builder.HasKey(album => album.Id);

        builder.Property(album => album.Id)
            .HasColumnName("id");

        builder.Property(album => album.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(album => album.ArtistName)
            .HasColumnName("artist_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(album => album.ReleaseYear)
            .HasColumnName("release_year")
            .IsRequired();
    }
}