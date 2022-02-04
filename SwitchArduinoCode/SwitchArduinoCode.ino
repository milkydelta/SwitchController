//Only works on Leonardo, Micro and other boards based on the 32u4
//H:-In order to function under switch, this relies upon resources from https://github.com/Jas2o/Leonardo-Switch-Controller

//I give thanks to this thread for educating me on how to convert a byte to a signed integer https://forum.arduino.cc/t/byte-to-signed-int/667253/5

//TODO: implement a counter for how many bytes of a message you have.
//TODO: make it possible to send -127 on an axis without shifting every message one byte out of alignment, breaking everything.


#include <Joystick.h>


byte dataBuffer[7] = {128,128,128,128,128,128,128};
int tempBuffer=0;

int16_t axisTemp;


void setup() {
  Serial1.begin(115200);

  Joystick.begin(false);

}


void loop() {
}

void joystickStateUpdate() {
  Joystick.setHatSwitch(int(dataBuffer[2]));
  
  for (int i=0;i<7;i++) {
    Joystick.setButton(i,bitRead(dataBuffer[0],i));
  }
  for (int i=0;i<7;i++) {
    Joystick.setButton(i+7,bitRead(dataBuffer[1],i));
  }
  
  axisTemp = (int8_t)dataBuffer[3];
  Joystick.setXAxis(axisTemp);
  axisTemp = (int8_t)dataBuffer[4];
  Joystick.setYAxis(axisTemp);
  axisTemp = (int8_t)dataBuffer[5];
  Joystick.setZAxis(axisTemp);
  axisTemp = (int8_t)dataBuffer[6];
  Joystick.setZAxisRotation(axisTemp);

  Joystick.sendState();

  for (int i=0;i<7;i++) {
    dataBuffer[i]=128;
    }
}

void serialEvent1() {
  tempBuffer = Serial1.read();
  if (tempBuffer!=-1){
    for (int i=0;i<7;i++) {
      if (dataBuffer[i]==128){
        dataBuffer[i]=byte(tempBuffer);
        if (i==6) {
          joystickStateUpdate();
        }
        break;
        }
      }
    }
  }
