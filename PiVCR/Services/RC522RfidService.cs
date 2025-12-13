using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Mfrc522;
using Iot.Device.Rfid;

namespace PiVCR.Services;

public class RC522RfidService : IDisposable
{
    private readonly MfRc522 _mfrc522;
    private readonly GpioController _gpioController;
    private bool _disposed;
    private CancellationTokenSource? _cancellationTokenSource;
    private string? _lastTagId;
    private DateTime _lastTagTime = DateTime.MinValue;

    public event EventHandler<string>? TagDetected;

    public RC522RfidService(int spiBus = 0, int chipSelect = 0, int resetPin = 25)
    {
        try
        {
            var spiSettings = new SpiConnectionSettings(spiBus, chipSelect)
            {
                ClockFrequency = 10_000_000,
                Mode = SpiMode.Mode0
            };

            var spiDevice = SpiDevice.Create(spiSettings);
            _gpioController = new GpioController();

            _mfrc522 = new MfRc522(spiDevice, resetPin, _gpioController);

            Console.WriteLine("✓ RC522 RFID reader initialized successfully on SPI bus {0}, CS {1}, RST pin {2}",
                spiBus, chipSelect, resetPin);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Failed to initialize RC522: {ex.Message}");
            Console.WriteLine("  Make sure SPI is enabled: sudo raspi-config → Interface Options → SPI");
            throw;
        }
    }

    public void StartScanning()
    {
        if (_cancellationTokenSource != null)
        {
            Console.WriteLine("RC522 scanning is already running");
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        Console.WriteLine("✓ Starting RC522 RFID scanning...");

        Task.Run(async () => await ScanLoopAsync(_cancellationTokenSource.Token));
    }

    public void StopScanning()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = null;
        Console.WriteLine("RC522 scanning stopped");
    }

    private async Task ScanLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && !_disposed)
        {
            try
            {
                // Listen for an ISO 14443 Type A card
                if (_mfrc522.ListenToCardIso14443TypeA(out Iot.Device.Rfid.Data106kbpsTypeA card, TimeSpan.FromMilliseconds(200)))
                {
                    if (card.NfcId != null && card.NfcId.Length > 0)
                    {
                        string tagId = BitConverter.ToString(card.NfcId).Replace("-", "");

                        // Avoid duplicate reads (debounce for 2 seconds)
                        if (tagId != _lastTagId || (DateTime.Now - _lastTagTime).TotalSeconds > 2)
                        {
                            Console.WriteLine($"✓ RFID Tag detected: {tagId}");
                            _lastTagId = tagId;
                            _lastTagTime = DateTime.Now;

                            TagDetected?.Invoke(this, tagId);

                            // Halt the card to avoid continuous reads
                            _mfrc522.Halt();

                            // Wait a bit to avoid multiple reads of same card
                            await Task.Delay(1500, cancellationToken);
                        }
                    }
                }

                // Small delay to prevent CPU spinning
                await Task.Delay(100, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation, exit gracefully
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error reading RFID tag: {ex.Message}");
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            StopScanning();
            _mfrc522?.Dispose();
            _gpioController?.Dispose();
            Console.WriteLine("RC522 RFID service disposed");
        }
    }
}