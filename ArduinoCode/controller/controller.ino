const int buttonPin = 2;
const int ledPin = 13;

int buttonState = LOW;
int lastButtonState = LOW;
int printed = 0;

long lastDebounceTime = 0;
long debounceDelay = 50;

void setup() {
  Serial.begin(9600);
  while(!Serial);
  pinMode(buttonPin, INPUT);
  pinMode(ledPin, OUTPUT);
}

void loop() {
  int reading = digitalRead(buttonPin);

  if (reading != lastButtonState) {
    lastDebounceTime = millis();
  }
 
  if ((millis() - lastDebounceTime) > debounceDelay) {
    buttonState = reading;
  }
  
  digitalWrite(ledPin, buttonState);
  if(buttonState == HIGH && printed != 1) {
    Serial.print('1');
    printed = 1;
  }
  
  if(buttonState == LOW) {
    printed = 0;
  }

  lastButtonState = reading;
}
