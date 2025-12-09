# Makefile for VideoPlayer

.PHONY: build test run clean publish-pi publish-pi32 publish-pi64 setup-samples

# Default target
all: build

# Build the application
build:
	@echo "Building PiVCR..."
	dotnet build

# Test build
test: build
	@echo "Running basic tests..."
	dotnet build --verbosity quiet

# Run the application
run:
	@echo "Running PiVCR..."
	dotnet run

# Clean build artifacts
clean:
	@echo "Cleaning build artifacts..."
	dotnet clean
	rm -rf bin obj

# Publish for Raspberry Pi (ARM64 - newer Pi models)
publish-pi64:
	@echo "Publishing for Raspberry Pi 4 (ARM64)..."
	dotnet publish -c Release -r linux-arm64 --self-contained false -o ./publish/pi64
	@echo "Published to ./publish/pi64/"
	@echo "Copy the entire ./publish/pi64/ folder to your Raspberry Pi"

# Publish for Raspberry Pi (ARM32 - older Pi models)
publish-pi32:
	@echo "Publishing for Raspberry Pi 3 and older (ARM32)..."
	dotnet publish -c Release -r linux-arm --self-contained false -o ./publish/pi32
	@echo "Published to ./publish/pi32/"
	@echo "Copy the entire ./publish/pi32/ folder to your Raspberry Pi"

# Publish for both architectures
publish-pi: publish-pi64 publish-pi32

# Publish with self-contained runtime (larger but no .NET requirement)
publish-standalone-pi64:
	@echo "Publishing standalone for Raspberry Pi 4 (ARM64)..."
	dotnet publish -c Release -r linux-arm64 --self-contained true -o ./publish/standalone-pi64
	@echo "Published to ./publish/standalone-pi64/"

publish-standalone-pi32:
	@echo "Publishing standalone for Raspberry Pi 3 and older (ARM32)..."
	dotnet publish -c Release -r linux-arm --self-contained true -o ./publish/standalone-pi32
	@echo "Published to ./publish/standalone-pi32/"

# Download sample videos for testing
setup-samples:
	@echo "Downloading sample videos..."
	./download-samples.sh

# Create deployment package
package: publish-pi
	@echo "Creating deployment packages..."
	mkdir -p ./dist
	cd ./publish/pi64 && tar -czf ../../dist/videoplayer-pi64.tar.gz *
	cd ./publish/pi32 && tar -czf ../../dist/videoplayer-pi32.tar.gz *
	cp setup-pi.sh ./dist/
	cp README.md ./dist/
	@echo "Deployment packages created in ./dist/"

# Show help
help:
	@echo "VideoPlayer Build System"
	@echo ""
	@echo "Available targets:"
	@echo "  build              - Build the application"
	@echo "  test               - Test the build"
	@echo "  run                - Run the application"
	@echo "  clean              - Clean build artifacts"
	@echo "  publish-pi64       - Publish for Raspberry Pi 4 (ARM64)"
	@echo "  publish-pi32       - Publish for Raspberry Pi 3 and older (ARM32)"
	@echo "  publish-pi         - Publish for both Pi architectures"
	@echo "  publish-standalone-*- Publish with runtime included"
	@echo "  setup-samples      - Download sample videos for testing"
	@echo "  package            - Create deployment packages"
	@echo "  help               - Show this help"
	@echo ""
	@echo "Quick start:"
	@echo "  make build         # Build the application"
	@echo "  make setup-samples # Download test videos"
	@echo "  make run           # Run the application"
	@echo ""
	@echo "For Raspberry Pi deployment:"
	@echo "  make publish-pi64  # For Pi 4 and newer"
	@echo "  make publish-pi32  # For Pi 3 and older"
