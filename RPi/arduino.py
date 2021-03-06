import asyncio
import time
import json
import requests
import uuid
import datetime
from RPi import GPIO
import serial
ser = serial.Serial('/dev/serial/by-id/usb-1a86_USB2.0-Serial-if00-port0', 9600, timeout=1)  # open serial port

## script is running
print("#####################################################################################")
print("aruino.py script is running")
print("This script is part of a school project SmartShower")
print("For more info please visit: https://github.com/VanheeTimothy/projectSmartShower/tree/master/RPi")
print("#################################### #################################################")
print("\n")
print("script is waiting for first shower session...")
print("\n\n")

GPIO.setwarnings(False)
GPIO.setmode(GPIO.BCM)

RGB = [26,19,13]

GPIO.setup(RGB, GPIO.OUT)
GPIO.output(RGB, GPIO.LOW)
GPIO.output(RGB[2], GPIO.HIGH)

isRunning = False
guid = uuid.uuid4()

possibleColors = [1,2,3,4,5,6,7]

idShower = "F1E5FB65-42B1-04EB-7D17-11D1BFB2E008" #idshower is hardcoded and unique

async def TransmitData(parts, guid):
    url = "https://smartshowerfunctions.azurewebsites.net/api/SmartShower/AddSession"
    data = {"idsession": str(guid), "idshower": idShower, "profilenumber": int(parts[0]),
            "temp": float(parts[1]), "waterusage": float(parts[2]),
            "timestamp": str(datetime.datetime.now())}
    jsondata = json.dumps(data)
    print("\n"+jsondata)
    requests.post(url, data=jsondata)
    GPIO.output(RGB[2], GPIO.LOW)
    time.sleep(0.05)
    GPIO.output(RGB[2], GPIO.HIGH)
    print("sessie is verzonden")

def getColors(guid):
    url = "https://smartshowerfunctions.azurewebsites.net/api/SmartShower/getAvailableColors"
    data = {"idshower": str(guid)}
    jsondata = json.dumps(data)
    result = requests.post(url, data=jsondata)
    try:
        for color in result.json():
            possibleColors.remove(color)
    except Exception as e:
        print(e)
    return possibleColors


async def calculateData(sessionid):
    print("\nSession ended")
    print("Calculating session")
    url = "https://smartshowerfunctions.azurewebsites.net/api/SmartShower/calculateSession/{0}".format(sessionid)
    response = requests.get(url)
    print(response.status_code)
    if (response.status_code == 200):
        GPIO.output(RGB, GPIO.LOW)
        GPIO.output(RGB[1], GPIO.HIGH)  # Blue when data is succesful transmitted
        print("Session report stored into the SQLDb")



async def main(isRunning, guid):
    getColors(idShower)
    while 1:
        try:
            #TransmitData()
            line = ser.readline()  # read a '\n' terminated line
            session = line.decode('utf8').strip('\r\n')
            parts = session.split()
            if (len(parts) == 3 and len(str(guid)) == 36):  # indien de lijst korter/langer is >> data corupted!
                if(int(parts[0]) in possibleColors): #kleur moet toegewezen zijn aan een gebruiker
                    await TransmitData(parts, guid)
                    isRunning = True
                else:
                    print("No user selected")
                    print("Data will not be stored.")
            elif (session == "false"):
                GPIO.output(RGB, GPIO.LOW)
                GPIO.output(RGB[2], GPIO.HIGH)
                if (isRunning == 1):
                    await calculateData(guid)
                    time.sleep(1)
                    print("Waiting for other session...")
                    GPIO.output(RGB, GPIO.LOW)
                    guid = uuid.uuid4()
                    isRunning = False

        except UnicodeDecodeError as ex:
            print("##########")
            print(ex)
            print("##########")
            with open("error.txt", "a") as f:
                f.write("Failed to decode: {0} \ton {1}\n".format(str(ex), str(datetime.datetime.now())))
            GPIO.output(RGB[2], GPIO.LOW)
            GPIO.output(RGB[:2], GPIO.HIGH)  # Magneta when UnicodeDecodeError occur
            time.sleep(1)
        except Exception as ex:
            print("##########")
            print(ex)
            print("##########")
            with open("error.txt", "a") as f:
                f.write("An error has occurd: {0} \ton {1}\n".format(str(ex), str(datetime.datetime.now())))
            GPIO.output(RGB[0], GPIO.HIGH) # red on Error
            GPIO.output(RGB[1:], GPIO.LOW)
            time.sleep(1)


        except KeyboardInterrupt as K:
            GPIO.output(RGB, GPIO.LOW)
            print("\n\n\n\n")
            print("##################################")
            print("script ended by user")
            print("##################################")
            with open("error.txt", "a") as f:
                f.write("User ended the script: {0} \ton {1}\n".format(str(K), str(datetime.datetime.now())))
            GPIO.output(RGB[0], GPIO.HIGH)  #Yellow on pause
            GPIO.output(RGB[2], GPIO.HIGH)
            GPIO.output(RGB[1], GPIO.LOW)
            break


loop = asyncio.get_event_loop()
loop.run_until_complete(main(isRunning, guid))




