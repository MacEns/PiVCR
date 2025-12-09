using FFMpegCore;
using Microsoft.EntityFrameworkCore;
using VideoPlayer.Data;
using VideoPlayer.Models;

namespace VideoPlayer.Services;

public class VideoFileService
{
    private readonly VideoPlayerDbContext _context;

    public VideoFileService()
    {
        _context = new VideoPlayerDbContext();
        InitializeDatabase();
    }

    public VideoFileService(VideoPlayerDbContext context)
    {
        _context = context;
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        _context.Database.EnsureCreated();
    }

    public async Task<VideoFile> AddVideoFileAsync(string filePath, string? title = null, string? description = null, string? genre = null)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Video file not found: {filePath}");
        }

        // Check if file already exists in database
        var existingFile = await _context.VideoFiles.FirstOrDefaultAsync(v => v.FilePath == filePath);
        if (existingFile != null)
        {
            return existingFile;
        }

        var videoFile = new VideoFile
        {
            Title = title ?? Path.GetFileNameWithoutExtension(filePath),
            FilePath = filePath,
            Description = description,
            Genre = genre,
            FileSizeBytes = new FileInfo(filePath).Length,
            DateAdded = DateTime.UtcNow
        };

        // Analyze video file to get metadata
        try
        {
            var mediaInfo = await FFProbe.AnalyseAsync(filePath);
            videoFile.Duration = mediaInfo.Duration;

            if (mediaInfo.PrimaryVideoStream != null)
            {
                var video = mediaInfo.PrimaryVideoStream;
                videoFile.VideoCodec = video.CodecName;
                videoFile.Width = video.Width;
                videoFile.Height = video.Height;
                videoFile.FrameRate = video.FrameRate;
                videoFile.BitRate = video.BitRate;
            }

            if (mediaInfo.PrimaryAudioStream != null)
            {
                videoFile.AudioCodec = mediaInfo.PrimaryAudioStream.CodecName;
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the operation
            Console.WriteLine($"Warning: Could not analyze video file {filePath}: {ex.Message}");
        }

        _context.VideoFiles.Add(videoFile);
        await _context.SaveChangesAsync();

        return videoFile;
    }

    public async Task<IEnumerable<VideoFile>> GetAllVideoFilesAsync()
    {
        return await _context.VideoFiles.OrderBy(v => v.Title).ToListAsync();
    }

    public async Task<IEnumerable<VideoFile>> SearchVideoFilesAsync(string searchTerm)
    {
        return await _context.VideoFiles
            .Where(v => v.Title.Contains(searchTerm) ||
                       v.Description!.Contains(searchTerm) ||
                       v.Genre!.Contains(searchTerm))
            .OrderBy(v => v.Title)
            .ToListAsync();
    }

    public async Task<IEnumerable<VideoFile>> GetVideoFilesByGenreAsync(string genre)
    {
        return await _context.VideoFiles
            .Where(v => v.Genre == genre)
            .OrderBy(v => v.Title)
            .ToListAsync();
    }

    public async Task<IEnumerable<VideoFile>> GetFavoriteVideoFilesAsync()
    {
        return await _context.VideoFiles
            .Where(v => v.IsFavorite)
            .OrderBy(v => v.Title)
            .ToListAsync();
    }

    public async Task<IEnumerable<VideoFile>> GetRecentlyPlayedAsync(int count = 10)
    {
        return await _context.VideoFiles
            .Where(v => v.LastPlayed.HasValue)
            .OrderByDescending(v => v.LastPlayed)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<VideoFile>> GetMostPlayedAsync(int count = 10)
    {
        return await _context.VideoFiles
            .Where(v => v.PlayCount > 0)
            .OrderByDescending(v => v.PlayCount)
            .ThenByDescending(v => v.LastPlayed)
            .Take(count)
            .ToListAsync();
    }

    public async Task<VideoFile?> GetVideoFileByIdAsync(int id)
    {
        return await _context.VideoFiles.FindAsync(id);
    }

    public async Task<VideoFile?> GetVideoFileByPathAsync(string filePath)
    {
        return await _context.VideoFiles.FirstOrDefaultAsync(v => v.FilePath == filePath);
    }

    public async Task<VideoFile> UpdateVideoFileAsync(VideoFile videoFile)
    {
        _context.VideoFiles.Update(videoFile);
        await _context.SaveChangesAsync();
        return videoFile;
    }

    public async Task<bool> DeleteVideoFileAsync(int id)
    {
        var videoFile = await _context.VideoFiles.FindAsync(id);
        if (videoFile == null)
        {
            return false;
        }

        _context.VideoFiles.Remove(videoFile);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task RecordPlaybackAsync(int videoFileId)
    {
        var videoFile = await _context.VideoFiles.FindAsync(videoFileId);
        if (videoFile != null)
        {
            videoFile.LastPlayed = DateTime.UtcNow;
            videoFile.PlayCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RecordPlaybackAsync(string filePath)
    {
        var videoFile = await _context.VideoFiles.FirstOrDefaultAsync(v => v.FilePath == filePath);
        if (videoFile != null)
        {
            videoFile.LastPlayed = DateTime.UtcNow;
            videoFile.PlayCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ToggleFavoriteAsync(int videoFileId)
    {
        var videoFile = await _context.VideoFiles.FindAsync(videoFileId);
        if (videoFile != null)
        {
            videoFile.IsFavorite = !videoFile.IsFavorite;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> ScanDirectoryAsync(string directoryPath, bool recursive = false)
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
        }

        var supportedExtensions = new[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".m4v", ".3gp" };
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var videoFiles = Directory.GetFiles(directoryPath, "*.*", searchOption)
            .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
            .ToList();

        int addedCount = 0;
        foreach (var filePath in videoFiles)
        {
            try
            {
                var existingFile = await _context.VideoFiles.FirstOrDefaultAsync(v => v.FilePath == filePath);
                if (existingFile == null)
                {
                    await AddVideoFileAsync(filePath);
                    addedCount++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding file {filePath}: {ex.Message}");
            }
        }

        return addedCount;
    }

    public async Task<IEnumerable<string>> GetGenresAsync()
    {
        return await _context.VideoFiles
            .Where(v => !string.IsNullOrEmpty(v.Genre))
            .Select(v => v.Genre!)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
