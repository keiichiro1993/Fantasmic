#include <Adafruit_NeoPixel.h>

#ifdef __AVR__
  #include <avr/power.h>
#endif

#define PIN 6

// Parameter 1 = number of pixels in strip
// Parameter 2 = Arduino pin number (most are valid)
// Parameter 3 = pixel ty pe flags, add together as needed:
//   NEO_KHZ800  800 KHz bitstream (most NeoPixel products w/WS2812 LEDs)
//   NEO_KHZ400  400 KHz (classic 'v1' (not v2) FLORA pixels, WS2811 drivers)
//   NEO_GRB     Pixels are wired for GRB bitstream (most NeoPixel products)
//   NEO_RGB     Pixels are wired for RGB bitstream (v1 FLORA pixels, not v2)
//   NEO_RGBW    Pixels are wired for RGBW bitstream (NeoPixel RGBW products)
Adafruit_NeoPixel strip = Adafruit_NeoPixel(30 , PIN, NEO_GRB + NEO_KHZ800);

// IMPORTANT: To reduce NeoPixel burnout risk, add 1000 uF capacitor across
// pixel power leads, add 300 - 500 Ohm resistor on first pixel's data input
// and minimize distance between Arduino and first pixel.  Avoid connecting
// on a live circuit...if you must, connect GND first.

void setup() {
  // This is for Trinket 5V 16MHz, you can remove these three lines if you are not using a Trinket
  #if defined (__AVR_ATtiny85__)
    if (F_CPU == 16000000) clock_prescale_set(clock_div_1);
  #endif
  // End of trinket special code

  Serial.begin(9600);      // 9600bpsでシリアルポートを開く

  initVariables();
  strip.begin();
  strip.show(); // Initialize all pixels to 'off'
}

int currentScene;
int currentMode;
bool introloop;

int center1 = 6;
int center2 = 25;

//できればモード切替のたびに呼んだほうが良い。
void initVariables(){
  introloop = false;
  strip.setBrightness(100);
  currentScene = 0;
  currentMode = 8;
}

void loop() {
  // Some example procedures showing how to display to the pixels:
/*
  colorWipe(strip.Color(255, 0, 0), 50); // Red
  colorWipe(strip.Color(0, 255, 0), 50); // Green
  colorWipe(strip.Color(0, 0, 255), 50); // Blue
//colorWipe(strip.Color(0, 0, 0, 255), 50); // White RGBW
  // Send a theater pixel chase in...
  theaterChase(strip.Color(127, 127, 127), 50); // White
  theaterChase(strip.Color(127, 0, 0), 50); // Red
  theaterChase(strip.Color(0, 0, 127), 50); // Blue
*/
  //始まりと最後の青と白のやつ, 色変えればほかの場面でも使えるかも
  //intro(30,0,255);//strip.Color(0, 20, 180));

  int i;
  if(currentScene == 0){  //アラビア
    switch(currentMode){
      case 0:
        rotateColorInOneColor(strip.Color(0, 0, 255), strip.Color(0, 0, 50));
        break;
      case 1:
        changeTwoColors(strip.Color(50, 50, 255), strip.Color(0, 0, 100));
        break;
      case 2:
        changeTwoColors(strip.Color(0, 200, 70), strip.Color(100, 200, 0));
        break;
      case 3:
        brighterOneColor(strip.Color(200, 0, 0));
        break;
      case 4:
        brighterOneColor(strip.Color(200, 100, 0));
        break;
      case 5:
        wipeTwoColors(strip.Color(50, 0, 255), strip.Color(100, 100, 100));
        break;
      case 6:
        brighterOneColor(strip.Color(50, 0, 255));
        break;
      case 7://真っ暗
        for(i=0; i<strip.numPixels(); i++){
          strip.setPixelColor(i, strip.Color(0, 0, 0));
        }
        strip.show();
        delay(100);
        break;
      default:
        //intro(0,200,15);
        //rainbowCycle(20);
        brighterOneColor(strip.Color(255, 20, 0));
        /*
        twoCenterTest(strip.Color(255, 20, 0), 0);
        delay(200);
        twoCenterTest(strip.Color(0, 0, 0), 0);
        delay(500);
        twoCenterTest(strip.Color(255, 20, 0), 2);
        delay(200);
        twoCenterTest(strip.Color(0, 0, 0), 2);
        delay(500);
        twoCenterTest(strip.Color(255, 20, 0), 1);
        delay(200);
        twoCenterTest(strip.Color(0, 0, 0), 1);
        delay(500);
        */
        break;
    }
  }

  checkSignal();//念のため変な設定になった時もSignalできるように。
  //delay(100);
  //rainbow(20);
  //rainbowCycle(20);
  //theaterChaseRainbow(50);
}

String str = "no data";

bool checkSignal(){
  if (Serial.available() > 0)
  {
    str = Serial.readStringUntil('\n');
  }
  Serial.println(str);

  if(!str.startsWith("Request Change:"))
  {
    return false;
  }

  currentScene = int(str.charAt(20) - '0');
  currentMode = int(str.charAt(26) - '0');
  
  Serial.print("Scene: ");
  Serial.println(currentScene);
  Serial.print("Mode: ");
  Serial.println(currentMode);

  str = "no data";
  return true;
}

//一色を強弱付ける
void brighterOneColor(uint32_t color){

  int i, j, brightness;

  for(i=0; i<strip.numPixels(); i++){
    strip.setPixelColor(i, color);
  }

  for(j=40; j<256; j+=3){
    strip.setBrightness(j);
    //Serial.println(j);
    strip.show();
    delay(10);
  }
  if(checkSignal())
  {
    return;
  }
  delay(150);

  for(j=0; j<215; j+=3){
    brightness = 255 - j;
    //Serial.println(brightness);
    strip.setBrightness(brightness);
    strip.show();
    delay(10);
  }
  if(checkSignal())
  {
    return;
  }
  delay(150);
}


//一色の中で白を回転
void rotateWhiteInOneColor(uint32_t color){
  int i;
  uint32_t white = strip.Color(150,150,150);
  for(i=0; i<strip.numPixels(); i++){
    strip.setPixelColor(i, color);
  }

  for(i=0; i<strip.numPixels(); i++){
    strip.setPixelColor(i, white);
    strip.setPixelColor(i+1, white);
    strip.show();
    delay(50);
    if(checkSignal())
    {
      return;
    }
    strip.setPixelColor(i, color);
    strip.setPixelColor(i+1, color);
  }
}

//一色の中でもう一色を回転
void rotateColorInOneColor(uint32_t color1, uint32_t color2){
  int i, j;
  uint32_t white = color2;

  for(j=0; j<4; j++){
    for(i=0; i<strip.numPixels(); i++){
      if(i%4 == 0)
      {
        strip.setPixelColor(i + j, color1);
      }else if(i%4 == 1)
      {
        strip.setPixelColor(i + j, color1);     
      }else if(i%4 == 2)
      {
        strip.setPixelColor(i + j, white);     
      }else if(i%4 == 3)
      {
        strip.setPixelColor(i + j, white);      
      }
    }
    
    strip.show();
    if(checkSignal())
    {
      return;
    }
    delay(100);
  }
}

void changeTwoColors(uint32_t color1, uint32_t color2)
{
  int i, j;

  for(j=0; j<2; j++)
  {
    for(i=0; i<strip.numPixels(); i++)
    {
      if((i%6 == 0 && j == 0) || (i%6 == 3 && j == 1))
      {
        strip.setPixelColor(i, color1);
      }else if((i%6 == 3 && j == 0) || (i%6 == 0 && j == 1))
      {
        strip.setPixelColor(i, color2);
      }else
      {
        strip.setPixelColor(i, 0);
      }
    }
    strip.show();
    if(checkSignal())
    {
      return;
    }
    delay(250);
  } 
}

//二色をワイプ
void wipeTwoColors(uint32_t color1, uint32_t color2)
{
  colorWipe(color1, 20);
  if(checkSignal())
  {
    return;
  }
  colorWipe(color2, 20);
  if(checkSignal())
  {
    return;
  }
}

//二色を強弱付けてマーブルローテーション
void rotateTwoColors(uint32_t color1, uint32_t color2, int wait){
  int i, j, brightness, k;
  uint32_t currentColor1, currentColor2;

  for(k=0; k<2; k++)
  {
    if(k==0){
      currentColor1 = color1;
      currentColor2 = color2;
    }
    else{
      currentColor1 = color2;
      currentColor2 = color1;
    }

    for(i=0; i<strip.numPixels(); i++){
      if(i%2 == 0){
        strip.setPixelColor(i, currentColor1);
      }
      else
      {
        strip.setPixelColor(i, currentColor2);
      }
    }

    for(j=1; j<256; j+=10){
      strip.setBrightness(j);
      //Serial.println(j);
      strip.show();
      delay(wait);
    }
  
    delay(50);

    for(j=0; j<255; j+=10){
      brightness = 255 - j;
      //Serial.println(brightness);
      strip.setBrightness(brightness);
      strip.show();
      delay(wait);
    }
    if(checkSignal())
    {
      return;
    }
  }
}

void intro(float red, float green, float blue){//uint32_t c) {
  int i, k;
  float j, brightness;
  uint32_t color, subcolor, white, subwhite;

  for(k=0; k<2; k++){
    for(j=0; j<1.0; j+=0.002){
      brightness = 1.0 - j;
      color = strip.Color(red * j, green * j, blue * j);
      subcolor = strip.Color(red * brightness, green * brightness, blue * brightness);
      white = strip.Color(100*j, 100*j, 100*j);
      subwhite = strip.Color(100*brightness, 100*brightness, 100*brightness);
      
      for(i=0; i<strip.numPixels(); i++){
        if(k==0){
          if(i%3 == 0){
           strip.setPixelColor(i, color);
          }
          else if(i%3 == 1)
          {
            if(introloop){
              strip.setPixelColor(i, white);
            }
          }else{
            if(introloop){
              strip.setPixelColor(i, subcolor);
            }
          }
        }
        else{
          if(i%3 == 0){
            strip.setPixelColor(i, subcolor);
          }else if(i%3 == 1){
            if(introloop){
              strip.setPixelColor(i, subwhite);
            }
          }else{
            strip.setPixelColor(i, color);
          }
        }
      }

     strip.show();
     if(checkSignal())
     {
       return;
     }
   }
  }
  introloop = true;
}

// Fill the dots one after the other with a color
void colorWipe(uint32_t c, uint8_t wait) {
  for(uint16_t i=0; i<strip.numPixels(); i++) {
    strip.setPixelColor(i, c);
    strip.show();
    delay(wait);
  }
}

void rainbow(uint8_t wait) {
  uint16_t i, j;

  for(j=0; j<256; j++) {
    for(i=0; i<strip.numPixels(); i++) {
      strip.setPixelColor(i, Wheel((i+j) & 255));
    }
    strip.show();
    if(checkSignal())
    {
      return;
    }
    delay(wait);
  }
}

// Slightly different, this makes the rainbow equally distributed throughout
void rainbowCycle(uint8_t wait) {
  uint16_t i, j;

  for(j=0; j<256*5; j++) { // 5 cycles of all colors on wheel
    for(i=0; i< strip.numPixels(); i++) {
      strip.setPixelColor(i, Wheel(((i * 256 / strip.numPixels()) + j) & 255));
    }
    strip.show();
    if(checkSignal())
    {
      return;
    }
    delay(wait);
  }
}

//Theatre-style crawling lights.
void theaterChase(uint32_t c, uint8_t wait) {
  for (int j=0; j<10; j++) {  //do 10 cycles of chasing
    for (int q=0; q < 3; q++) {
      for (uint16_t i=0; i < strip.numPixels(); i=i+3) {
        strip.setPixelColor(i+q, c);    //turn every third pixel on
      }
      strip.show();

      delay(wait);

      for (uint16_t i=0; i < strip.numPixels(); i=i+3) {
        strip.setPixelColor(i+q, 0);        //turn every third pixel off
      }
    }
  }
}

//Theatre-style crawling lights with rainbow effect
void theaterChaseRainbow(uint8_t wait) {
  for (int j=0; j < 256; j++) {     // cycle all 256 colors in the wheel
    for (int q=0; q < 3; q++) {
      for (uint16_t i=0; i < strip.numPixels(); i=i+3) {
        strip.setPixelColor(i+q, Wheel( (i+j) % 255));    //turn every third pixel on
      }
      strip.show();

      delay(wait);

      for (uint16_t i=0; i < strip.numPixels(); i=i+3) {
        strip.setPixelColor(i+q, 0);        //turn every third pixel off
      }
    }
  }
}

// Input a value 0 to 255 to get a color value.
// The colours are a transition r - g - b - back to r.
uint32_t Wheel(byte WheelPos) {
  WheelPos = 255 - WheelPos;
  if(WheelPos < 85) {
    return strip.Color(255 - WheelPos * 3, 0, WheelPos * 3);
  }
  if(WheelPos < 170) {
    WheelPos -= 85;
    return strip.Color(0, WheelPos * 3, 255 - WheelPos * 3);
  }
  WheelPos -= 170;
  return strip.Color(WheelPos * 3, 255 - WheelPos * 3, 0);
}

void twoCenterTest(uint32_t color, int mode)
{
  for(int i = 0; i + center1 < 19; i++){
    if(mode == 0 || mode==1){
      strip.setPixelColor(i + center1, color);
      if(i <= center1){
        strip.setPixelColor(center1 - i, color);
      }
    }
    if(mode == 1 || mode == 2){
      if(i <= center2){
        strip.setPixelColor(center2 - i, color);
      }
      if(i + center2 <= strip.numPixels()){
        strip.setPixelColor(center2 + i, color);
      }
    }
    strip.show();
    delay(50);
  }
}
