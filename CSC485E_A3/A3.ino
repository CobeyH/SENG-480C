// ---------------------------------------------------------------- //
// Arduino Ultrasoninc Sensor HC-SR04
// Re-writed by Arbi Abdul Jabbaar
// Using Arduino IDE 1.8.7
// Using HC-SR04 Module
// Tested on 17 September 2019

#include <CapacitiveSensor.h>

#define echoPin 2 // attach pin D2 Arduino to pin Echo of HC-SR04
#define trigPin 3 //attach pin D3 Arduino to pin Trig of HC-SR04
#define POTENTIOMETER_PIN A0 // Arduino pin connected to Potentiometer pin


// defines variables
long duration; // variable for the duration of sound wave travel
int distance; // variable for the distance measurement
CapacitiveSensor capacitive = CapacitiveSensor(6,7);

void setup() {
  pinMode(trigPin, OUTPUT); // Sets the trigPin as an OUTPUT
  pinMode(echoPin, INPUT); // Sets the echoPin as an INPUT
  pinMode(6, INPUT); // Capacitive Sensor
  Serial.begin(9600); // // Serial Communication is starting with 9600 of baudrate speed
  Serial.println("Ultrasonic Sensor HC-SR04 Test"); // print some text in Serial Monitor
  Serial.println("with Arduino UNO R3");
}
void loop() {
  // Ultrasonic
  // Clears the trigPin condition
  digitalWrite(trigPin, LOW);
  delayMicroseconds(2);
  // Sets the trigPin HIGH (ACTIVE) for 10 microseconds
  digitalWrite(trigPin, HIGH);
  delayMicroseconds(10);
  digitalWrite(trigPin, LOW);
  // Reads the echoPin, returns the sound wave travel time in microseconds
  duration = pulseIn(echoPin, HIGH);
  // Calculating the distance
  distance = duration * 0.034 / 2; // Speed of sound wave divided by 2 (go and back)
  // Displays the distance on the Serial Monitor

  //Capacitive
  long capTotal = capacitive.capacitiveSensor(30);


  // Potentiometer
  int analogValue = analogRead(POTENTIOMETER_PIN);
  Serial.print(analogValue);
  Serial.print(",");
//  // Ultrasonic
  Serial.print(distance);
  Serial.print(",");
  Serial.println(capTotal);

  delay(100);
  
}
