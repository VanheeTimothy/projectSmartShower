#include <StandardCplusplus.h>

#include <iostream>
#include <map>

//Inlezen Van De Lichtsensor
int lichtSensor = A0;
int sensorWaarde = 0; //var om de analogRead in te stockeren
String idShower = "F1E5FB65-42B1-04EB-7D17-11D1BFB2E008";
int profileNumber = 3;
int tijdFase = 0;
float Temp = 22.2;
int waterUsage = 1;

using namespace std;
void setup() {
  Serial.begin(9600);
}

void loop() {
  //pinmode is niet nodig want hij heeft slechts 1 toestand
  //sensorWaarde = analogRead(lichtSensor); //Dit is een getal van 0 tot 1023
 /* Serial.println("Idshower");
  Serial.println(String(idShower));
  Serial.println("profileNr");
  Serial.println(String(profileNumber));
  Serial.println("tijdfase");
  Serial.println(String(tijdFase));
  Serial.println("Temp");
  Serial.println(String(Temp));
  Serial.println("waterUsage");
  Serial.println(String(waterUsage));*/

  

std::map<String, String> mySession; 
mySession["idshower"] = String(idShower);
mySession["profilenumber"] =  String(profileNumber);
mySession["tijdfase"] = String(tijdFase);
mySession["temp"] = String(Temp);
mySession["waterusage"] = String(waterUsage);

Serial.println(mySession["idshower"] + " "+ mySession["profilenumber"] + " "+ mySession["tijdfase"]+ " "+mySession["temp"]+ " "+mySession["waterusage"]);


//c++0x too

/*
= {
    { "idshower", idShower },
    { "profilenumber", String(profileNumber) },
    { "tijdfase", String(tijdFase) },
    { "temp", String(Temp) },
    { "waterusage", String(waterUsage) }
};*/
  delay(100);
  waterUsage ++;
}

float conversie(int sensorWaarde)//sensorwaarde als parameter want je moet deze converteren
{
  //Eerst code demo uitvoeren op lichtsensor om sensorwaarden te bekomen (deze zijn bij iedereen verschillend)
  //sensorwaarde donker 1021 => 0%
  //sensorwaarde licht 11 => 100%
  //n stappen = 1021 - 11 = 1010
  // aftrekken van 100 want anders krijg je groot percentage voor donker en klein voor licht
  float percent = 100-((sensorWaarde/1010.0)*100); //*100 want percent
 
 return percent;
}

