using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace LiveTAS
{
    static class Methods
    {

        public static string GetBetween(string s, char start, char end)
        {
            string newString = s;
            if (newString.Contains(start))
            {
                newString = newString.Substring(newString.IndexOf(start) + 1);
            }
            if (newString.Contains(end))
            {
                newString = newString.Substring(0, newString.IndexOf(end));
            }
            return newString;
        }
        // Returns the section of the string between the first instances of each character.

        public static string GetChoice(params string[] options)
        {
            string choice = Console.ReadLine().Trim();

            int optionNum = 0;

            while (true)
            {
                if (optionNum < options.Length)
                {
                    if (choice.Equals(options[optionNum].Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return options[optionNum];
                    }
                    else optionNum++;
                }

                else
                {
                    Console.Write("Please enter one of ");
                    foreach (string option in options) Console.Write($"{option},");
                    Console.Write("\b.\n");

                    choice = Console.ReadLine().Trim();
                    optionNum = 0;
                }
            }
        }
        // Gets the user's response to a question asked before calling this function.
        // If the answer is not one of the pre-defined answers, promtps the user to try again.

        public static TimeSpan ParseTime(string s)
        {
            TimeSpan time = new TimeSpan(0, 0, int.Parse(s.Split(':','.',',')[0]), int.Parse(s.Split(':','.',',')[1]), int.Parse(s.Split(':', '.', ',')[2].PadRight(3,'0')));
            return time;
        }
        // Time in the format MM:SS.mmm or MM:SS,mmm etc
        public static string WriteTime(TimeSpan t)
        {
            return $"{(int)t.TotalMinutes}:{t.Seconds}.{t.Milliseconds.ToString().PadLeft(3,'0')}";
        }
        public static string FindFile(string fileName, string extension, List<string> subFolders)
        {
            if (!fileName.EndsWith(extension)) fileName += extension;
            
            if (File.Exists(fileName)) return fileName;
            else
            {
                foreach(string folder in subFolders)
                {
                    fileName += @"\" + folder;
                    if (File.Exists(fileName)) return fileName;
                }
            }

            throw new Exception("No such file found.");
        }

        /// <summary>
        /// Pauses running of this thread, using Thread.Sleep and then Thread.SpinWait in conjunction with Stopwatch for high accuracy.
        /// The timing is never early and (on a non-busy CPU) within 10 ms after the desired time.
        /// However, it is relatively CPU intensive and performs badly at near-100% CPU usage, so if the timing is looking a bit ඞ blame CPU usage.
        /// </summary>
        /// <param name="waitTime"> The time to pause running for. Smaller wait times tend to be less accurate and use more CPU while waiting.</param>
        public static void PreciseWait(TimeSpan waitTime)
        {
            if (waitTime.TotalMilliseconds <= 0) return;
            Stopwatch s = Stopwatch.StartNew();
            TimeSpan stage1delay = new TimeSpan(0,0,0,0,20); // This is the minimum Thread.Sleep time.
            // Smaller values mean less CPU usage as longer is spent sleeping.
            // However if the value is too small the thread is likely to "oversleep" especially at high CPU usage, making the accuracy plummet.
            int stage2delay = 100;

            if (waitTime - s.Elapsed > stage1delay)
            {
                Thread.Sleep(waitTime - s.Elapsed - stage1delay);
            }

            while (s.Elapsed < waitTime)
            {
                Thread.SpinWait(stage2delay);
            }
        }

        public static void ClearKeys(List<VirtualKey> toClear)
        {
            List<Input> raiseKeys = new List<Input>();
            foreach (VirtualKey vkey in toClear)
            {
                raiseKeys.Add(new Input
                {
                    type = (int)InputType.Keyboard,
                    u = new InputUnion
                    {
                        ki = KeyboardInput.KeyPress(vkey, false)
                    }
                });
            }

            InputSender.SendInput((uint)raiseKeys.Count(), raiseKeys.ToArray(), Marshal.SizeOf(typeof(Input)));
        }
        public static POINT ScalePos(POINT position, (double, double) scale)
        {
            return new POINT { x = (int)(position.x * scale.Item1),y = (int)(position.y * scale.Item2) };
        }

        public static VirtualKey ParseKey(string key)
        {
            string formattedKey = key.ToUpper().Trim();
            formattedKey = formattedKey.StartsWith("VK_") ? formattedKey.Remove(0, 3) : formattedKey;

            return (VirtualKey)Enum.Parse(typeof(VirtualKey), formattedKey,true);
        }


        public static List<VirtualKey> mouseKeys = new List<VirtualKey>() { VirtualKey.LBUTTON, VirtualKey.RBUTTON, VirtualKey.MBUTTON, VirtualKey.XBUTTON1, VirtualKey.XBUTTON2 };
        [DllImport("user32.dll")] public static extern IntPtr GetMessageExtraInfo();
        [DllImport("user32.dll")] public static extern uint MapVirtualKeyA(uint uCode, uint uMapType);
        [DllImport("user32.dll")] public static extern bool GetCursorPos(out POINT lppoint);
        [DllImport("user32.dll")] public static extern ushort GetAsyncKeyState(int vkey);
        [DllImport("user32.dll")] public static extern ushort GetKeyState(int vkey);
        [DllImport("user32.dll")] public static extern bool GetKeyboardState(byte[] lpKeyState);
    }
}
