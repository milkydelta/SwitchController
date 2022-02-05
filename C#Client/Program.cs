using System;
using System.Collections.Generic;
using WebSocketSharp;
using System.Windows.Input;
using System.Text.Json;



namespace CustomExtensions
{
	public static class ExtensionMethods
	{
//The Arduino map() function implemented as an Extension Method. https://www.arduino.cc/reference/en/language/functions/math/map/
		public static decimal Map(this decimal value, decimal fromSource, decimal toSource, decimal fromTarget, decimal toTarget)
		{
			return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
		}

		public static int Map(this int value, float fromSource, float toSource, float fromTarget, float toTarget) 
		{
			return Convert.ToInt32((Convert.ToSingle(value) - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget);
		}
	}
}


namespace C_Client
{
	using CustomExtensions;
	//define the class for the JSON message sent over the network
	public class JSONSwitchMessage
	{
		public string type { get; set; }
		public string[] bytestringarray { get; set; }
	}

	class Program
	{
		public static string ToBinary(UInt64 myValue,int digits)
		{
			unchecked
			{
				Int64 cheeseStrategy = (long)myValue;
				return Convert.ToString(cheeseStrategy, 2).PadLeft(digits, '0');
			}
		}
		public static string ToBinary(byte myValue, int digits) 
		{
			return Convert.ToString(myValue, 2).PadLeft(digits, '0');
		}


		
		//Initialise the dictionary for which keyboard keys correspond
		//to which controller buttons in the bit message.
		static Dictionary<System.Windows.Input.Key, UInt16> Key2BytesDict = new Dictionary<System.Windows.Input.Key, UInt16>();
		static void AddToDict()
		{
			// Key2BytesDict.Add(Key, Convert.ToInt64(Math.Pow(2, Position_In_Byte+(8*Byte))));
			//Where Position_in_Byte and Byte are counted from zero, from the left
			Key2BytesDict.Add(Key.S, 0b00000001_00000000); //Y BUTTON
			Key2BytesDict.Add(Key.Z, 0b00000010_00000000); //B BUTTON
			Key2BytesDict.Add(Key.X, 0b00000100_00000000); //A BUTTON
			Key2BytesDict.Add(Key.C, 0b00001000_00000000); //X BUTTON

			Key2BytesDict.Add(Key.A, 0b00010000_00000000);//L
			Key2BytesDict.Add(Key.D, 0b00100000_00000000);//R

			Key2BytesDict.Add(Key.Q, 0b01000000_00000000); //ZL
			Key2BytesDict.Add(Key.E, 0b00000000_00000001); //ZR

			Key2BytesDict.Add(Key.Back, 0b00000000_00000010); //-
			Key2BytesDict.Add(Key.Enter, 0b00000000_00000100); //+

			Key2BytesDict.Add(Key.LeftShift, 0b00000000_00001000); //Left Stick Click
			Key2BytesDict.Add(Key.LeftCtrl, 0b00000000_00010000); //Right Stick Click

			Key2BytesDict.Add(Key.Home, 0b00000000_00100000); //Home
			Key2BytesDict.Add(Key.End, 0b00000000_01000000); //Capture

		}

		//The websocket connection requires this function to be executed on STAThread
		[STAThread]
		static void Main(string[] args)
		{
			AddToDict();
			Console.WriteLine("Arduino Switch Controller  PC Websocket Client\nEnter the URL fo the WebSocket server (RPi): ");
			string serverURL = Console.ReadLine();
			using (var ws = new WebSocket(serverURL))
			{
				ws.Connect();

				//Initialise the JSON message and other variables used in the loop
				byte[] byteArray = {0,0,0,0,0,0,0};
				UInt16 buttonByteBuffer = 0;
				JSONSwitchMessage byteJSONbuffer = new JSONSwitchMessage();
				byteJSONbuffer.type = "switchArdData";
				byteJSONbuffer.bytestringarray = new string[] { "", "", "", "", "", "", "" };
				int axisUpTemp;
				int axisDownTemp;
				int axisLeftTemp;
				int axisRightTemp;
				int Dx;
				int Dy;
				double Angle;
				SByte hatValue;

				
				while (true)
				{
					if (Keyboard.IsKeyDown(Key.Escape)) { break; }
					
					//Add all pressed buttons to the buffer for transmission
					foreach (KeyValuePair<Key, UInt16> DictByte in Key2BytesDict)
					{
						if ((Keyboard.GetKeyStates(DictByte.Key) & KeyStates.Down) > 0)
						{
							buttonByteBuffer += DictByte.Value;
						}
					}
					byteArray[0]=(byte)(buttonByteBuffer >> 8);//right shift of 8 discards last 8 bits and puts first 8 in their place
					byteArray[1]=(byte)(buttonByteBuffer & 0b00000000_11111111); //bitwise AND isolates last 8 bits to avoid OverflowException in cast

					
					//convert states of joystick buttons to axes and add them to the buffer
					//must be done in unchecked space to convert the bits of a negative number to the type of unsigned
					unchecked
					{
						axisLeftTemp=Convert.ToInt32((Keyboard.GetKeyStates(Key.Left) & KeyStates.Down) > 0);
						axisRightTemp=Convert.ToInt32((Keyboard.GetKeyStates(Key.Right) & KeyStates.Down) > 0);
						axisUpTemp=Convert.ToInt32((Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) > 0);
						axisDownTemp=Convert.ToInt32((Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) > 0);

						Dx = axisRightTemp-axisLeftTemp;
						Dy = axisUpTemp-axisDownTemp;

						Dx = Dx.Map(-1,1,-127,127);
						Dy = Dy.Map(-1,1,127,-127);

						byteArray[3]=(byte)(sbyte)Dx;
						byteArray[4]=(byte)(sbyte)Dy;

						axisLeftTemp=Convert.ToInt32((Keyboard.GetKeyStates(Key.J) & KeyStates.Down) > 0);
						axisRightTemp=Convert.ToInt32((Keyboard.GetKeyStates(Key.L) & KeyStates.Down) > 0);
						axisUpTemp=Convert.ToInt32((Keyboard.GetKeyStates(Key.I) & KeyStates.Down) > 0);
						axisDownTemp=Convert.ToInt32((Keyboard.GetKeyStates(Key.K) & KeyStates.Down) > 0);

						Dx = axisRightTemp-axisLeftTemp;
						Dy = axisUpTemp-axisDownTemp;

						Dx = Dx.Map(-1,1,-127,127);
						Dy = Dy.Map(-1,1,127,-127);

						byteArray[5]=(byte)(sbyte)Dx;
						byteArray[6]=(byte)(sbyte)Dy;

					}


					//Get value of dpad keys and convert to two axes before finding HAT value by angle between vectors
					axisUpTemp=Convert.ToInt32((Keyboard.GetKeyStates(Key.T) & KeyStates.Down) > 0);
					axisDownTemp=Convert.ToInt32((Keyboard.GetKeyStates(Key.G) & KeyStates.Down) > 0);
					axisLeftTemp=Convert.ToInt32((Keyboard.GetKeyStates(Key.F) & KeyStates.Down) > 0);
					axisRightTemp=Convert.ToInt32((Keyboard.GetKeyStates(Key.H) & KeyStates.Down) > 0);
					
					Dx = axisRightTemp-axisLeftTemp;
					Dy = axisUpTemp-axisDownTemp;
					Angle = (360 + Math.Atan2(Dx, Dy) * (180 / Math.PI)) % 360; //https://stackoverflow.com/questions/38803734/calculating-the-angle-between-two-vectors
					hatValue = Convert.ToSByte(Math.Round(Convert.ToDecimal(Angle).Map(0,360,0,8)));

					if(Dx==0 & Dy==0) {hatValue=-1;}
					unchecked{byteArray[2]=(byte)(sbyte)hatValue;}




					//Convert the controller byte buffer to a string array in the JSON message
					for(int i = 0; i<7;i++) {
						byteJSONbuffer.bytestringarray[i] = ToBinary(byteArray[i],8);
					}

					
					
					ws.Send(JsonSerializer.Serialize(byteJSONbuffer));

					buttonByteBuffer = 0;
					System.Threading.Thread.Sleep(10);

				}
				ws.Close();
			}
		}
	}
}
