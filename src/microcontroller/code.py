import time
import battery
import server
import monitor
import display
import time_manager

def main():
    battery.display_state_if_D2_pressed()

    print("Intitializing.")
    display.initializing()
    pm25 = initialize_PM25_sensor()

    (online_mode, connection) = connect_to_wifi()

    if online_mode:
        online_mode = test_internet_connection(connection)

    if online_mode:
        time_manager.set_rtc_from_internet(connection)

    if online_mode:
        online_mode = test_server_availability(connection)

    if not online_mode:
        print("Using offline mode.")

    loop(connection, pm25, online_mode)

def initialize_PM25_sensor():
    while True:
        try:
            pm25 = monitor.initialize_PM25()
            print("✅ Sucessfully initialized PM2.5 sensor.")
            return pm25
        except Exception as err:
            print(f"❌ Unable to initialize sensor with error '{err}', retrying...")
            display.failed_and_retrying("Sensor init")
            time.sleep(5)

def connect_to_wifi():
    connection = server.initialize_wifi_connection()
    if connection is None:
        display.connection_failed("Wi-Fi")
        time.sleep(5)
        return (False, None)
    else:
        return (True, connection)

def test_internet_connection(connection):
    online_mode = server.test_internet_connection(connection)
    if online_mode:
        return True
    display.connection_failed("Internet")
    time.sleep(5)
    return False

def test_server_availability(connection):
    online_mode = server.test_server_availability(connection)
    if online_mode:
        return True
    display.connection_failed("Server")
    time.sleep(5)
    return False

def read_sensor_data(pm25):
    while True:
        try:
            #print("Reading data from sensor.")
            return pm25.read()
        except Exception as err:
            print(f"❌ Unable to read from sensor with error '{err}', retrying...")
            display.failed_and_retrying("Sensor reading")
            time.sleep(5)

def loop(connection, pm25, online_mode):
    READ_CADENCE = 5
    READS_BEFORE_POST = 12
    RETRIES_BEFORE_OFFLINE = 3
    reads_since_last_post = 0
    retries = 0
    while True:
        sensor_data = read_sensor_data(pm25)

        if online_mode and reads_since_last_post == READS_BEFORE_POST :
            reads_since_last_post = 0
            try:
                server.post_measurement(connection, sensor_data)
                retries = 0

            except Exception as err:
                print(f"❌ Error posting measurements to server: {err}")
                if retries >= RETRIES_BEFORE_OFFLINE:
                    print("Switching to offline mode.")
                    online_mode = False
                    retries = 0
                    time.sleep(READ_CADENCE)
                    continue

                print("Retrying...")
                display.failed_and_retrying("Posting to server")
                time.sleep(READ_CADENCE)
                retries += 1
                continue

        reads_since_last_post += 1
        display.print_sensor_data(sensor_data, online_mode)
        time.sleep(READ_CADENCE)

main()
