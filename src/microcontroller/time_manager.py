import os
import time
import rtc

_TIME_SERVER_URL = "https://timeapi.io/api/Time/current/zone?timeZone=utc"

def set_rtc_from_internet(connection):
    print(f"Retrieving time using url: {_TIME_SERVER_URL}")
    with connection.get(_TIME_SERVER_URL) as response:
        utc = response.json()
        print(f"✅ GET response: {utc}")

    # Day is always set to monday as it is not used.
    rtc.RTC().datetime = time.struct_time((
        utc["year"],
        utc["month"],
        utc["day"],
        utc["hour"],
        utc["minute"],
        utc["seconds"],
        0, -1, -1))
    print(f"Board time set to UTC: {rtc.RTC().datetime}")
