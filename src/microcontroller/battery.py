import time
import board
import digitalio
import supervisor
from adafruit_max1704x import MAX17048
import display

# Exiting the battery state screen triggers a restart due to an apparent incompatibility
# between MAX17048 and PM25 monitor.

def display_state_if_D2_pressed():
    button_d2 = digitalio.DigitalInOut(board.D2)
    button_d2.pull = digitalio.Pull.DOWN

    if button_d2.value:
        print("Battery mode entered. Intitializing battery monitor.")
        battery_monitor = MAX17048(board.I2C())
        time.sleep(1)
        print(f"Cell voltage: {battery_monitor.cell_voltage:.2f} V")
        print(f"Battery level: {battery_monitor.cell_percent:.0f} %")
        display.battery_status(battery_monitor.cell_percent, battery_monitor.cell_voltage)
        time.sleep(2)

        while not button_d2.value:
            pass
        supervisor.reload()
