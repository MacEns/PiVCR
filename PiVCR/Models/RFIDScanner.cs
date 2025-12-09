using System.IO.Ports;
using System.Text.Json;

namespace PiVCR.Models;

public class RFIDScanner : IDisposable
{
    private SerialPort? _rfidPort;
    private Dictionary<string, string> _rfidToVideoMap = new();
    private bool _isEnabled = false;
    private readonly string _configFilePath;

    public event EventHandler<RFIDTagEventArgs>? TagDetected;

    public bool IsEnabled => _isEnabled;
    public int MappingCount => _rfidToVideoMap.Count;

    public RFIDScanner(string configFilePath = "rfid-config.json")
    {
        _configFilePath = configFilePath;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Load RFID configuration
            await LoadConfigurationAsync();

            // Try to detect and configure RFID scanner
            Console.WriteLine("Initializing RFID scanner...");

            // Common serial port names for RFID scanners on Raspberry Pi
            var possiblePorts = new[]
            {
                "/dev/ttyUSB0",
                "/dev/ttyUSB1",
                "/dev/ttyACM0",
                "/dev/ttyACM1"
            };

            foreach (var port in possiblePorts)
            {
                if (File.Exists(port))
                {
                    try
                    {
                        _rfidPort = new SerialPort(port, 9600, Parity.None, 8, StopBits.One);
                        _rfidPort.DataReceived += OnDataReceived;
                        _rfidPort.Open();
                        _isEnabled = true;
                        Console.WriteLine($"RFID scanner connected on {port}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Could not connect to RFID scanner on {port}: {ex.Message}"
                        );
                        _rfidPort?.Dispose();
                        _rfidPort = null;
                    }
                }
            }

            if (!_isEnabled)
            {
                Console.WriteLine("No RFID scanner detected. RFID functionality disabled.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing RFID scanner: {ex.Message}");
        }
    }

    public async Task LoadConfigurationAsync()
    {
        if (File.Exists(_configFilePath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(_configFilePath);
                var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                if (config != null)
                {
                    _rfidToVideoMap = config;
                    Console.WriteLine(
                        $"Loaded {_rfidToVideoMap.Count} RFID mappings from configuration."
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading RFID configuration: {ex.Message}");
            }
        }
        else
        {
            // Create default configuration file
            _rfidToVideoMap = new Dictionary<string, string>
            {
                { "0123456789", "/home/pi/Videos/movie1.mp4" },
                { "9876543210", "/home/pi/Videos/movie2.mp4" }
            };

            await SaveConfigurationAsync();
            Console.WriteLine($"Created default RFID configuration at {_configFilePath}");
        }
    }

    public async Task SaveConfigurationAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(
                _rfidToVideoMap,
                new JsonSerializerOptions { WriteIndented = true }
            );
            await File.WriteAllTextAsync(_configFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving RFID configuration: {ex.Message}");
        }
    }

    public void ShowMappings()
    {
        Console.WriteLine("\nCurrent RFID Mappings:");
        Console.WriteLine(new string('-', 60));

        if (_rfidToVideoMap.Any())
        {
            foreach (var mapping in _rfidToVideoMap)
            {
                Console.WriteLine($"RFID: {mapping.Key,-12} -> {mapping.Value}");
            }
        }
        else
        {
            Console.WriteLine("No RFID mappings configured.");
        }
    }

    public async Task AddMappingAsync(string rfidTag, string videoPath)
    {
        if (string.IsNullOrWhiteSpace(rfidTag))
        {
            throw new ArgumentException("RFID tag cannot be empty.", nameof(rfidTag));
        }

        if (string.IsNullOrWhiteSpace(videoPath))
        {
            throw new ArgumentException("Video path cannot be empty.", nameof(videoPath));
        }

        _rfidToVideoMap[rfidTag] = videoPath;
        await SaveConfigurationAsync();
    }

    public async Task<bool> RemoveMappingAsync(string rfidTag)
    {
        if (string.IsNullOrWhiteSpace(rfidTag))
        {
            return false;
        }

        var removed = _rfidToVideoMap.Remove(rfidTag);
        if (removed)
        {
            await SaveConfigurationAsync();
        }

        return removed;
    }

    public string? GetVideoPath(string rfidTag)
    {
        return _rfidToVideoMap.TryGetValue(rfidTag, out var videoPath) ? videoPath : null;
    }

    public void ShowScannerStatus()
    {
        if (!_isEnabled || _rfidPort == null)
        {
            Console.WriteLine("RFID scanner is not connected.");
            Console.WriteLine("Please check your RFID scanner connection and restart the application.");
            return;
        }

        Console.WriteLine("Scanner status: Connected");
        Console.WriteLine($"Port: {_rfidPort.PortName}");
        Console.WriteLine($"Baud Rate: {_rfidPort.BaudRate}");
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            var port = (SerialPort)sender;
            var data = port.ReadLine().Trim();

            if (!string.IsNullOrWhiteSpace(data))
            {
                Console.WriteLine($"\nRFID Tag detected: {data}");
                TagDetected?.Invoke(this, new RFIDTagEventArgs(data));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing RFID data: {ex.Message}");
        }
    }

    public void Dispose()
    {
        try
        {
            if (_rfidPort?.IsOpen == true)
            {
                _rfidPort.Close();
            }
            _rfidPort?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning up RFID scanner: {ex.Message}");
        }
    }
}

public class RFIDTagEventArgs : EventArgs
{
    public string TagId { get; }

    public RFIDTagEventArgs(string tagId)
    {
        TagId = tagId;
    }
}
