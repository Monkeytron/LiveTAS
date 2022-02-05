using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Globalization;

namespace LiveTAS
{

    public class InputSender
    {
        [DllImport("user32.dll", SetLastError = true)] public static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);


        public bool ParseFile(string fullFilepath)
        {
            int linenum = 0;
            List<InputTime> inputTimes = new List<InputTime>();
            try
            {
                List<string> fileText = File.ReadAllLines(fullFilepath).Select(i => i.Trim()).ToList();

                if (true)
                {
                    playbackDescription = fileText[0];
                    fileText.RemoveAt(0); linenum++;
                    playbackInstructions = fileText[0];
                    fileText.RemoveAt(0); linenum++;
                }// Read in the first 2 lines of the file.
                 // These should contain: description of what the sequence will do, instructions for window positioning, instructions for what culture to set the keyboard to.


                if (true)
                {
                    fileText = fileText.Select(i => i.Split('#')[0].Trim()).ToList();//Comments within the file are indicated by a # before the comment, python style.
                    stopStartKey = Methods.ParseKey(fileText[0]);
                    fileText.RemoveAt(0);

                }// Read in the third line of the file: Escape key is: {Escape key}

                if (true)
                {
                    Console.WriteLine("Reading in input sequence now.");
                    Console.WriteLine($"Sequence length: {fileText.Count}");

                    while (fileText.Count > 0)
                    {
                        linenum++;
                        if (fileText[0].Trim().Length != 0)
                        {
                            inputTimes.AddRange(ParseInputLine(fileText[0]));
                        }
                        fileText.RemoveAt(0);
                    }

                }// Read in all remaining lines of the file: [Timestamp] Command(Argument) {Duration}
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error occured when reading line {linenum} of the input file.");
                Console.WriteLine(e);
                Console.ResetColor();
                Console.ReadKey(true);
                return false;
            }// Read in and parse the file.

            inputTimes.Sort((i, j) =>
            {
                return i.Timestamp.CompareTo(j.Timestamp);
            });
            allInputs = inputTimes.GroupBy(i => i.Timestamp).Select(i => ((TimeSpan)i.Key, i.ToArray().Select(j => j.input).ToArray())).ToList();
            // This line is a huge mess but it groups together all the inputs that occur at the same time. Hopefully.

            Console.WriteLine("[File successfully read in.]");
            return true;
        }

        public bool Playback()
        {

            Console.WriteLine("\nInput playback is ready to begin.");
            Console.WriteLine($"Press the start/stop key, {stopStartKey}, to start. Input playback will begin shortly afterwards.");

            while ((Methods.GetAsyncKeyState((int)stopStartKey) & 0x8000) == 0)
            {
                Thread.Sleep(0);
            }

            Console.WriteLine("\nStart/stop key registered - playback will begin in 5");
            Thread.Sleep(1000);
            Console.WriteLine(4);
            Thread.Sleep(1000);
            Console.WriteLine(3);
            Thread.Sleep(1000);
            Console.WriteLine(2);
            Thread.Sleep(1000);
            Console.WriteLine(1);
            Thread.Sleep(1000);
            Console.WriteLine("Press the start/stop key again to stop the playback at any time.\n");


            Stopwatch s = new Stopwatch();
            int nextInput = 0;

            List<VirtualKey> pressedButtons = new List<VirtualKey>();
            try
            {
                s.Restart();
                while (true)
                {
                    if ((Methods.GetAsyncKeyState((int)stopStartKey) & 0x8001) == 0)
                    {
                        if (allInputs[nextInput].Item1 <= s.Elapsed)
                        {
                            SendInput((uint)allInputs[nextInput].Item2.Length, allInputs[nextInput].Item2, Marshal.SizeOf(typeof(Input)));

                            foreach (Input button in allInputs[nextInput].Item2)
                            {
                                if (button.type == (int)InputType.Keyboard)
                                {
                                    VirtualKey vkey = (VirtualKey)button.u.ki.wVk;
                                    if (!pressedButtons.Contains(vkey))
                                        pressedButtons.Add(vkey);
                                }
                            } // Keep a track of which keys were pressed and unpress them all (to make sure we don't have a problem at the end).

                            nextInput++;
                            if (nextInput >= allInputs.Count) break;
                        }
                        Methods.PreciseWait(allInputs[nextInput].Item1 - s.Elapsed);
                    }
                    else
                    {
                        Console.WriteLine("Playback stopped.");
                        Methods.ClearKeys(pressedButtons);
                        return false;
                    }
                }
                Methods.ClearKeys(pressedButtons);
                s.Stop();
            }
            catch(Exception e)
            {
                Methods.ClearKeys(pressedButtons);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error occured during input file playback.");
                Console.WriteLine(e);
                Console.ResetColor();
                Thread.Sleep(Program.messageWait);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey(true);
                return false;
            } // Play out all the inputs.

            Console.WriteLine("Playback complete");
            Console.WriteLine($"Time taken: {s.Elapsed}");

            return true;
        }// Plays all inputs out at the specified framerate. Returns true if playback finished uninterrupted.

        public static List<InputTime> ParseInputLine(string inputLine)
        {
            TimeSpan timeStamp = Methods.ParseTime(Methods.GetBetween(inputLine, '[', ']'));
            string command = Methods.GetBetween(inputLine, ']', '(');
            string argument = Methods.GetBetween(inputLine, '(', ')');

            List<InputTime> parsed = new List<InputTime>();

            switch (command.Trim().ToUpper())
            {
                case "PRESS": // [timestamp] PRESS(VirtualKey) {duration}
                    TimeSpan duration = Methods.ParseTime(Methods.GetBetween(inputLine, '{', '}'));
                    VirtualKey key = Methods.ParseKey(argument);

                    parsed.Add(new InputTime
                    {
                        Timestamp = timeStamp,
                        input = new Input
                        {
                            type = (int)InputType.Keyboard,
                            u = new InputUnion
                            {
                                ki = KeyboardInput.KeyPress(key, true)
                            }
                        }
                    }); // Press that key at time "timestamp"...
                    parsed.Add(new InputTime
                    {
                        Timestamp = timeStamp + duration,
                        input = new Input
                        {
                            type = (int)InputType.Keyboard,
                            u = new InputUnion
                            {
                                ki = KeyboardInput.KeyPress(key, false)
                            }
                        }
                    }); // ...and release that key "duration" later.

                    return parsed;
                case "MOVE": // [timestamp] MOVE(x,y) {duration}
                    POINT position = new POINT { x = int.Parse(argument.Split(',')[0]), y = int.Parse(argument.Split(',')[1]) };
                    parsed.Add(new InputTime
                    {
                        Timestamp = timeStamp,
                        input = new Input
                        {
                            type = (int)InputType.Mouse,
                            u = new InputUnion
                            {
                                mi = MouseInput.MoveTo(position)
                            }
                        }
                    }); // Move the mouse to position (x,y)
                    return parsed;
                case "SCROLL":
                    int amount = int.Parse(argument);

                    parsed.Add(new InputTime
                    {
                        Timestamp = timeStamp,
                        input = new Input
                        {
                            type = (int)InputType.Mouse,
                            u = new InputUnion
                            {
                                mi = MouseInput.Scroll(amount)
                            }
                        }
                    }) ; // Scroll the mouse wheel
                    return parsed;
                default:
                    throw new ArgumentException($"Instruction {command} at time {timeStamp} is unknown.");

            }
        }

        public string playbackDescription;
        public string playbackInstructions;

        public VirtualKey stopStartKey;


        public List<(TimeSpan, Input[])> allInputs;
    }
}
