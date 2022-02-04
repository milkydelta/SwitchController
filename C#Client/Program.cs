using System;
using System.Collections.Generic;
using WebSocketSharp;
using System.Windows.Input;
using System.Text.Json;



namespace CustomExtensions
{
	public static class ExtensionMethods
	{
//The Arduino map() function implemented as an Extension Method for decimal. https://www.arduino.cc/reference/en/language/functions/math/map/
		public static decimal Map(this decimal value, decimal fromSource, decimal toSource, decimal fromTarget, decimal toTarget)
		{
			return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
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
		public static string ToBinary(UInt64 myValue)
		{
			unchecked
			{
				Int64 cheeseStrategy = (long)myValue;
				return Convert.ToString(cheeseStrategy, 2).PadLeft(64 - 8, '0');
			}
		}


		
		//Initialise the dictionary for which keyboard keys correspond
		//to which controller buttons in the bit message.
		static Dictionary<System.Windows.Input.Key, UInt64> Key2BytesDict = new Dictionary<System.Windows.Input.Key, UInt64>();
		static void AddToDict()
		{
			// Key2BytesDict.Add(Key, Convert.ToInt64(Math.Pow(2, Position_In_Byte+(8*Byte))));
			//Where Position_in_Byte and Byte are counted from zero, from the left
			Key2BytesDict.Add(Key.S, 0b00000001_00000000_00000000_00000000_00000000_00000000_00000000); //Y BUTTON
			Key2BytesDict.Add(Key.Z, Convert.ToUInt64(Math.Pow(2, 1 + (8 * 6)))); //B BUTTON
			Key2BytesDict.Add(Key.X, Convert.ToUInt64(Math.Pow(2, 2 + (8 * 6)))); //A BUTTON
			Key2BytesDict.Add(Key.C, Convert.ToUInt64(Math.Pow(2, 3 + (8 * 6)))); //X BUTTON

			Key2BytesDict.Add(Key.A, Convert.ToUInt64(Math.Pow(2, 4 + (8 * 6))));//L
			Key2BytesDict.Add(Key.D, Convert.ToUInt64(Math.Pow(2, 5 + (8 * 6))));//R

			Key2BytesDict.Add(Key.Q, Convert.ToUInt64(Math.Pow(2, 6 + (8 * 6)))); //ZL
			Key2BytesDict.Add(Key.E, Convert.ToUInt64(Math.Pow(2, 0 + (8 * 5)))); //ZR

			Key2BytesDict.Add(Key.Back, Convert.ToUInt64(Math.Pow(2, 1 + (8 * 5)))); //-
			Key2BytesDict.Add(Key.Enter, Convert.ToUInt64(Math.Pow(2, 2 + (8 * 5)))); //+

			Key2BytesDict.Add(Key.LeftShift, Convert.ToUInt64(Math.Pow(2, 3 + (8 * 5)))); //Left Stick Click
			Key2BytesDict.Add(Key.LeftCtrl, Convert.ToUInt64(Math.Pow(2, 4 + (8 * 5)))); //Right Stick Click

			Key2BytesDict.Add(Key.Home, Convert.ToUInt64(Math.Pow(2, 5 + (8 * 5)))); //Home
			Key2BytesDict.Add(Key.End, Convert.ToUInt64(Math.Pow(2, 6 + (8 * 5)))); //Capture

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
				UInt64 byteBuffer = 0;
				JSONSwitchMessage byteJSONbuffer = new JSONSwitchMessage();
				byteJSONbuffer.type = "switchArdData";
				byteJSONbuffer.bytestringarray = new string[] { "", "", "", "", "", "", "" };
				string binaryStringHolder = "";

				while (true)
				{
					if (Keyboard.IsKeyDown(Key.Escape)) { break; }
					
					//Add all pressed buttons to the buffer for transmission
					foreach (KeyValuePair<Key, UInt64> DictByte in Key2BytesDict)
					{
						if ((Keyboard.GetKeyStates(DictByte.Key) & KeyStates.Down) > 0)
						{
							byteBuffer += DictByte.Value;
						}
					}

					
					//convert states of joystick buttons to axes and add them to the buffer
					//must be done in unchecked space to convert the bits of a negative number to the type of unsigned
					//so that they may be added to the bytebuffer without affecting any bits to the left.
					//this should be changed to something else, perhaps an array of byte integers instead of one big UInt64
					unchecked
					{
						if ((Keyboard.GetKeyStates(Key.Left) & KeyStates.Down) > 0) { byteBuffer += (UInt32)((byte)((sbyte)-126)) << (8 * 3); }
						else if ((Keyboard.GetKeyStates(Key.Right) & KeyStates.Down) > 0) { byteBuffer += ((sbyte)127 << (8 * 3)); }

						if ((Keyboard.GetKeyStates(Key.Up) & KeyStates.Down) > 0) { byteBuffer += ((byte)((sbyte)-126)) << (8 * 2); }
						else if ((Keyboard.GetKeyStates(Key.Down) & KeyStates.Down) > 0) { byteBuffer += ((sbyte)127 << (8 * 2)); }

						if ((Keyboard.GetKeyStates(Key.J) & KeyStates.Down) > 0) { byteBuffer += ((byte)((sbyte)-126)) << (8); }
						else if ((Keyboard.GetKeyStates(Key.L) & KeyStates.Down) > 0) { byteBuffer += (((sbyte)127) << (8 * 1)); }

						if ((Keyboard.GetKeyStates(Key.I) & KeyStates.Down) > 0) { byteBuffer += (byte)((sbyte)-126); }
						else if ((Keyboard.GetKeyStates(Key.K) & KeyStates.Down) > 0) { byteBuffer += ((sbyte)127 << (0)); }
					}

					//Get value of dpad keys and convert to two axes before finding HAT value by angle between vectors
					int dpadUp=Convert.ToInt32((Keyboard.GetKeyStates(Key.T) & KeyStates.Down) > 0);
					int dpadDown=Convert.ToInt32((Keyboard.GetKeyStates(Key.G) & KeyStates.Down) > 0);
					int dpadLeft=Convert.ToInt32((Keyboard.GetKeyStates(Key.F) & KeyStates.Down) > 0);
					int dpadRight=Convert.ToInt32((Keyboard.GetKeyStates(Key.H) & KeyStates.Down) > 0);
					
					var Dx = dpadRight-dpadLeft;
					var Dy = dpadUp-dpadDown;
					double Angle = (360 + Math.Atan2(Dx, Dy) * (180 / Math.PI)) % 360; //https://stackoverflow.com/questions/38803734/calculating-the-angle-between-two-vectors
					SByte hatValue = Convert.ToSByte(Math.Round(Convert.ToDecimal(Angle).Map(0,360,0,8)));

					if(Dx==0 & Dy==0) {hatValue=-1;}
					unchecked{byteBuffer += (UInt64)((byte)(sbyte)hatValue) << (8 * 4);}




					//Convert the controller byte buffer to a string array in the JSON message
					binaryStringHolder = ToBinary(byteBuffer);
					byteJSONbuffer.bytestringarray[0] = binaryStringHolder.Substring(0, 8);
					byteJSONbuffer.bytestringarray[1] = binaryStringHolder.Substring(8, 8);
					byteJSONbuffer.bytestringarray[2] = binaryStringHolder.Substring(16, 8);
					byteJSONbuffer.bytestringarray[3] = binaryStringHolder.Substring(24, 8);
					byteJSONbuffer.bytestringarray[4] = binaryStringHolder.Substring(32, 8);
					byteJSONbuffer.bytestringarray[5] = binaryStringHolder.Substring(40, 8);
					byteJSONbuffer.bytestringarray[6] = binaryStringHolder.Substring(48, 8);

					
					
					ws.Send(JsonSerializer.Serialize(byteJSONbuffer));

					byteBuffer = 0;
					System.Threading.Thread.Sleep(10);

				}
				ws.Close();
			}
		}
	}
}
