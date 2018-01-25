#include <StandardCplusplus.h>
#include <iostream>
#include <Rotary.h>


//variabelen Rotary Encode
Rotary r = Rotary(2, 3);
float startTemp = 35;
float huidigeTemp;
float kleur;
float sterkte;
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
bool msgSend;
unsigned long timer;
unsigned long startTimer;
unsigned long tweedeKansTimer;

// variabelen leds
int ledStrip = 6;
int ledWater = 5;

int profielKleuren[7][3] = {
  {255, 0, 0}, //red
  {255, 165, 0}, // yellow
  {0, 255, 0}, // green
  {0, 255, 255}, // cyan
  {0, 0, 255}, // blue
  {255, 0, 255}, // purple
  {255, 255, 255} // white
};

void setup() {
  PCICR |= (1 << PCIE2);
  PCMSK2 |= (1 << PCINT18) | (1 << PCINT19);
  sei();
  Serial.begin(9600);
}


// LEDS
#include <PololuLedStrip.h>

PololuLedStrip<6> selectieRing;
PololuLedStrip<5> tempLed;

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
    if (huidigeTemp  < 54) {
      huidigeTemp += 0.25;
    }

    magAfkoelen = false; // indien men manueel de temperatuur instelt zal het timer
    tweedeKans = true;
    tweedeKansTimer = timer;
  }
  else if (result == DIR_CCW) {
    Serial.println("CounterClockWise");
    if (huidigeTemp > 15) {
      huidigeTemp -= 0.25;

    }
    magAfkoelen = false;
    tweedeKans = true;
    tweedeKansTimer = timer;
  }
}

void profielLed()
{
  kleurWaarde = analogRead(potKleur); //Dit is een getal van 0 tot 1023
  gekozenProfiel = map(kleurWaarde, 0, 1023, 0, 7);
  for (uint16_t i = 0; i < LED_COUNT_RING; i++)
  {
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
  sterkte = map(waterVerbruik, 0, 1023, 0, 255);
  literPerSeconde = literPerSeconde / 100;
  kleur = map(huidigeTemp, 15, 55, 0, sterkte);
  if (waterVerbruik > 10) // waterKraan is open
  {
    msgSend = true;
    Serial.println(String(gekozenProfiel + 1) + " " + String(huidigeTemp) + " " + String(literPerSeconde));
    for (uint16_t i = 0; i < LED_COUNT_TEMP; i++)
    {
      colorTemp[i] = rgb_color(kleur, 0, sterkte - kleur);
    }
    tempLed.write(colorTemp, LED_COUNT_TEMP);
    if (session == false) {
      startTimer = timer;
      magAfkoelen = true;
      huidigeTemp = startTemp;
    }
    if ((timer > (startTimer + 9000) && magAfkoelen == true) || (timer > (tweedeKansTimer + 9000) && tweedeKans == true)) // TIJD NOG AANPASSEN!!!!!!!!!!!!!!!!!
    {
      afkoelen = true;
    }
    else {
      afkoelen = false;
    }
    if (afkoelen && huidigeTemp > 15) {
      huidigeTemp -= 0.25;

    }
    session = true;
    delay(1000);
  }
  else {
    if (msgSend) {
      Serial.println("false");
      msgSend = false;

    }

    timer = 0;
    session = false;
    afkoelen = false;
    rgb_color out;
    for (uint16_t i = 0; i < LED_COUNT_TEMP; i++)
    {
      colorTemp[i] = out;
    }
    tempLed.write(colorTemp, LED_COUNT_TEMP);
    profielLed();
  }
}
