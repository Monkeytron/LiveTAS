using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Deployment.Application;

// Important : always use invariant culture when writing/reading numbers from the file.

namespace LiveTAS
{
    static class Program
    {
        public static int messageWait = 700;


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Console.Title = $"LiveTAS {System.Reflection.Assembly.GetEntryAssembly().GetName().Version}";
            Thread.CurrentThread.Priority = ThreadPriority.Highest; // High priority makes timing more accurate but is also more CPU intensive.

            try
            {
                string externalFilePath = File.ReadAllText("ExternalFilePath");
                Directory.SetCurrentDirectory(externalFilePath);
            }
            catch
            {
                while (true)
                {
                    Console.WriteLine("Please enter the full filepath of the folder you would like to store the program data in.");
                    string externalFilePath = Console.ReadLine();
                    if (Directory.Exists(externalFilePath))
                    {
                        File.WriteAllText("ExternalFilePath", externalFilePath);
                        Directory.SetCurrentDirectory(externalFilePath);
                        File.WriteAllText("Instructions.txt", Properties.Resources.Instructions);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("That is not an existing folder on this computer.");
                        Console.WriteLine("Please ensure you have correctly copied the whole file path.");
                    }
                }
                Console.WriteLine("File location successfully updated!");
                Thread.Sleep(messageWait);
            }

            Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\Input Sequences");
            Directory.SetCurrentDirectory(Directory.GetCurrentDirectory() + @"\Input Sequences");

            Console.Clear();
            Console.WriteLine("Hello! Welcome to the LiveTAS console window.");

            try
            {
                while (true)
                {
                    Console.WriteLine("What would you like to do?");
                    Thread.Sleep(messageWait);
                    Console.WriteLine("1) Play an input sequence from a file");
                    Thread.Sleep(messageWait);
                    Console.WriteLine("2) Record a series of human inputs");
                    Thread.Sleep(messageWait);
                    Console.WriteLine("3) Exit the program");

                    switch (Methods.GetChoice("1", "2", "3"))
                    {
                        case "1":
                            PlayAnInputFile();
                            Console.Clear();
                            //PlayAnInputFile();
                            break;
                        case "2":
                            RecordInputs();
                            Console.Clear();
                            //RecordInputs();
                            break;
                        case "3":
                            Console.WriteLine("Press any key to close the program.");
                            Console.ReadKey();
                            return;
                    } // Choosing which mode of the program to use.
                }//This is the main program loop.
            }
            catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("A fatal error occured.");
                Console.WriteLine(e);
                Console.ResetColor();
                Thread.Sleep(messageWait);
                Console.WriteLine("Press any key to exit.");
                Console.ReadLine();
                Environment.Exit(0x544153); // Yeah idk why I did this. If you're wondering it's the VirtualKey codes for T A S.
            }


            Console.WriteLine("Press any key to close the program.");
            Console.ReadKey(true);

        }

        static void PlayAnInputFile()
        {
            Thread.Sleep(messageWait);
            Console.Clear();

            Console.WriteLine("Input playback mode selected.");
            Thread.Sleep(messageWait);

            InputSender sender = new InputSender();

            while (true)
            {
                string sequenceName = "";
                while (true)
                {
                    try
                    {
                        Console.WriteLine("Enter input sequence file name here.");
                        sequenceName = Methods.FindFile(Console.ReadLine(), ".txt", new List<string>());
                        break;
                    }
                    catch
                    {
                        Console.WriteLine("No such file found.");
                        Console.WriteLine($"Please check you have spelt it correctly, and it is in the folder {Directory.GetCurrentDirectory()}");
                    }
                }

                if (sender.ParseFile(sequenceName))
                {
                    Console.WriteLine(sender.playbackDescription);
                    Console.WriteLine(sender.playbackInstructions);
                    bool sameSequence = true;
                    while (sameSequence)
                    {
                        sender.Playback();

                        Console.WriteLine("\nWould you like to:");
                        Console.WriteLine($"A) Play the input sequence {sequenceName} again?");
                        Console.WriteLine("B) Play a different input sequence?");
                        Console.WriteLine("C) Return to the main menu?");

                        switch (Methods.GetChoice("A", "B", "C"))
                        {
                            case "A":
                                Console.WriteLine("Playing same sequence again.");
                                sameSequence = true;
                                continue;
                            case "B":
                                Console.Clear();
                                Console.WriteLine("Playing a new input sequence.");
                                sameSequence = false;
                                continue;
                            case "C":
                                return;
                        } // Get the user's choice.
                    }
                }
                else return;
            }
        }

        static void RecordInputs()
        {
            int frameLength = 30;

            Thread.Sleep(messageWait);
            Console.Clear();

            Console.WriteLine("Input recording mode selected");
            Thread.Sleep(messageWait);

            string fileName = "";

            Console.WriteLine("What would you like the input sequence to be called?");
            while (true)
            {
                fileName = Console.ReadLine().Trim();
                fileName = fileName.EndsWith(".txt") ? fileName : fileName + ".txt";

                if (File.Exists(fileName))
                {
                    Console.WriteLine("A file with that name already exists. Do you want to overwrite it? y/n");
                    if (Console.ReadLine().Trim().ToLower().StartsWith("n"))
                    {
                        Console.WriteLine("What would you like the file to be called?");
                        continue;
                    }
                }
                try
                {
                    File.WriteAllText(fileName,"");
                    break;
                }
                catch
                {
                    Console.WriteLine("That file name is invalid. Please try again with a valid name");
                    continue;
                }
            }

            Console.WriteLine("Would you like to constantly record mouse position? y/n");
            Console.WriteLine("The alternative is to only record mouse position while a mouse button is pressed.");
            bool constantMouse = Console.ReadLine().Trim().ToLower().StartsWith("y");
            POINT screenSize = new POINT { x = Screen.PrimaryScreen.Bounds.Width, y = Screen.PrimaryScreen.Bounds.Height };
            (double, double) screenToFileScale = (65535.0 / screenSize.x, 65535.0 / screenSize.y);

            VirtualKey stopStartKey;
            Console.WriteLine("What should the stop/start key be?");
            while (true)
            {
                try
                {
                    stopStartKey = Methods.ParseKey(Console.ReadLine());
                    break;
                }
                catch
                {
                    Console.Write("That is not a recognised key code. Please try again.");
                }
            }

            List<VirtualKey> recognisedKeys = new List<VirtualKey>();
            for(VirtualKey i = 0; (int)i < 0xff; i = (VirtualKey)((int)i+1))
            {
                if (Enum.IsDefined(typeof(VirtualKey), i))
                {
                    if(i != VirtualKey.SHIFT && i != VirtualKey.MENU && i != VirtualKey.CONTROL)
                    {
                        recognisedKeys.Add(i);
                    }
                }
            }

            List<RecordFrame> frames = new List<RecordFrame>();
            POINT mousePos = new POINT();

            Console.WriteLine($"Press the stop/start key, {stopStartKey}, to start recording.");
            while ((Methods.GetAsyncKeyState((int)stopStartKey) & 0x8000) == 0)
            {
                Thread.Sleep(0);
            }
            Console.WriteLine("\nRecording begins in 5");
            Thread.Sleep(1000);
            Console.WriteLine(4);
            Thread.Sleep(1000);
            Console.WriteLine(3);
            Thread.Sleep(1000);
            Console.WriteLine(2);
            Thread.Sleep(1000);
            Console.WriteLine(1);
            Thread.Sleep(1000);
            Console.WriteLine("Press the start/stop key again when you want to stop recording.\n");

            for (int i = 0; i < 256; i++) Methods.GetAsyncKeyState(i); // Clears any keys pressed before recording actually starts
            Stopwatch s = Stopwatch.StartNew();

            while (true)
            {
                if (Methods.GetAsyncKeyState((int)stopStartKey) != 0) break;
                Methods.GetCursorPos(out mousePos);
                RecordFrame thisFrame = new RecordFrame(s.Elapsed,Methods.ScalePos(mousePos,screenToFileScale));
                foreach (VirtualKey key in recognisedKeys)
                {
                    ushort state = Methods.GetAsyncKeyState((int)key);
                    if (state != 0)
                    {
                        if(state != 0)
                        {
                            thisFrame.heldKeys.Add(key);
                        }
                    }
                }
                frames.Add(thisFrame);
                Methods.PreciseWait(new TimeSpan(0,0,0,0,frameLength*frames.Count)-s.Elapsed);
            }
            Console.WriteLine("Processing all recorded inputs, please wait...");

            List<string> lines = new List<string>();

            POINT mousePosition = new POINT();

            RecordFrame frame = new RecordFrame();

            for(int frameNum = 0; frameNum < frames.Count; frameNum++)
            {
                frame = frames[frameNum];
                if((frame.heldKeys.Any(i => Methods.mouseKeys.Contains(i))||constantMouse) && frame.scaledMousePos != mousePosition)
                {
                    mousePosition = frame.scaledMousePos;
                    lines.Add($"[{Methods.WriteTime(frame.timeStamp)}] MOVE({mousePosition.x},{mousePosition.y})");
                } // log mouse positions.
            }
            for(int framenum = 0; framenum < frames.Count; framenum++)
            {
                frame = frames[framenum];

                foreach (VirtualKey key in frame.heldKeys)
                {
                    TimeSpan firstFrame = frame.timeStamp;
                    TimeSpan lastFrame = frame.timeStamp;

                    int i = framenum + 1;
                    if (i == frames.Count)
                    {

                    }

                    else while (frames[i].heldKeys.Contains(key))
                        {
                            frames[i].heldKeys.Remove(key);
                            lastFrame = frames[i].timeStamp;
                            i++;
                            if (i == frames.Count) break;
                    }

                    lines.Add($"[{Methods.WriteTime(firstFrame)}] PRESS({key}) {'{'}{Methods.WriteTime(lastFrame - firstFrame)}{'}'}");
                }
            }

            lines.Sort((i, j) =>
            {
                return Methods.ParseTime(Methods.GetBetween(i, '[', ']')).CompareTo(Methods.ParseTime(Methods.GetBetween(j, '[', ']')));
            });

            lines.Insert(0, stopStartKey.ToString());
            Console.WriteLine("Write a short description about to be included in the first line of the file.");
            lines.Insert(0, Console.ReadLine().Trim());
            Console.WriteLine("Now, write one line of instruction to be given to the user of this sequence.");
            lines.Insert(1, Console.ReadLine().Trim());

            File.WriteAllLines(fileName, lines);

            Console.WriteLine("Your input sequence has been saved successfully! (Press enter)");
            Console.ReadLine();
            Thread.Sleep(700);
            Console.Clear();
        }
    }
}
