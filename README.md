# SwitchController
 
This is a project made to control a nintendo switch programmatically from a computer, without using any sort of homebrew software exploit. I wrote it a week before the first commit to this repo. I wrote it because i was isolating and I was getting very bored of staring at my walls. It is written with Arduino language because I needed to use an Arduino and don't know enough 'real' C to write it in that. There are portions written in Python because I wrote that bit next and python is the language I am most comfortable with. There is some code written in C# because I wrote that last and thought it might be a good idea to sharpen my skills there because I don't do much C#. The reason it uses 2 programs and a websocket connection is because I misplaced my USB to Serial Converter board and a Raspberry Pi was the most immediately accesible device with a serial bus on it.

## Dependencies
The Arduino sketch included **requires** [this modified version](https://github.com/Jas2o/Leonardo-Switch-Controller) of the Arduino Joystick library in order to function with a Nintendo Switch.  
The python programs included require the libraries [pyserial](https://pypi.org/project/pyserial/) and [websockets](https://pypi.org/project/websockets/)
## How to use
Install the libraries mentioned in the Dependencies section.  
Upload the sketch to an Arduino Leonardo.  
Connect pin 0 of the Leonardo to a USB to Serial converter such as an FTDI chip (If you have one lying about, another arduino can be used)  
Run the Python program "AruinoJoystickSocket.py"  
Enter the serial port of the Arduino.  
Build and run the C# Client.  
Enter the URL of the machine running the python program at port 6969. Example: `ws://192.168.0.420:6969`  
Fun happens.  

The machine running the C# client can also be the same one running the python program, in which case the URL is `ws://localhost:6969`. I would reccomend this as having them on 2 separate machines introduces latency.

## How it works
An Arduino Leonardo is connected to the Switch over USB. It pretends to be the Pokken Tournament controller for the Wii U, which was given compatibility with the Switch in a firmware update. It is recognised as a Pro Controller in the system and has the capability to send all button and stick inputs a Pro Controller can.  

The Arduino Leonardo listens on the serial port connected to pins 0 and 1 for signals from a program running on a computer.

The program hosts a websocket server listening for messages from a client, which it sends down the serial connection.

A websocket client takes user input and encodes it in the proper format to be sent to the other program.


## Notice for Non-Windows users

The included C# program uses the library `System.Windows.Input`, which is part of WPF and is therefore windows-only. Sorry.
It was the first result for a way to get the state of the keyboard from a console application without downloading another library. You'll have to make your own version of the program with some other input library.

### Personal Off-Topic Annoyances

I have, however found out something annoying. If you set the value UseWPF to true in your csproj, .NET 5 sets the Output Type to WinExe regardless of your project file. This disables the ability to open a console window and must have the value DisableWinExeOutputInference set to true if you want it to not do that. However, they are apparently rolling ths change back in the next version, .NET 6.