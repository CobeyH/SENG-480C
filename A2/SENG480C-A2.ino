/* Example sketch to control a 28BYJ-48 stepper motor with ULN2003 driver board and Arduino UNO. More info: https://www.makerguides.com */
// Include the Arduino Stepper.h library:
#include <Stepper.h>
// Define number of steps per rotation:
const int stepsPerRevolution = 2048;

int analogPin = A5;
int resVal = 0;
int Vin = 5;
float Vout = 0;
float R1 = 2000;
float R2 = 0;
float buffer = 0;
// Wiring:
// Pin 8 to IN1 on the ULN2003 driver
// Pin 9 to IN2 on the ULN2003 driver
// Pin 10 to IN3 on the ULN2003 driver
// Pin 11 to IN4 on the ULN2003 driver
// Create stepper object called 'myStepper', note the pin order:
Stepper myStepper = Stepper(stepsPerRevolution, 8, 10, 9, 11);

void setup() {
  myStepper.setSpeed(1);
  
  // Begin Serial communication at a baud rate of 9600:
  Serial.begin(9600);
}

void loop() {  
    Serial.println("Loop");
  // Ohm Meter Code:
   resVal = analogRead(analogPin);
  if(resVal){
    buffer = resVal * Vin;
    Vout = (buffer)/1024.0;
    buffer = (Vin/Vout) - 1;
    R2= R1 * buffer;
    int magnitudeIndex = getEnergyIndex();
    Serial.print("Magnitude: ");
    Serial.println(5 + 0.1 * magnitudeIndex);
    // Set motor speed
    Serial.print("Resistance: ");
    Serial.println(R2);
  } else {
    myStepper.setSpeed(1);
  }
  delay(1000);
}

int getEnergyIndex() {
  if(R2 <= 28.5) {
    return 0;
  } 
  if(R2 <= 73.5) {
    return 1;
  }
  if(R2 <= 160) {
    return 2;
  }
  if(R2 <= 275) {
    return 3;
  }
  if(R2 <= 505) {
    return 4;
  }
  if(R2 <= 840) {
    return 5;
  }
  if(R2 <= 1500) {
    return 6;
  }
  if(R2 <= 3500) {
    return 7;
  }
  if(R2 <= 7500) {
    return 8;
  }
  if(R2 <= 50000) {
    return 9;
  }
  if(R2 <= 200000) {
    return 10;
  }
}
