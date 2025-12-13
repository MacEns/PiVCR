RC522 Pin Raspberry Pi Pin GPIO Pin Description
SDA (SS) Pin 24 GPIO 8 (CE0) Chip Select
SCK Pin 23 GPIO 11 (SCLK) SPI Clock
MOSI Pin 19 GPIO 10 (MOSI) Master Out Slave In
MISO Pin 21 GPIO 9 (MISO) Master In Slave Out
IRQ Not connected - Optional interrupt
GND Pin 6, 9, 14, 20, 25, 30, 34, or 39 GND Ground
RST Pin 22 GPIO 25 Reset
3.3V Pin 1 or 17 3.3V Power (NOT 5V!)

RC522 Module Raspberry Pi 3B GPIO Header
(looking at top of board)

3.3V ────────────────[ 1] [ 2] 5V
[ 3] [ 4] 5V
[ 5] [ 6] GND ──────────── GND
[ 7] [ 8]
[ 9] [10]
[11] [12]
[13] [14]
[15] [16]
[17] [18]
MOSI ────────────────[19] [20]
MISO ────────────────[21] [22] ──────────────── RST
SCK ────────────────[23] [24] ──────────────── SDA (SS)
[25] [26]
