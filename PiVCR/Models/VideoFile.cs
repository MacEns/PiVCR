using System.ComponentModel.DataAnnotations;

namespace PiVCR.Models;

public class VideoFile
{
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public long FileSizeBytes { get; set; }

    public TimeSpan? Duration { get; set; }

    [MaxLength(50)]
    public string? VideoCodec { get; set; }

    [MaxLength(50)]
    public string? AudioCodec { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public double? FrameRate { get; set; }

    public long? BitRate { get; set; }

    [MaxLength(100)]
    public string? Genre { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    public DateTime? LastPlayed { get; set; }

    public int PlayCount { get; set; } = 0;

    public bool IsFavorite { get; set; } = false;

    [MaxLength(500)]
    public string? ThumbnailPath { get; set; }

    // Computed property for display
    public string DisplayName => !string.IsNullOrWhiteSpace(Title) ? Title : Path.GetFileNameWithoutExtension(FilePath);

    public string FileSizeDisplay => FileSizeBytes switch
    {
        < 1024 => $"{FileSizeBytes} B",
        < 1024 * 1024 => $"{FileSizeBytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{FileSizeBytes / (1024.0 * 1024):F1} MB",
        _ => $"{FileSizeBytes / (1024.0 * 1024 * 1024):F1} GB"
    };

    public string ResolutionDisplay => Width.HasValue && Height.HasValue ? $"{Width}x{Height}" : "Unknown";
}
