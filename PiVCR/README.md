# PiVCR - Raspberry Pi Video Control & Recording

A .NET console application designed to play video files on Raspberry Pi with hardware acceleration support and RFID scanner integration.

## Features

-   Cross-platform video playback (optimized for Raspberry Pi)
-   Hardware acceleration support for Raspberry Pi
-   **RFID scanner integration for automatic video selection**
-   Support for multiple video formats (MP4, AVI, MKV, MOV, WMV, FLV, WebM, M4V, 3GP)
-   Interactive mode for browsing and selecting videos
-   Video information analysis
-   Command-line argument support
-   Directory listing of video files
-   **RFID tag to video mapping configuration**

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
    "PiVCR": {
        "DefaultVideoPath": "/home/pi/Videos",
        "RaspberryPi": {
            "UseHardwareAcceleration": true,
            "PreferOMXPlayer": true,
            "FFMpegPath": "/usr/bin/ffmpeg"
        },
        "RFID": {
            "Enabled": true,
            "Type": "RC522_SPI",
            "SPI": {
                "BusId": 0,
                "ChipSelectLine": 0,
                "ResetPin": 25
            },
            "Serial": {
                "PortNames": [
                    "/dev/ttyUSB0",
                    "/dev/ttyUSB1",
                    "/dev/ttyACM0",
                    "/dev/ttyACM1",
                    "/dev/serial0"
                ],
                "BaudRate": 9600
            },
            "ConfigFile": "rfid-config.json"
        }
    }
}
```

## RC522 RFID Module Setup

The PiVCR now supports the **RC522 RFID module** over SPI for reading RFID/NFC cards. This is the recommended method for RFID integration.

### Hardware Connection (SPI)

Connect your RC522 module to the Raspberry Pi 3B as follows:

| RC522 Pin    | Raspberry Pi Pin                    | GPIO           | Description |
| ------------ | ----------------------------------- | -------------- | ----------- |
| **SDA (SS)** | Pin 24                              | GPIO 8 (CE0)   | Chip Select |
| **SCK**      | Pin 23                              | GPIO 11 (SCLK) | SPI Clock   |
| **MOSI**     | Pin 19                              | GPIO 10 (MOSI) | Master Out  |
| **MISO**     | Pin 21                              | GPIO 9 (MISO)  | Master In   |
| **RST**      | Pin 22                              | GPIO 25        | Reset       |
| **GND**      | Pin 6, 9, 14, 20, 25, 30, 34, or 39 | GND            | Ground      |
| **3.3V**     | Pin 1 or 17                         | 3.3V           | Power       |

#### Visual Pin Layout

```
Raspberry Pi 3B GPIO Header (top view):

3.3V ─────┐   ┌───── 5V
    [ 1] [ 2]
          [ 3] [ 4]  5V
          [ 5] [ 6] ─── GND (RC522)
          [ 7] [ 8]
    GND  [ 9] [10]
         [11] [12]
         [13] [14]
         [15] [16]
3.3V ────[17] [18]
MOSI ────[19] [20]  GND
MISO ────[21] [22] ─── RST (RC522)
SCK  ────[23] [24] ─── SDA/SS (RC522)
         [25] [26]
```

⚠️ **IMPORTANT**:

-   RC522 operates at **3.3V only** - do NOT connect to 5V or you will damage the module!
-   Ensure your RC522 module is specifically designed for 3.3V (most are)

### Enable SPI on Raspberry Pi

1. **Enable SPI interface:**

```bash
sudo raspi-config
# Navigate to: Interface Options → SPI → Yes
# Select: Finish → Reboot
```

2. **Verify SPI is enabled after reboot:**

```bash
# Check if SPI modules are loaded
lsmod | grep spi
# Should show: spi_bcm2835 or similar

# Check SPI devices exist
ls -l /dev/spi*
# Should show: /dev/spidev0.0 and /dev/spidev0.1
```

### Software Configuration

The RC522 support is already included in PiVCR. The configuration in `appsettings.json` should look like this:

```json
{
    "PiVCR": {
        "RFID": {
            "Enabled": true,
            "Type": "RC522_SPI",
            "SPI": {
                "BusId": 0,
                "ChipSelectLine": 0,
                "ResetPin": 25
            },
            "ConfigFile": "rfid-config.json"
        }
    }
}
```

### Alternative: Serial RFID Scanner

If you prefer using a USB/Serial RFID scanner instead, update the configuration:

```json
{
    "PiVCR": {
        "RFID": {
            "Enabled": true,
            "Type": "Serial",
            "Serial": {
                "PortNames": [
                    "/dev/ttyUSB0",
                    "/dev/ttyUSB1",
                    "/dev/ttyACM0",
                    "/dev/ttyACM1",
                    "/dev/serial0"
                ],
                "BaudRate": 9600
            },
            "ConfigFile": "rfid-config.json"
        }
    }
}
```

### Testing Your RC522 Module

1. **Run PiVCR:**

```bash
cd PiVCR
dotnet run
```

2. **Look for initialization message:**

```
✓ RC522 RFID reader initialized successfully on SPI bus 0, CS 0, RST pin 25
✓ Starting RC522 RFID scanning...
```

3. **Place an RFID/NFC card near the reader**

You should see:

```
✓ RFID Tag detected: A1B2C3D4
```

If you see errors about SPI not being available, make sure you've enabled SPI and rebooted the Pi.

## RFID Scanner Integration

The application supports RFID scanner integration for automatic video playback. When an RFID tag is scanned, the application will automatically play the associated video file.

### RFID Hardware Setup

1. **Connect RFID Scanner to Raspberry Pi:**

    - USB RFID scanners: Connect via USB port
    - Serial RFID scanners: Connect to GPIO serial pins or USB-to-serial adapter

2. **Common RFID Scanner Types:**
    - **USB HID Scanners**: Act like keyboards, send RFID data as keystrokes
    - **Serial Scanners**: Send RFID data via serial communication (RS232/TTL)
    - **EM4100/EM4102 based scanners**: Most common format

### RFID Configuration

1. **RFID Tag Mapping:**
   The application uses `rfid-config.json` to map RFID tag IDs to video files:

    ```json
    {
        "0123456789": "/home/pi/Videos/movie1.mp4",
        "9876543210": "/home/pi/Videos/movie2.mp4",
        "1111111111": "/home/pi/Videos/cartoon.avi",
        "2222222222": "/home/pi/Videos/documentary.mkv"
    }
    ```

2. **Managing RFID Mappings:**
    - Use interactive mode option 4 to configure RFID settings
    - Add new tag-to-video mappings
    - Remove existing mappings
    - Test RFID scanner connectivity

### RFID Usage

1. **Automatic Mode:**

    - Start the application
    - RFID scanner will be automatically detected and initialized
    - Scan any configured RFID tag near the scanner
    - Associated video will start playing automatically

2. **Interactive Configuration:**

    ```bash
    dotnet run
    # Select option 4: RFID Scanner settings
    # Select option 2: Add new RFID mapping
    # Scan your RFID tag when prompted
    # Enter the path to your video file
    ```

3. **Testing RFID Scanner:**
    ```bash
    dotnet run
    # Select option 4: RFID Scanner settings
    # Select option 4: Test RFID scanner
    # Scan RFID tags to see if they're detected
    ```

### Supported RFID Scanners

-   **USB RFID Scanners**: Most USB HID-compatible scanners
-   **Serial RFID Scanners**: Connected to `/dev/ttyUSB0`, `/dev/ttyUSB1`, `/dev/ttyACM0`, `/dev/ttyACM1`
-   **Common formats**: EM4100, EM4102, Mifare (depends on scanner firmware)

### RFID Troubleshooting

**Scanner Not Detected:**

```bash
# Check USB devices
lsusb

# Check serial devices
ls /dev/tty*

# Check permissions
sudo chmod 666 /dev/ttyUSB0  # Replace with your device
```

**Permission Issues:**

```bash
# Add user to dialout group for serial access
sudo usermod -a -G dialout $USER
# Logout and login again
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
PiVCR/
├── Program.cs              # Main application logic
├── PiVCR.csproj           # Project file
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
