using System.IO.Ports;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PiVCR.Services;

namespace PiVCR.Models;

public enum RFIDScannerType
{
    Serial,
    RC522_SPI,
    RC522_I2C
}

public class RFIDScanner : IDisposable
{
    private SerialPort? _rfidPort;
    private RC522RfidService? _rc522Service;
    private RFIDScannerType _scannerType = RFIDScannerType.RC522_SPI;
    private Dictionary<string, string> _rfidToVideoMap = new();
    private bool _isEnabled = false;
    private readonly string _configFilePath;
    private readonly IConfiguration? _configuration;

    public event EventHandler<RFIDTagEventArgs>? TagDetected;

    public bool IsEnabled => _isEnabled;
    public int MappingCount => _rfidToVideoMap.Count;

    public RFIDScanner(string configFilePath = "rfid-config.json", IConfiguration? configuration = null)
    {
        _configFilePath = configFilePath;
        _configuration = configuration;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Load RFID configuration
            await LoadConfigurationAsync();

            // Check if RFID is enabled in configuration
            var rfidEnabled = _configuration?.GetValue<bool>("PiVCR:RFID:Enabled") ?? true;
            if (!rfidEnabled)
            {
                Console.WriteLine("RFID scanner disabled in configuration.");
                return;
            }

            // Get scanner type from configuration
            var scannerTypeStr = _configuration?.GetValue<string>("PiVCR:RFID:Type") ?? "RC522_SPI";
            if (Enum.TryParse<RFIDScannerType>(scannerTypeStr, out var scannerType))
            {
                _scannerType = scannerType;
            }

            Console.WriteLine($"Initializing RFID scanner ({_scannerType})...");

            switch (_scannerType)
            {
                case RFIDScannerType.RC522_SPI:
                    await InitializeRC522SPI();
                    break;
                case RFIDScannerType.Serial:
                    await InitializeSerial();
                    break;
                default:
                    Console.WriteLine($"Scanner type {_scannerType} not yet implemented.");
                    break;
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

    private async Task InitializeRC522SPI()
    {
        try
        {
            var spiBus = _configuration?.GetValue<int>("PiVCR:RFID:SPI:BusId") ?? 0;
            var chipSelect = _configuration?.GetValue<int>("PiVCR:RFID:SPI:ChipSelectLine") ?? 0;
            var resetPin = _configuration?.GetValue<int>("PiVCR:RFID:SPI:ResetPin") ?? 25;

            _rc522Service = new RC522RfidService(spiBus, chipSelect, resetPin);
            _rc522Service.TagDetected += OnRC522TagDetected;
            _rc522Service.StartScanning();
            _isEnabled = true;
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize RC522: {ex.Message}");
            _rc522Service?.Dispose();
            _rc522Service = null;
        }
    }

    private async Task InitializeSerial()
    {
        var possiblePorts = _configuration?.GetSection("PiVCR:RFID:Serial:PortNames").Get<string[]>()
            ?? new[] { "/dev/ttyUSB0", "/dev/ttyUSB1", "/dev/ttyACM0", "/dev/ttyACM1", "/dev/serial0" };
        var baudRate = _configuration?.GetValue<int>("PiVCR:RFID:Serial:BaudRate") ?? 9600;

        foreach (var port in possiblePorts)
        {
            if (File.Exists(port))
            {
                try
                {
                    _rfidPort = new SerialPort(port, baudRate, Parity.None, 8, StopBits.One);
                    _rfidPort.DataReceived += OnDataReceived;
                    _rfidPort.Open();
                    _isEnabled = true;
                    Console.WriteLine($"✓ RFID scanner connected on {port}");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not connect to RFID scanner on {port}: {ex.Message}");
                    _rfidPort?.Dispose();
                    _rfidPort = null;
                }
            }
        }
        await Task.CompletedTask;
    }

    private void OnRC522TagDetected(object? sender, string tagId)
    {
        TagDetected?.Invoke(this, new RFIDTagEventArgs(tagId));
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
            _rc522Service?.Dispose();
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
