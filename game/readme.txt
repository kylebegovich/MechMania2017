Hey!

In this folder, you'll find a few executables for 3 different platforms (Windows, MacOS and Linux):

* On a Mac? Try opening mac.app
* On Windows? Try opening win.exe
* On Linux? Try running linux.x86 (on a 32 bit machine) or linux.x86_64 (on a 64 bit machine)

First, try the appropriate executable above. IF that doesn't work, come find us and we'll help.

If that does work, you now need to know how to run the game with YOUR scripts.

To do that, modify TEAM_RED_SCRIPT.cs provided so that the class name and references to the class are your team name instead of TEAM_RED_SCRIPT.

Feel free to also update the logic so you have your own ass kicking bot :)

Now, to run the game with your script against TEAM_BLUE_SCRIPT:

* On a Mac? 
  ./game/mac.app/Contents/MacOS/mac {{Your Team Name}} {{path to your script}} TEAM_BLUE_SCRIPT {{path to TEAM_BLUE_SCRIPT}}
* On Windows?
    cd game && win.exe {{Your Team Name}} {{path to your script}} TEAM_BLUE_SCRIPT {{path to TEAM_BLUE_SCRIPT}}
* On Linux?
    On 32 bit
      ./game/linux.x86 {{Your Team Name}} {{path to your script}} TEAM_BLUE_SCRIPT {{path to TEAM_BLUE_SCRIPT}}
    On 64 bit
      ./game/linux.x86_64 {{Your Team Name}} {{path to your script}} TEAM_BLUE_SCRIPT {{path to TEAM_BLUE_SCRIPT}}

Good luck!