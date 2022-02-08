# LiveTAS
Record, edit and play back a series of key presses and mouse movements.

LiveTAS is a tool for the windows OS that will play out a predefined series of key presses at specific intervals. It was originally intended to allow people to test what could be achieved in games if you could press keys perfectly - for example in a "Tool Assisted Speedrun" (hence the name). However, using a tool like this in online play, during competitions or to submit to a high score or speedrun leaderboard is ***considered cheating and can get you banned***. 

This program provides 2 functions:
#### Input recording mode
- Records all key presses, and optionally cursor position, at ~30fps
- Cursor position is always recorded while mouse buttons are held down.

#### Input playback mode
- Plays back a recorded "input sequence" of key presses and mouse movements by simulating those key presses for other programs.
- This will also work with recording files that have been edited (using a text editor), or even entirely user - generated files, provided they are in the correct format.
- In early testing, the inputs were consistently pressed at the right time to within 10 milliseconds. However, when your computer is also being used for other things this can cause the program to lag and 'miss' the input by much more, up to 100 milliseconds.

### FAQs

##### Q: How do I run LiveTAS on my computer?

A: Download the ZIP file of this program from github. Open the file inside it called "publish", and double-click on setup.exe. This will install the program onto your computer.
      You can then open LiveTAS from the icon on your desktop whenever you want.



##### Q: How do I update to the latest version?

A: The program should automatically update. If it is not working, you can always download and install the latest version from here?



##### Q: Where can I find the input recordings that LiveTAS has made on my computer?

A: When you first open LiveTAS, it will ask you to give it a filepath on your computer to store its data in.
      From then on, all recordings will be recorded to a folder called Input Sequences in that folder.
      This is also where the program will expect input files to be when it is playing them back.
      
      
      
##### Q: I want to edit a file of recorded inputs. What does all the writing in it mean?

A: In the same folder as the Input Sequences folder, there should be a file called Instructions.txt . This contains an explanation of the format inputs are recorded in.
      If you still have questions, feel free to ask about it in issues and I'll clarify for you!
