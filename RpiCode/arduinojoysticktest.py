import serial #pySerial
from time import sleep
ser = serial.Serial("/dev/ttyAMA0",115200) #Initialise to Raspberry pi GPIO serial port at 115200 baud


def doThing(sar, bytestringlast):
	byteintlast=[]
	
	for i in range(0,len(bytestringlast)):
		byteintlast.append(int(bytestringlast[i],2))

	bytebytelast=bytes(byteintlast)

	print(bytebytelast)
	sar.write(bytebytelast)



doThing(ser, ["00000000","00000000","00000000","01111111","00000000","00000000","00000000"])
sleep(1000/1000)
doThing(ser, ["00000000","00000000","00000000","10000000","00000000","00000000","00000000"]) #PROBLEM - Fix Arduino Code for this.
#Sending 10000000 as a byte in any message causes the arduino to count it as empty, only filling 6 of the 7 nescessary for it to update the joystick
#Consquently, sending this byte pulls everything out of alignment by one byte, breaking everything and requiring an extra byteS somewhere else to fix
sleep(1000/1000)
doThing(ser, ["00000000","00000000","00000000","00000000","00000000","00000000","00000000"])
sleep(1000/1000)

bytebytelist=bytes([0,0,0,0,0,0,0])
ser.write(bytebytelist)


