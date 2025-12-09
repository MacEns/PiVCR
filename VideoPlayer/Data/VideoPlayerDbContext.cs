using Microsoft.EntityFrameworkCore;
using VideoPlayer.Models;

namespace VideoPlayer.Data;

public class VideoPlayerDbContext : DbContext
{
    public DbSet<VideoFile> VideoFiles { get; set; }

    public VideoPlayerDbContext()
    {
    }

    public VideoPlayerDbContext(DbContextOptions<VideoPlayerDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VideoPlayer");
            Directory.CreateDirectory(dbPath);
            var connectionString = $"Data Source={Path.Combine(dbPath, "videoplayer.db")}";
            optionsBuilder.UseSqlite(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VideoFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.VideoCodec).HasMaxLength(50);
            entity.Property(e => e.AudioCodec).HasMaxLength(50);
            entity.Property(e => e.Genre).HasMaxLength(100);
            entity.Property(e => e.ThumbnailPath).HasMaxLength(500);

            // Add index on frequently searched fields
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.FilePath).IsUnique();
            entity.HasIndex(e => e.Genre);
            entity.HasIndex(e => e.DateAdded);
            entity.HasIndex(e => e.LastPlayed);
            entity.HasIndex(e => e.IsFavorite);
        });

        base.OnModelCreating(modelBuilder);
    }
}
