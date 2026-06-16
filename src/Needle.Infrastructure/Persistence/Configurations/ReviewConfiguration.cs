using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Needle.Domain.Albums;
using Needle.Domain.Reviews;

namespace Needle.Infrastructure.Persistence.Configurations;

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(review => review.Id);

        builder.Property(review => review.Id)
            .HasColumnName("id");

        builder.Property(review => review.AlbumId)
            .HasColumnName("album_id")
            .IsRequired();
        
        builder.HasOne<Album>()
            .WithMany()
            .HasForeignKey(review => review.AlbumId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(review => review.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(review => review.Rating)
            .HasColumnName("rating")
            .HasConversion(
                rating => rating.Value,
                value => new Rating(value))
            .HasPrecision(2, 1)
            .IsRequired();

        builder.Property(review => review.Text)
            .HasColumnName("text")
            .HasMaxLength(2000);

        builder.Property(review => review.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(review => review.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(review => new
            {
                review.AlbumId,
                review.UserId
            })
            .IsUnique()
            .HasDatabaseName("ux_reviews_album_id_user_id");
    }
}