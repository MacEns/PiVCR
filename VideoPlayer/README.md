# Raspberry Pi Video Player

A .NET console application designed to play video files on Raspberry Pi with hardware acceleration support.

## Features

-   Cross-platform video playback (optimized for Raspberry Pi)
-   Hardware acceleration support for Raspberry Pi
-   Support for multiple video formats (MP4, AVI, MKV, MOV, WMV, FLV, WebM, M4V, 3GP)
-   Interactive mode for browsing and selecting videos
-   Video information analysis
-   Command-line argument support
-   Directory listing of video files

## Prerequisites

### For Raspberry Pi

1. **Install FFMpeg and related tools:**

    ```bash
    sudo apt update
    sudo apt install ffmpeg
    ```

2. **Optional: Install OMXPlayer (for older Raspberry Pi models):**

    ```bash
    sudo apt install omxplayer
    ```

3. **Install .NET Runtime:**

    ```bash
    # For Raspberry Pi OS (32-bit)
    curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel Current --runtime dotnet

    # For Raspberry Pi OS (64-bit)
    sudo apt-get update && sudo apt-get install -y dotnet-runtime-9.0
    ```

### For Development/Other Platforms

1. Install .NET 8.0 or later
2. Install FFMpeg and ensure it's in your system PATH

## Building the Application

```bash
# Clone or navigate to the project directory
cd VideoPlayer

# Restore dependencies
dotnet restore

# Build the application
dotnet build

# Publish for Raspberry Pi (ARM64)
dotnet publish -c Release -r linux-arm64 --self-contained false

# Publish for Raspberry Pi (ARM32)
dotnet publish -c Release -r linux-arm --self-contained false
```

## Usage

### Interactive Mode

```bash
dotnet run
```

### Command Line Mode

```bash
# Play a specific video file
dotnet run "/path/to/video.mp4"

# Examples
dotnet run "/home/pi/Videos/movie.mp4"
dotnet run "sample.avi"
```

### Published Application

```bash
# After publishing, run the executable directly
./VideoPlayer
./VideoPlayer "/path/to/video.mp4"
```

## Configuration

The application uses `appsettings.json` for configuration:

```json
{
    "VideoPlayer": {
        "DefaultVideoPath": "/home/pi/Videos",
        "RaspberryPi": {
            "UseHardwareAcceleration": true,
            "PreferOMXPlayer": true,
            "FFMpegPath": "/usr/bin/ffmpeg"
        }
    }
}
```

## Video Playback on Raspberry Pi

The application automatically detects when running on Raspberry Pi and uses the most appropriate playback method:

1. **OMXPlayer**: Hardware-accelerated player for older Raspberry Pi models
2. **FFPlay with hardware acceleration**: For newer models
3. **FFMpeg processing**: Fallback option for analysis and conversion

## Supported Video Formats

-   MP4 (H.264, H.265)
-   AVI
-   MKV (Matroska)
-   MOV (QuickTime)
-   WMV (Windows Media Video)
-   FLV (Flash Video)
-   WebM
-   M4V
-   3GP

## Performance Tips for Raspberry Pi

1. **Use H.264 encoded videos** for best performance
2. **Keep video resolution reasonable** (1080p or lower recommended)
3. **Store videos on fast storage** (USB 3.0 drive or fast SD card)
4. **Ensure adequate power supply** for your Raspberry Pi model
5. **Close unnecessary applications** while playing videos

## Troubleshooting

### FFMpeg Not Found

```bash
# Verify FFMpeg installation
which ffmpeg
ffmpeg -version

# If not installed
sudo apt install ffmpeg
```

### Permission Issues

```bash
# Make sure the video files are readable
chmod +r /path/to/video.mp4

# For the application executable
chmod +x VideoPlayer
```

### Memory Issues

-   Use lower resolution videos
-   Increase GPU memory split: `sudo raspi-config` > Advanced Options > Memory Split > 128

### Audio Issues

```bash
# Configure audio output
sudo raspi-config
# Navigate to System Options > Audio

# Test audio
speaker-test -t sine -f 1000 -c 2 -s 2
```

## Development

### Project Structure

```
VideoPlayer/
├── Program.cs              # Main application logic
├── VideoPlayer.csproj      # Project file
├── appsettings.json        # Configuration file
└── README.md              # This file
```

### Key Dependencies

-   **FFMpegCore**: Video processing and analysis
-   **System.Text.Json**: Configuration handling

### Building for Different Architectures

```bash
# For Raspberry Pi 4 (ARM64)
dotnet publish -r linux-arm64

# For Raspberry Pi 3 and older (ARM32)
dotnet publish -r linux-arm

# For development (current platform)
dotnet publish -r linux-x64  # Linux
dotnet publish -r win-x64     # Windows
dotnet publish -r osx-x64     # macOS
```

## Contributing

Feel free to submit issues and enhancement requests!

## License

This project is open source and available under the MIT License.
