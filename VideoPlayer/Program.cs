using PiVCR;
using PiVCR.Models;

class Program
{
    private static RFIDScanner? _rfidScanner;

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Raspberry Pi Video Player ===");
        Console.WriteLine();

        // Configure FFMpeg binary path for Raspberry Pi
        VideoManager.ConfigureFFMpeg();

        // Initialize RFID scanner
        _rfidScanner = new RFIDScanner();
        _rfidScanner.TagDetected += OnRFIDTagDetected;
        await _rfidScanner.InitializeAsync();

        if (args.Length > 0)
        {
            // If file path is provided as argument
            string videoPath = args[0];
            await VideoManager.PlayVideoAsync(videoPath);
        }
        else
        {
            // Interactive mode
            await InteractiveMode();
        }

        // Cleanup RFID scanner on exit
        _rfidScanner?.Dispose();
    }

    private static async void OnRFIDTagDetected(object? sender, RFIDTagEventArgs e)
    {
        if (_rfidScanner != null)
        {
            var videoPath = _rfidScanner.GetVideoPath(e.TagId);
            if (videoPath != null)
            {
                Console.WriteLine($"Playing associated video: {videoPath}");
                await VideoManager.PlayVideoAsync(videoPath);
            }
            else
            {
                Console.WriteLine("No video associated with this RFID tag.");
                Console.WriteLine("Use option 4 to configure RFID mappings.");
            }
        }
    }

    private static async Task InteractiveMode()
    {
        while (true)
        {
            Console.WriteLine("\nOptions:");
            Console.WriteLine("1. Play video file");
            Console.WriteLine("2. List videos in directory");
            Console.WriteLine("3. Get video information");
            Console.WriteLine("4. RFID Scanner settings");
            Console.WriteLine("5. Wait for RFID scan");
            Console.WriteLine("6. Video Library");
            Console.WriteLine("7. Exit");
            Console.Write("\nSelect option (1-7): ");

            var choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1":
                    await HandlePlayVideo();
                    break;
                case "2":
                    HandleListVideos();
                    break;
                case "3":
                    await HandleVideoInfo();
                    break;
                case "4":
                    await HandleRFIDSettings();
                    break;
                case "5":
                    await HandleRFIDScan();
                    break;
                case "6":
                    await HandleVideoLibrary();
                    break;
                case "7":
                    Console.WriteLine("Goodbye!");
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    private static async Task HandlePlayVideo()
    {
        Console.Write("Enter video file path: ");
        var videoPath = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(videoPath))
        {
            await VideoManager.PlayVideoAsync(videoPath);
        }
    }

    private static void HandleListVideos()
    {
        Console.Write("Enter directory path (or press Enter for current directory): ");
        var dirPath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(dirPath))
        {
            dirPath = Directory.GetCurrentDirectory();
        }

        VideoManager.ListVideosInDirectory(dirPath);
    }

    private static async Task HandleVideoInfo()
    {
        Console.Write("Enter video file path: ");
        var videoPath = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(videoPath))
        {
            await VideoManager.ShowVideoInfoAsync(videoPath);
        }
    }

    private static async Task HandleRFIDSettings()
    {
        if (_rfidScanner == null)
        {
            Console.WriteLine("RFID scanner is not available.");
            return;
        }

        while (true)
        {
            Console.WriteLine("\nRFID Settings:");
            Console.WriteLine("1. View current mappings");
            Console.WriteLine("2. Add new RFID mapping");
            Console.WriteLine("3. Remove RFID mapping");
            Console.WriteLine("4. Test RFID scanner");
            Console.WriteLine("5. Back to main menu");
            Console.Write("\nSelect option (1-5): ");

            var choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1":
                    _rfidScanner.ShowMappings();
                    break;
                case "2":
                    await AddRFIDMapping();
                    break;
                case "3":
                    await RemoveRFIDMapping();
                    break;
                case "4":
                    await TestRFIDScanner();
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    private static async Task AddRFIDMapping()
    {
        if (_rfidScanner == null) return;

        Console.Write("Enter RFID tag value: ");
        var rfidTag = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(rfidTag))
        {
            Console.WriteLine("Invalid RFID tag value.");
            return;
        }

        Console.Write("Enter video file path: ");
        var videoPath = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(videoPath))
        {
            Console.WriteLine("Invalid video path.");
            return;
        }

        if (!File.Exists(videoPath))
        {
            Console.WriteLine("Warning: Video file does not exist at specified path.");
        }

        try
        {
            await _rfidScanner.AddMappingAsync(rfidTag, videoPath);
            Console.WriteLine("RFID mapping added successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding RFID mapping: {ex.Message}");
        }
    }

    private static async Task RemoveRFIDMapping()
    {
        if (_rfidScanner == null) return;

        if (_rfidScanner.MappingCount == 0)
        {
            Console.WriteLine("No RFID mappings to remove.");
            return;
        }

        _rfidScanner.ShowMappings();
        Console.Write("\nEnter RFID tag to remove: ");
        var rfidTag = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(rfidTag))
        {
            Console.WriteLine("Invalid RFID tag value.");
            return;
        }

        try
        {
            var removed = await _rfidScanner.RemoveMappingAsync(rfidTag);
            if (removed)
            {
                Console.WriteLine("RFID mapping removed successfully.");
            }
            else
            {
                Console.WriteLine("RFID tag not found in mappings.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing RFID mapping: {ex.Message}");
        }
    }

    private static async Task TestRFIDScanner()
    {
        if (_rfidScanner == null)
        {
            Console.WriteLine("RFID scanner is not available.");
            return;
        }

        Console.WriteLine("RFID scanner test mode. Scan an RFID tag or press Enter to stop...");
        _rfidScanner.ShowScannerStatus();

        // Wait for user input to stop test
        await Task.Run(() => Console.ReadLine());
        Console.WriteLine("Test mode ended.");
    }

    private static async Task HandleRFIDScan()
    {
        if (_rfidScanner == null || !_rfidScanner.IsEnabled)
        {
            Console.WriteLine("RFID scanner is not enabled. Please check your scanner connection.");
            return;
        }

        Console.WriteLine("Waiting for RFID scan... (Press Enter to stop)");
        Console.WriteLine("Scan an RFID tag near the scanner.");

        // Wait for user input to stop scanning
        await Task.Run(() => Console.ReadLine());
        Console.WriteLine("RFID scan mode ended.");
    }

    private static async Task HandleVideoLibrary()
    {
        while (true)
        {
            Console.WriteLine("\nVideo Library:");
            Console.WriteLine("1. Browse all videos");
            Console.WriteLine("2. Search videos");
            Console.WriteLine("3. Browse by genre");
            Console.WriteLine("4. Recently played");
            Console.WriteLine("5. Most played");
            Console.WriteLine("6. Favorites");
            Console.WriteLine("7. Add video to library");
            Console.WriteLine("8. Scan directory for videos");
            Console.WriteLine("9. Back to main menu");
            Console.Write("\nSelect option (1-9): ");

            var choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1":
                    await BrowseAllVideos();
                    break;
                case "2":
                    await SearchVideos();
                    break;
                case "3":
                    await BrowseByGenre();
                    break;
                case "4":
                    await ShowRecentlyPlayed();
                    break;
                case "5":
                    await ShowMostPlayed();
                    break;
                case "6":
                    await ShowFavorites();
                    break;
                case "7":
                    await AddVideoToLibrary();
                    break;
                case "8":
                    await ScanDirectoryForVideos();
                    break;
                case "9":
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    private static async Task BrowseAllVideos()
    {
        try
        {
            var videos = await VideoManager.VideoFileService.GetAllVideoFilesAsync();
            await DisplayVideoListAndPlay(videos, "All Videos");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error browsing videos: {ex.Message}");
        }
    }

    private static async Task SearchVideos()
    {
        Console.Write("Enter search term: ");
        var searchTerm = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            Console.WriteLine("Search term cannot be empty.");
            return;
        }

        try
        {
            var videos = await VideoManager.VideoFileService.SearchVideoFilesAsync(searchTerm);
            await DisplayVideoListAndPlay(videos, $"Search results for '{searchTerm}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching videos: {ex.Message}");
        }
    }

    private static async Task BrowseByGenre()
    {
        try
        {
            var genres = await VideoManager.VideoFileService.GetGenresAsync();
            var genreList = genres.ToList();

            if (!genreList.Any())
            {
                Console.WriteLine("No genres found in the library.");
                return;
            }

            Console.WriteLine("\nGenres:");
            for (int i = 0; i < genreList.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {genreList[i]}");
            }

            Console.Write($"\nSelect genre (1-{genreList.Count}): ");
            if (int.TryParse(Console.ReadLine(), out var genreIndex) &&
                genreIndex >= 1 && genreIndex <= genreList.Count)
            {
                var selectedGenre = genreList[genreIndex - 1];
                var videos = await VideoManager.VideoFileService.GetVideoFilesByGenreAsync(selectedGenre);
                await DisplayVideoListAndPlay(videos, $"Videos in genre '{selectedGenre}'");
            }
            else
            {
                Console.WriteLine("Invalid genre selection.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error browsing by genre: {ex.Message}");
        }
    }

    private static async Task ShowRecentlyPlayed()
    {
        try
        {
            var videos = await VideoManager.VideoFileService.GetRecentlyPlayedAsync();
            await DisplayVideoListAndPlay(videos, "Recently Played Videos");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing recently played: {ex.Message}");
        }
    }

    private static async Task ShowMostPlayed()
    {
        try
        {
            var videos = await VideoManager.VideoFileService.GetMostPlayedAsync();
            await DisplayVideoListAndPlay(videos, "Most Played Videos");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing most played: {ex.Message}");
        }
    }

    private static async Task ShowFavorites()
    {
        try
        {
            var videos = await VideoManager.VideoFileService.GetFavoriteVideoFilesAsync();
            await DisplayVideoListAndPlay(videos, "Favorite Videos");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing favorites: {ex.Message}");
        }
    }

    private static async Task DisplayVideoListAndPlay(IEnumerable<PiVCR.Models.VideoFile> videos, string title)
    {
        var videoList = videos.ToList();

        if (!videoList.Any())
        {
            Console.WriteLine($"No videos found for '{title}'.");
            return;
        }

        Console.WriteLine($"\n{title}:");
        Console.WriteLine(new string('-', 80));

        for (int i = 0; i < videoList.Count; i++)
        {
            var video = videoList[i];
            var durationStr = video.Duration?.ToString(@"hh\:mm\:ss") ?? "Unknown";
            var playCountStr = video.PlayCount > 0 ? $"({video.PlayCount} plays)" : "";
            var favoriteStr = video.IsFavorite ? "⭐" : "";

            Console.WriteLine($"{i + 1,3}. {video.DisplayName,-40} {durationStr,-10} {video.FileSizeDisplay,-10} {favoriteStr} {playCountStr}");
        }

        Console.WriteLine($"\nTotal: {videoList.Count} video(s)");
        Console.Write($"Select video to play (1-{videoList.Count}), or 0 to go back: ");

        if (int.TryParse(Console.ReadLine(), out var choice))
        {
            if (choice == 0)
            {
                return;
            }

            if (choice >= 1 && choice <= videoList.Count)
            {
                var selectedVideo = videoList[choice - 1];
                await VideoManager.PlayVideoAsync(selectedVideo.FilePath);
            }
            else
            {
                Console.WriteLine("Invalid selection.");
            }
        }
    }

    private static async Task AddVideoToLibrary()
    {
        Console.Write("Enter video file path: ");
        var filePath = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            Console.WriteLine("File path cannot be empty.");
            return;
        }

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        Console.Write("Enter title (optional, press Enter to use filename): ");
        var title = Console.ReadLine()?.Trim();

        Console.Write("Enter description (optional): ");
        var description = Console.ReadLine()?.Trim();

        Console.Write("Enter genre (optional): ");
        var genre = Console.ReadLine()?.Trim();

        try
        {
            var videoFile = await VideoManager.VideoFileService.AddVideoFileAsync(
                filePath,
                string.IsNullOrWhiteSpace(title) ? null : title,
                string.IsNullOrWhiteSpace(description) ? null : description,
                string.IsNullOrWhiteSpace(genre) ? null : genre
            );

            Console.WriteLine($"Video '{videoFile.DisplayName}' added to library successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding video to library: {ex.Message}");
        }
    }

    private static async Task ScanDirectoryForVideos()
    {
        Console.Write("Enter directory path to scan: ");
        var dirPath = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(dirPath))
        {
            Console.WriteLine("Directory path cannot be empty.");
            return;
        }

        if (!Directory.Exists(dirPath))
        {
            Console.WriteLine($"Directory not found: {dirPath}");
            return;
        }

        Console.Write("Scan subdirectories recursively? (y/N): ");
        var recursive = Console.ReadLine()?.Trim().ToLowerInvariant() == "y";

        try
        {
            Console.WriteLine($"Scanning {dirPath}...");
            var addedCount = await VideoManager.VideoFileService.ScanDirectoryAsync(dirPath, recursive);
            Console.WriteLine($"Scan completed. {addedCount} new video(s) added to library.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scanning directory: {ex.Message}");
        }
    }
}
