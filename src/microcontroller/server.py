import os
import adafruit_connection_manager
import wifi
import adafruit_requests
import json

# Ensure environment variables are defined in settings.toml
_device_id = os.getenv("DEVICE_ID")
_ssid = os.getenv("CIRCUITPY_WIFI_SSID")
_password = os.getenv("CIRCUITPY_WIFI_PASSWORD")
_internet_connection_test_url = os.getenv("INTERNET_CONNECTION_TEST_URL")
_server_connection_test_url = os.getenv("SERVER_CONNECTION_TEST_URL")
_server_connection_expected_response = os.getenv("SERVER_CONNECTION_EXPECTED_RESPONSE")
_server_post_url = os.getenv("SERVER_POST_URL")

def initialize_wifi_connection():
    try:
        print(f"Searching for Wi-Fi {_ssid}...")

        # Initalize Wifi, Socket Pool, Request Session
        pool = adafruit_connection_manager.get_radio_socketpool(wifi.radio)
        ssl_context = adafruit_connection_manager.get_radio_ssl_context(wifi.radio)
        connection = adafruit_requests.Session(pool, ssl_context)
        if connection is None:
            raise RuntimeError

        rssi = wifi.radio.ap_info.rssi
        print(f"Signal strength: {rssi}")
        print(f"Connecting to {_ssid}...")

        # Connect to the Wi-Fi network
        wifi.radio.connect(_ssid, _password)
        print("✅ Sucessfully connected to Wi-Fi.")
        return connection

    except Exception as err:
        print(f"❌ Wi-Fi connection failed: {err}")
        return None

def test_internet_connection(connection):
    print(f"Testing internet connection by calling {_internet_connection_test_url}...")
    try:
        with connection.get(_internet_connection_test_url) as response:
            if response.status_code == 200:
                print("✅ Get returned 200.")
                return True
            else:
                print("❌ Get did not return 200.")
                return False

    except Exception as err:
        print(f"❌ Exception: {err}")
        return False

def test_server_availability(connection):
    print(f"Testing server availability by calling {_server_connection_test_url}...")
    try:
        with connection.get(_server_connection_test_url) as response:
            if response.text == _server_connection_expected_response:
                print("✅ Get returned expected response.")
                return True
            else:
                print(f'❌ Get did not return expected response "{_server_connection_expected_response}", but "{response.text}"')
                return False

    except Exception as err:
        print(f"❌ Exception: {err}")
        return False

def post_measurement(connection, sensor_data):
    measurement = {
        "deviceId": _device_id,
        "pm10": sensor_data["pm10 standard"],
        "pm25": sensor_data["pm25 standard"],
        "pm100": sensor_data["pm100 standard"]
    }
    measurement_json = json.dumps(measurement)
    print(f"Posting to server: {_server_post_url} - {measurement_json}")
    with connection.post(_server_post_url, data=measurement_json, headers={"Content-Type": "application/json"}) as response:
        print(f"✅ POST response: {response.json()}")
