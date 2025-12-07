using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VideoPlayer;

class Program
{
    private static readonly string[] SupportedVideoExtensions =
    {
        ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".m4v", ".3gp"
    };

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Raspberry Pi Video Player ===");
        Console.WriteLine();

        // Configure FFMpeg binary path for Raspberry Pi
        ConfigureFFMpeg();

        if (args.Length > 0)
        {
            // If file path is provided as argument
            string videoPath = args[0];
            await PlayVideo(videoPath);
        }
        else
        {
            // Interactive mode
            await InteractiveMode();
        }
    }

    private static void ConfigureFFMpeg()
    {
        // For Raspberry Pi, FFMpeg is typically installed via apt
        // Set the path where FFMpeg binaries are located
        if (RuntimeInformation.OSArchitecture == Architecture.Arm ||
            RuntimeInformation.OSArchitecture == Architecture.Arm64)
        {
            // Raspberry Pi paths
            GlobalFFOptions.Configure(new FFOptions
            {
                BinaryFolder = "/usr/bin/", // Default location for apt-installed FFMpeg
                TemporaryFilesFolder = "/tmp/"
            });
        }
        else
        {
            // Development/other platforms
            // FFMpeg should be in PATH or specify custom path
            try
            {
                // Try to find ffmpeg in PATH
                var ffmpegPath = GetFFMpegPath();
                if (!string.IsNullOrEmpty(ffmpegPath))
                {
                    GlobalFFOptions.Configure(new FFOptions
                    {
                        BinaryFolder = Path.GetDirectoryName(ffmpegPath),
                        TemporaryFilesFolder = Path.GetTempPath()
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not configure FFMpeg path: {ex.Message}");
                Console.WriteLine("Please ensure FFMpeg is installed and available in PATH");
            }
        }
    }

    private static string GetFFMpegPath()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "where" : "which",
                    Arguments = "ffmpeg",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return process.ExitCode == 0 ? result.Trim().Split('\n')[0] : string.Empty;
        }
        catch
        {
            return string.Empty;
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
            Console.WriteLine("4. Exit");
            Console.Write("\nSelect option (1-4): ");

            string choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1":
                    await HandlePlayVideo();
                    break;
                case "2":
                    await HandleListVideos();
                    break;
                case "3":
                    await HandleVideoInfo();
                    break;
                case "4":
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
        string? videoPath = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(videoPath))
        {
            await PlayVideo(videoPath);
        }
    }

    private static async Task HandleListVideos()
    {
        Console.Write("Enter directory path (or press Enter for current directory): ");
        string? dirPath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(dirPath))
        {
            dirPath = Directory.GetCurrentDirectory();
        }

        ListVideosInDirectory(dirPath);
    }

    private static async Task HandleVideoInfo()
    {
        Console.Write("Enter video file path: ");
        string? videoPath = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(videoPath))
        {
            await ShowVideoInfo(videoPath);
        }
    }

    private static async Task PlayVideo(string videoPath)
    {
        try
        {
            if (!File.Exists(videoPath))
            {
                Console.WriteLine($"Error: File '{videoPath}' not found.");
                return;
            }

            if (!IsVideoFile(videoPath))
            {
                Console.WriteLine($"Error: '{videoPath}' does not appear to be a supported video file.");
                Console.WriteLine($"Supported extensions: {string.Join(", ", SupportedVideoExtensions)}");
                return;
            }

            Console.WriteLine($"Playing video: {Path.GetFileName(videoPath)}");
            Console.WriteLine("Press 'q' to stop playback or Ctrl+C to exit.");

            // For Raspberry Pi, use hardware acceleration if available
            if (RuntimeInformation.OSArchitecture == Architecture.Arm ||
                RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                // Raspberry Pi optimizations
                await PlayVideoOnRaspberryPi(videoPath);
            }
            else
            {
                // Standard playback for development/testing
                await PlayVideoStandard(videoPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error playing video: {ex.Message}");
        }
    }

    private static async Task PlayVideoOnRaspberryPi(string videoPath)
    {
        try
        {
            // Use omxplayer if available (legacy), or try hardware-accelerated ffplay
            if (File.Exists("/usr/bin/omxplayer"))
            {
                Console.WriteLine("Using OMXPlayer for hardware acceleration...");
                var omxProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/usr/bin/omxplayer",
                        Arguments = $"\"{videoPath}\"",
                        UseShellExecute = false
                    }
                };

                omxProcess.Start();
                await omxProcess.WaitForExitAsync();
            }
            else if (File.Exists("/usr/bin/ffplay"))
            {
                Console.WriteLine("Using FFPlay with hardware acceleration...");
                var ffplayProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/usr/bin/ffplay",
                        Arguments = $"-hwaccel auto -i \"{videoPath}\"",
                        UseShellExecute = false
                    }
                };

                ffplayProcess.Start();
                await ffplayProcess.WaitForExitAsync();
            }
            else
            {
                // Fallback to standard FFMpeg conversion/streaming
                Console.WriteLine("Using FFMpeg for playback...");
                await ConvertAndDisplay(videoPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during Raspberry Pi playback: {ex.Message}");
            Console.WriteLine("Falling back to standard playback...");
            await PlayVideoStandard(videoPath);
        }
    }

    private static async Task PlayVideoStandard(string videoPath)
    {
        try
        {
            // Try to use system's default video player
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = videoPath,
                    UseShellExecute = true
                }
            };

            process.Start();
            Console.WriteLine("Video opened in system default player.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not open with system player: {ex.Message}");
            await ConvertAndDisplay(videoPath);
        }
    }

    private static async Task ConvertAndDisplay(string videoPath)
    {
        try
        {
            Console.WriteLine("Processing video with FFMpeg...");

            var mediaInfo = await FFProbe.AnalyseAsync(videoPath);
            Console.WriteLine($"Duration: {mediaInfo.Duration}");
            Console.WriteLine($"Video Codec: {mediaInfo.PrimaryVideoStream?.CodecName}");
            Console.WriteLine($"Audio Codec: {mediaInfo.PrimaryAudioStream?.CodecName}");

            Console.WriteLine("Note: For full video playback, use a video player application.");
            Console.WriteLine("This application can process and analyze video files.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing video: {ex.Message}");
        }
    }

    private static async Task ShowVideoInfo(string videoPath)
    {
        try
        {
            if (!File.Exists(videoPath))
            {
                Console.WriteLine($"Error: File '{videoPath}' not found.");
                return;
            }

            Console.WriteLine($"Analyzing video: {Path.GetFileName(videoPath)}");

            var mediaInfo = await FFProbe.AnalyseAsync(videoPath);

            Console.WriteLine("\n=== Video Information ===");
            Console.WriteLine($"File: {Path.GetFileName(videoPath)}");
            Console.WriteLine($"Duration: {mediaInfo.Duration}");
            Console.WriteLine($"Size: {new FileInfo(videoPath).Length / (1024 * 1024)} MB");

            if (mediaInfo.PrimaryVideoStream != null)
            {
                var video = mediaInfo.PrimaryVideoStream;
                Console.WriteLine($"\nVideo Stream:");
                Console.WriteLine($"  Codec: {video.CodecName}");
                Console.WriteLine($"  Resolution: {video.Width}x{video.Height}");
                Console.WriteLine($"  Frame Rate: {video.FrameRate:F2} fps");
                Console.WriteLine($"  Bit Rate: {video.BitRate / 1000} kbps");
                Console.WriteLine($"  Pixel Format: {video.PixelFormat}");
            }

            if (mediaInfo.PrimaryAudioStream != null)
            {
                var audio = mediaInfo.PrimaryAudioStream;
                Console.WriteLine($"\nAudio Stream:");
                Console.WriteLine($"  Codec: {audio.CodecName}");
                Console.WriteLine($"  Sample Rate: {audio.SampleRateHz} Hz");
                Console.WriteLine($"  Channels: {audio.Channels}");
                Console.WriteLine($"  Bit Rate: {audio.BitRate / 1000} kbps");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error analyzing video: {ex.Message}");
        }
    }

    private static void ListVideosInDirectory(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Error: Directory '{directoryPath}' not found.");
                return;
            }

            Console.WriteLine($"\nVideo files in '{directoryPath}':");
            Console.WriteLine(new string('-', 50));

            var videoFiles = Directory.GetFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => IsVideoFile(file))
                .ToList();

            if (videoFiles.Any())
            {
                for (int i = 0; i < videoFiles.Count; i++)
                {
                    var fileInfo = new FileInfo(videoFiles[i]);
                    Console.WriteLine($"{i + 1,3}. {fileInfo.Name,-30} ({fileInfo.Length / (1024 * 1024)} MB)");
                }

                Console.WriteLine($"\nFound {videoFiles.Count} video file(s)");
            }
            else
            {
                Console.WriteLine("No video files found in the specified directory.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing directory: {ex.Message}");
        }
    }

    private static bool IsVideoFile(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        return SupportedVideoExtensions.Contains(extension);
    }
}
