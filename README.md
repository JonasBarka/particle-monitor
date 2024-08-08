# Particle Monitor
### A learning project programming a microcontroller and particle sensor 

A microcontroller with integrated display ([Adafruit ESP32-S3 Reverse TFT Feather](https://www.adafruit.com/product/5691)) connected to a particle sensor ([Adafruit PMSA003I Air Quality Breakout](https://www.adafruit.com/product/4632)), both from [Adafruit](https://www.adafruit.com/).
The device detects the level of air particles and displays it on the screen. If connected via WiFi the result is sent a server. The controller is programmed using [CircuitPython 9](https://circuitpython.org/).

The goal of this project is to get a basic knowledge of Python and microcontroller programming.

The code depends on the following CircuitPython libraries from Adafruit:
- Bitmap font
- Bus device
- Connection manager
- Display Text
- Max1704x
- PM25
- Register
- Requests

The easiest way to get the libraries is by downloading the [bundle](https://circuitpython.org/libraries).
