import time
import json
import requests
import uuid
import datetime
from RPi import GPIO

import serial
ser = serial.Serial('/dev/serial/by-id/usb-1a86_USB2.0-Serial-if00-port0', 9600, timeout=1)  #open serial port
GPIO.setmode(GPIO.BCM)

led = 21
GPIO.setup(led, GPIO.OUT)

guid = uuid.uuid4()
try:
    while 1:
        GPIO.output(led, GPIO.HIGH)
        line = ser.readline()  # read a '\n' terminated line
        session = line.decode().strip('\r\n')
        parts = session.split()
        if (len(parts) == 5 and len(str(guid)) == 36 and len(str(parts[0])) == 36):  # indien de lijst korter/langer is >> data corupted!
            url = "https://smartshowerfunctions.azurewebsites.net/api/SmartShower/AddSession"
            data = {"idsession": str(guid), "idshower": parts[0], "profilenumber": int(parts[1]),
                    "tijdfase": int(parts[2]), "temp": float(parts[3]), "waterusage": int(parts[4]),
                    "timestamp": str(datetime.datetime.now())}
            jsondata = json.dumps(data)
            print(jsondata)
            requests.post(url, data=jsondata)
        else:
            print("data is corrupted")
except KeyboardInterrupt:
    print("einde")


