#!/bin/bash

# Raspberry Pi Video Player Setup Script
# This script sets up the video player application on a Raspberry Pi

echo "=== Raspberry Pi Video Player Setup ==="
echo ""

# Check if running on Raspberry Pi
if [[ ! -f /proc/device-tree/model ]] || [[ ! $(grep -i "raspberry" /proc/device-tree/model 2>/dev/null) ]]; then
    echo "Warning: This script is designed for Raspberry Pi. Continuing anyway..."
fi

# Update system
echo "Updating system packages..."
sudo apt update

# Install required packages
echo "Installing required packages..."
sudo apt install -y ffmpeg

# Optional: Install OMXPlayer for older Pi models
read -p "Install OMXPlayer for older Raspberry Pi models? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    sudo apt install -y omxplayer
fi

# Install .NET Runtime if not present
if ! command -v dotnet &> /dev/null; then
    echo "Installing .NET Runtime..."
    
    # Determine architecture
    ARCH=$(uname -m)
    if [[ "$ARCH" == "aarch64" ]]; then
        # ARM64
        sudo apt-get install -y dotnet-runtime-9.0
    elif [[ "$ARCH" == "armv7l" ]] || [[ "$ARCH" == "armv6l" ]]; then
        # ARM32
        curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel Current --runtime dotnet
        echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
        export PATH=$PATH:$HOME/.dotnet
    else
        echo "Unsupported architecture: $ARCH"
        exit 1
    fi
else
    echo ".NET Runtime already installed."
fi

# Create Videos directory if it doesn't exist
VIDEOS_DIR="/home/pi/Videos"
if [[ ! -d "$VIDEOS_DIR" ]]; then
    echo "Creating Videos directory at $VIDEOS_DIR..."
    mkdir -p "$VIDEOS_DIR"
fi

# Set up the application
echo "Setting up VideoPlayer application..."

# If the application files are present, make them executable
if [[ -f "./VideoPlayer" ]]; then
    chmod +x ./VideoPlayer
    echo "VideoPlayer executable permissions set."
fi

# Copy appsettings.json to user directory if it doesn't exist
if [[ -f "./appsettings.json" ]] && [[ ! -f "/home/pi/appsettings.json" ]]; then
    cp ./appsettings.json /home/pi/
    echo "Configuration file copied to /home/pi/"
fi

# Test FFMpeg installation
echo ""
echo "Testing FFMpeg installation..."
if command -v ffmpeg &> /dev/null; then
    ffmpeg -version | head -1
    echo "✓ FFMpeg is working"
else
    echo "✗ FFMpeg not found"
fi

# Test .NET installation
echo ""
echo "Testing .NET installation..."
if command -v dotnet &> /dev/null; then
    dotnet --version
    echo "✓ .NET is working"
else
    echo "✗ .NET not found"
fi

# GPU Memory split recommendation
echo ""
echo "=== Recommendations ==="
echo "1. For better video performance, consider increasing GPU memory split:"
echo "   sudo raspi-config > Advanced Options > Memory Split > 128"
echo ""
echo "2. Place your video files in: $VIDEOS_DIR"
echo ""
echo "3. Run the application with: dotnet run"
echo "   Or if published: ./VideoPlayer"
echo ""
echo "Setup complete!"
