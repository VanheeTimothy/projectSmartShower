import asyncio
import time
import json
import requests
import uuid
import datetime
from RPi import GPIO
import sys

import serial
GPIO.setwarnings(False)
ser = serial.Serial('/dev/serial/by-id/usb-1a86_USB2.0-Serial-if00-port0', 9600, timeout=1)  # open serial port
GPIO.setmode(GPIO.BCM)


RGB = [26,19,13]
GPIO.setup(RGB, GPIO.OUT)

## script runt
print("#####################################################################################")
print("aruino.py script is running")
print("This script is part of a school project SmartShower")
print("For more info please visit: https://github.com/VanheeTimothy/projectSmartShower/tree/master/RPi")
print("#####################################################################################")
GPIO.output(RGB[2], GPIO.HIGH)

print("\n")
print("script is waiting for first shower session...")
print("\n\n")

isRunning = False
guid = uuid.uuid4()
while 1:
    try:
        #TransmitData()
        line = ser.readline()  # read a '\n' terminated line
        session = line.decode().strip('\r\n')
        parts = session.split()
        if (len(parts) == 5 and len(str(guid)) == 36 and len(
                str(parts[0])) == 36):  # indien de lijst korter/langer is >> data corupted!
            GPIO.output(RGB[2], GPIO.HIGH)
            GPIO.output(RGB[:2], GPIO.LOW)
            url = "https://smartshowerfunctions.azurewebsites.net/api/SmartShower/AddSession"
            data = {"idsession": str(guid), "idshower": parts[0], "profilenumber": int(parts[1]),
                    "tijdfase": int(parts[2]), "temp": float(parts[3]), "waterusage": int(parts[4]),
                    "timestamp": str(datetime.datetime.now())}
            jsondata = json.dumps(data)
            print(jsondata)
            isRunning = True
            requests.post(url, data=jsondata)
        elif (session == "false"):
            GPIO.output(RGB, GPIO.LOW)
            GPIO.output(RGB[2], GPIO.HIGH)
            if (isRunning == 1):
                url = "https://smartshowerfunctions.azurewebsites.net/api/SmartShower/calculateSession/{0}".format(guid)
                response = requests.get(url)
                print(response.status_code)
                if(response.status_code == 200):
                    GPIO.output(RGB, GPIO.LOW)
                    GPIO.output(RGB[1], GPIO.HIGH) #Blue when data is succesful transmitted
                    print("Data succesful send to cloud")
                    time.sleep(1)
                    print("Waiting for other session...")
                    GPIO.output(RGB, GPIO.LOW)
                guid = uuid.uuid4()
                isRunning = False
    except UnicodeDecodeError as ex:
        GPIO.output(RGB[2], GPIO.LOW)
        GPIO.output(RGB[:2], GPIO.HIGH)  # Magneta when UnicodeDecodeError occur
        time.sleep(1)
    except Exception as ex:
        GPIO.output(RGB[0], GPIO.HIGH) # red on Error
        GPIO.output(RGB[1:], GPIO.LOW)
        time.sleep(1)

        print(ex)
    except KeyboardInterrupt:
        GPIO.output(RGB, GPIO.LOW)
        print("\n\n\n")
        print("##################################")
        print("script ended by user")
        print("##################################")
        GPIO.output(RGB[0], GPIO.HIGH)  # red on Error
        GPIO.output(RGB[2], GPIO.HIGH)
        GPIO.output(RGB[1], GPIO.LOW)
        break




def calculateData(sessionid):
    pass

def TransmitData():
    line = ser.readline()  # read a '\n' terminated line
    session = line.decode().strip('\r\n')
    parts = session.split()
    counter = 0
    if (len(parts) == 5 and len(str(guid)) == 36 and len(
            str(parts[0])) == 36):  # indien de lijst korter/langer is >> data corupted!

        url = "https://smartshowerfunctions.azurewebsites.net/api/SmartShower/AddSession"
        data = {"idsession": str(guid), "idshower": parts[0], "profilenumber": int(parts[1]),
                "tijdfase": int(parts[2]), "temp": float(parts[3]), "waterusage": int(parts[4]),
                "timestamp": str(datetime.datetime.now())}
        jsondata = json.dumps(data)
        print(jsondata)
        counter +=1
        requests.post(url, data=jsondata)
    elif (session == "false"):
        if(counter > 0):
            print("sessionId= " + str(guid))
            url = "https://smartshowerfunctions.azurewebsites.net/api/SmartShower/calculateSession/{0}".format(guid)
            requests.get(url)
            print(requests.get(url))
            counter = 0
