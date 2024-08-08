import board
import terminalio
import displayio
from adafruit_display_text import label

_RED = 0xFF0000
_GREEN = 0x228B22
_YELLOW = 0xD1D100

_display = board.DISPLAY
_display.auto_refresh = False

def print_sensor_data(sensor_data, online_mode):
    group = _create_black_group()
    if online_mode:
        _text("Airborne particles", 0, 10, _YELLOW, group)
    else:
        _text("Particles (offline)", 0, 10, _YELLOW, group)
    _text(f"PM1:   {sensor_data['pm10 standard']}", 20, 50, _GREEN, group)
    _text(f"PM2.5: {sensor_data['pm25 standard']}", 20, 80, _GREEN, group)
    _text(f"pm10:  {sensor_data['pm100 standard']}", 20, 110, _GREEN, group)
    _display.refresh()

def initializing():
    group = _create_black_group()
    _text("Initializing...", 30, 60, _GREEN, group)
    _display.refresh()

def connection_failed(step):
    x = 0
    group = _create_black_group()
    _text(step + " connection", x, 20, _RED, group)
    _text("failed!", x, 50, _RED, group)
    _text("Entering offline", x, 90, _GREEN, group)
    _text("mode...", x, 120, _GREEN, group)
    _display.refresh()

def failed_and_retrying(step):
    x = 0
    group = _create_black_group()
    _text(step, x, 20, _RED, group)
    _text("failed!", x, 50, _RED, group)
    _text("Retrying...", x, 90, _YELLOW, group)
    _display.refresh()

def battery_status(percent, voltage):
    x = 0
    group = _create_black_group()
    _text("Battery status", x, 20, _YELLOW, group)
    _text(f"Charge level: {percent:.1f} %", x, 60, _GREEN, group)
    _text(f"Cell voltage: {voltage:.2f} V", x, 100, _GREEN, group)
    _display.refresh()

def _create_black_group():
    group = displayio.Group()
    _display.root_group = group
    return group

def _text(text, x, y, color, group):
    text_area = label.Label(terminalio.FONT, text=text, color=color)
    text_width = text_area.bounding_box[2] * 2
    text_group = displayio.Group(scale=2, x=x, y=y)
    text_group.append(text_area)
    group.append(text_group)
