#include <StandardCplusplus.h>
#include <iostream>
#include <Rotary.h>

//#include <map>

//variabelen Rotary Encode
Rotary r = Rotary(2, 3);
float startTemp = 35;
float huidigeTemp;

// variabelen potentiometer Kleur selecteren
int potKleur = A5;
int kleurWaarde;
int gekozenProfiel;
int selectieProfiel;

// variabelen potentiometer Water
int potWater = A4;
int waterVerbruik = 0;
float literPerSeconde;
bool afkoelen; 
bool magAfkoelen;
bool tweedeKans;

bool session;
unsigned long timer;
unsigned long startTimer;
unsigned long tweedeKansTimer;

// variabelen leds
int ledStrip = 6;
int ledWater = 5;

int profielKleuren[7][3] = {
  {255, 0, 0}, //red
  {255, 165, 0}, // orange
  {0, 255, 0}, // green
  {0, 255, 255}, // cyan
  {0, 0, 255}, // blue
  {255, 0, 255}, // purple
  {255, 255, 255} // white
};



String idShower = "F1E5FB65-42B1-04EB-7D17-11D1BFB2E008";

// using namespace std;
void setup() {
  PCICR |= (1 << PCIE2);
  PCMSK2 |= (1 << PCINT18) | (1 << PCINT19);
  sei();
  Serial.begin(9600);
}


// LEDS
#include <PololuLedStrip.h>

// Which pin on the Arduino is connected to the NeoPixels?
// Create an ledStrip object and specify the pin it will use.
PololuLedStrip<6> selectieRing;
PololuLedStrip<5> tempLed;

// How many NeoPixels are attached to the Arduino?
#define LED_COUNT_RING 5
#define LED_COUNT_TEMP 60

rgb_color colorRing[LED_COUNT_RING];
rgb_color colorTemp[LED_COUNT_TEMP];

ISR(PCINT2_vect) {
  unsigned char result = r.process();
  if (result == DIR_NONE) {
    // do nothing
  }
  else if (result == DIR_CW) {
    Serial.println("ClockWise");
    huidigeTemp += 0.25;
    magAfkoelen = false; // indien men manueel de temperatuur instelt zal het timer
    tweedeKans = true;
    tweedeKansTimer = timer;
  }
  else if (result == DIR_CCW) {
    Serial.println("CounterClockWise");
    huidigeTemp -= 0.25;
    magAfkoelen = false;
    tweedeKans = true;
    tweedeKansTimer = timer;

  }
}

void profielLed()
{
  kleurWaarde = analogRead(potKleur); //Dit is een getal van 0 tot 1023
  gekozenProfiel = map(kleurWaarde, 0, 1023, 0, 8);
  Serial.println("kleurwaarde: " + String(kleurWaarde));
  Serial.println("selectie profiel: " + String(gekozenProfiel));
    for (uint16_t i = 0; i < LED_COUNT_RING; i++)
  {
     Serial.println("r: " + String(profielKleuren[gekozenProfiel][0]));
  Serial.println("g: " + String(profielKleuren[gekozenProfiel][1]));
  Serial.println("b: " + String(profielKleuren[gekozenProfiel][2]));

     colorRing[i] = rgb_color(profielKleuren[gekozenProfiel][0],
                        profielKleuren[gekozenProfiel][1],
                        profielKleuren[gekozenProfiel][2]);
  }
  
   selectieRing.write(colorRing, LED_COUNT_RING);
                

}

void loop() {
  timer = millis();
  waterVerbruik = analogRead(potWater);
  literPerSeconde = map(waterVerbruik, 0, 1023, 0, 25);
  literPerSeconde = literPerSeconde / 100;
  Serial.println("waterverbruik: " + String(waterVerbruik));
  if (waterVerbruik > 10) // waterKraan is open
  {
    Serial.println("duration: " + String(timer));
    Serial.println("Douche sessie begonnen ");
    Serial.println("het profielnummer die nu aan het douchen is: " + String(gekozenProfiel));
    Serial.println("literPerSeconde: " + String(literPerSeconde));
    Serial.println("De voorlopige Temperatuur: " + String(huidigeTemp));
    if (session == false) {
      startTimer = timer;
      magAfkoelen = true;
      huidigeTemp = startTemp;
      Serial.println(startTimer);
    }
    Serial.println("Actual millis: " + String(timer));
    Serial.println("startTimeShower: " + String(startTimer));
    if ((timer > (startTimer + 9000) && magAfkoelen == true) || (timer > (tweedeKansTimer + 9000) && tweedeKans == true)) // TIJD NOG AANPASSEN!!!!!!!!!!!!!!!!!
    {
      afkoelen = true;
    }
    else {
      afkoelen = false;
    }
    if (afkoelen) {
      huidigeTemp -= 0.25;
    }
    session = true;
  }
  else {
    timer = 0;
    session = false;
    afkoelen = false;
    profielLed();
    
  }
}
