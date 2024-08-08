import board
import busio
from adafruit_pm25.i2c import PM25_I2C

def initialize_PM25():
    i2c = busio.I2C(board.SCL, board.SDA)
    pm25 = PM25_I2C(i2c, reset_pin = None)
    return pm25
