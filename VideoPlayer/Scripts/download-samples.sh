#!/bin/bash

# Sample Video Downloader for Testing
# This script downloads a few small sample videos for testing the video player

VIDEOS_DIR="./SampleVideos"

echo "=== Sample Video Downloader ==="
echo "This script will download a few small sample videos for testing."
echo "Videos will be saved to: $VIDEOS_DIR"
echo ""

# Create directory if it doesn't exist
mkdir -p "$VIDEOS_DIR"

# Sample video URLs (small files for testing)
declare -A VIDEOS=(
    ["big_buck_bunny_480p_1mb.mp4"]="https://sample-videos.com/zip/10/mp4/480/big_buck_bunny_480p_1mb.mp4"
    ["sample_video_480p.mp4"]="https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"
)

# Download function
download_video() {
    local filename="$1"
    local url="$2"
    local filepath="$VIDEOS_DIR/$filename"
    
    if [[ -f "$filepath" ]]; then
        echo "✓ $filename already exists, skipping..."
        return
    fi
    
    echo "Downloading $filename..."
    if command -v wget &> /dev/null; then
        wget -q --show-progress "$url" -O "$filepath"
    elif command -v curl &> /dev/null; then
        curl -L --progress-bar "$url" -o "$filepath"
    else
        echo "Error: Neither wget nor curl found. Please install one of them."
        return 1
    fi
    
    if [[ $? -eq 0 ]]; then
        echo "✓ Downloaded $filename successfully"
    else
        echo "✗ Failed to download $filename"
        rm -f "$filepath"
    fi
}

# Check for wget or curl
if ! command -v wget &> /dev/null && ! command -v curl &> /dev/null; then
    echo "Error: Neither wget nor curl found."
    echo "Please install wget or curl first:"
    echo "  sudo apt install wget"
    echo "  sudo apt install curl"
    exit 1
fi

# Download sample videos
echo "Downloading sample videos..."
echo ""

for filename in "${!VIDEOS[@]}"; do
    url="${VIDEOS[$filename]}"
    download_video "$filename" "$url"
    echo ""
done

echo "=== Download Complete ==="
echo ""
echo "Sample videos are available in: $VIDEOS_DIR"
echo ""
echo "You can now test the video player with:"
echo "  dotnet run \"$VIDEOS_DIR/big_buck_bunny_480p_1mb.mp4\""
echo ""
echo "Or use interactive mode:"
echo "  dotnet run"
echo ""
