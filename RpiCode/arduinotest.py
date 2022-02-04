import serial
from time import sleep
ser = serial.Serial("/dev/ttyAMA0",115200)


bytestringlist=["00110000","00000000","00000000","00000000","00000000","00000000","00000000"]
byteintlist=[]

def doThing(sar, bytestringlast):
	byteintlast=[]
	
	for i in range(0,len(bytestringlast)):
		byteintlast.append(int(bytestringlast[i],2))

	bytebytelast=bytes(byteintlast)

	print(bytebytelast)
	sar.write(bytebytelast)



doThing(ser,bytestringlist)
sleep(20/1000)
doThing(ser, ["00000000","00000000","00000000","00000000","00000000","00000000","00000000"])
sleep(20/1000)
doThing(ser, ["00000100","00000000","00000000","00000000","00000000","00000000","00000000"])
sleep(20/1000)
doThing(ser, ["00000010","00000000","00000000","00000000","00000000","00000000","00000000"])
sleep(20/1000)

bytebytelist=bytes([0,0,0,0,0,0,0])
ser.write(bytebytelist)
