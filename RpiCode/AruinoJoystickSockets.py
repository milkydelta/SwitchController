import asyncio
import serial #pySerial
import websockets
import json


print("Please enter the serial port of the Arduino.")
print("Linux Desktop: /dev/ttySOMETHINGOROTHER")
print("RasPi GPIO: /dev/ttyAMA0")
print("Windows: COM_")
serport=input("")
ser = serial.Serial(serport,115200)

def doThing(sar, bytestringlast):
	byteintlast=[]
	
	for i in range(0,len(bytestringlast)):
		byteintlast.append(int(bytestringlast[i],2))

	bytebytelast=bytes(byteintlast)

	print(bytebytelast)
	sar.write(bytebytelast)

async def handler(websocket):
	while True:
		try:
			message= await websocket.recv()	#wait here until message received on this connection
		except websockets.ConnectionClosedOK:
			print("Client disconnected.")
			ser.write(bytes([0,0,0,0,0,0,0])) #reset controller to all blank
			break	#if Client disconnects, exit loop
		#print(message)
		try:
			messagedecoded=json.loads(message)
		except json.JSONDecodeError:
			print("Previous message was not a valid JSON document. Disconnecting offending client.")
			break
		if messagedecoded["type"] == "switchArdData":
			doThing(ser, messagedecoded["bytestringarray"])

		



async def main():
	async with websockets.serve(handler, "", 6969):
		await asyncio.Future()














if __name__ =="__main__":
	asyncio.run(main())
