# Opstelling Arduino en raspberry pi

dit is onderdeel van het project SmartShower

## Configuratie Aruino (UNO)

De eerste stap is het dowloaden van de Arduino Ide. Vervolgens kan u de software installeren. Tijdens de installatie kunt u de arduino aansluiten via de usb met de computer. Eenmaal de installatie is voltooid kan u het arduino.ino  bestand uploaden.
### Mogelijke problemen
Problemen bij het uploaden naar het board
![alt text](https://raw.githubusercontent.com/username/projectname/branch/path/to/img.png)
Navigeer onder het tabblad hulpmiddelen >> poort en selecteer de aangeven poort bv COM3.

## Configuratie raspberry pi 3
Voor de Raspberry Pi is de laatste versie van Debian  gebruikt geweest als besturingsysteem. Met behulp van Win32DiskImager dient de SD kaart met dit besturingsysteem geinstalleerd te worden. Eenmaal de image geinstalleerd is kan men zich aanmelden op de raspberry pi met standaard credentials username = pi password= raspberry. Voor een correcte installatie verwijs ik naar de officele documentatie .
Eenmaal de raspberry pi geconfigureerd is en de gebruiker zich in de home folder bevindt, kan de volgende stap ondernomen worden:
```
git clone: https://github.com/VanheeTimothy/projectSmartShower.git
```
belangrijk hierbij is de file arduino.py rechten krijgt, dit omdat het script moet runnen bij opstart. De u kunt de file de juiste rechten geven door volgend commando
```
sudo chmod +x arduino.py
```
vervolgens maken we een opstartscript aan op volgende locatie /bin/. Het aanmaken van het opstart script gebeurd als volgt: 
```
sudo nano smartShower.sh
```
in het bestand kopieer je volgende tekst: 
```
#!/bin/sh
sleep 15
sudo python /home/pi/Documents/SmartShower/arduino.py
```
noot: het pad van het .py file kan anders zijn dan deze van uw pi om de juiste directory te vinden navigeer naar Arduino.py en gebruik volgend commando pwd.
Sla smartShower.sh op door crtl+x y in te typen. Tot slot moeten we de /etc/rc.local aanpassen
```
Sudo nano /etc/rc.local
```
Kopieer volgende tekst:
```
/bin/smartShower.sh &
Exit 0
CRTL+x y
```
Indien problemen zich hiervoor doen, kan men ook kiezen voor volgende optie om het script te laten runnen at start up. Doch prefereer ik de voorgaande optie.
```
sudo nano /etc/profile
sudo python /home/pi/Documents/SmartShower/arduino.py &
```
Gezien arduino.py gebruikt maakt van een packages die niet standaard met python is meegeleverd moeten we deze zelf in de terminal toevoegen. Gelukkig is pip wel standaard op de debian image geinstalleerd. Om de ontbrekende packages toe te voegen run volgende commando’s:
```
pip install pyserial
pip install requests
```

## mogelijke problemen
Het opstart script start niet op en print volgende error
```
File "Documents/SmartShower/arduino.py", line 19, in <module>
    line = ser.readline()  # read a '\n' terminated line
  File "/usr/lib/python2.7/dist-packages/serial/serialposix.py", line 490, in read
    'device reports readiness to read but returned no data '
serial.serialutil.SerialException: device reports readiness to read but returned no data (device disconnected or multiple access on port?)

```
Zorg ervoor dat de USB-kabel, tussen de Arduino en Raspberry Pi, goed en correct in de juiste poort zit. Indien het probleem nog niet is verholpen, type volgend commando in de terminal:
```
ls /dev/serial/by-id
```
Nu wordt er een id van de poort afgeprint. Kopieer het id samen met de locatie ernaar toe. Indien er geen id wordt afgeprint, controleer of de USB-kabel correct is aangesloten. Indien het probleem zich nog blijft voordoen, test voorgaande stap opnieuw met een andere USB-kabel.
We moeten nu onze arduino.py file open:

```
sudo nano /home/pi/Documents/SmartShower/arduino.py
```
Verwijder de de tekst in de serial.Serial()  en plak de voorgaande gekopieerde tekst.

