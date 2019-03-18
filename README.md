# OpenVIII
Open source VIII engine implementation in C#


Progress:
Current state- DEBUG ONLY; pre-prototype, may contain DIRTY code and file reverse engineering work in progress with a lot of trash. 
Such DEBUG modules are designed to be rewritten into stable version, without trash, test functions and keeping in mind all future improvements.

=Are debug modules fully featured?

-Yes, they are. They are debug just to test the functions and work with reverse engineering, but they should support all functionalities.

Legend:

NAME OF MODULE

PROGRESS BAR PROGRESS PERCENTAGE  -> WHAT'S REMAINING TO DO


## PROGRESS IN-DEV PROTOTYPES

MODULE OVERTURE

⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛ 100% 

MODULE MAIN MENU

⬛⬛⬛⬛⬛⬛⬜⬜⬜⬜ 60%  -> LOAD MENU; SAVEGAME PARSING

MODULE IN-GAME MENU

⬜⬜⬜⬜⬜⬜⬜⬜⬜⬜ 0%   -> EVERYTHING

MODULE BATTLE

⬛⬛⬛⬛⬜⬜⬜⬜⬜⬜ 40%  -> CAMERA SEQUENCE;ENTITY RENDERING; AI; WHOLE BATTLE CODE

MODULE TRIPLE TRIAD

⬜⬜⬜⬜⬜⬜⬜⬜⬜⬜ 0%   -> EVERYTHING

MODULE FIELD

⬛⬛⬛⬛⬜⬜⬜⬜⬜⬜ 40%  -> SCRIPT; ENTITY RENDERING; BACKGROUND FIXES; BACKGROUND ANIMATION; 

MODULE FMV

⬛⬛⬛⬛⬜⬜⬜⬜⬜⬜ 45%  -> AUDIO; BINK IMPLEMENTATION

MODULE WORLD MAP

⬛⬛⬛⬛⬛⬜⬜⬜⬜⬜ 50%   -> CHARACTER; OBJECT OF INTEREST; WM2FIELD; RAGNAROK/VEHICLES; ENCOUNTERS


## Getting started (Windows)

Requirements: MonoGame + Visual Studio

1. Download and install **Visual Studio 2017** (2015 is not supported) and **NET Framework 4.6**

2. Clone whole repository:

`git clone https://github.com/MaKiPL/OpenVIII.git`

3. Download and install **development build** of MonoGame:
[MonoGame for Visual Studio](http://teamcity.monogame.net/repository/download/MonoGame_PackagingWindows/latest.lastSuccessful/MonoGameSetup.exe?guest=1)

4. Open solution FF8.sln

5. Edit game path (so far you have to type it manually) at `Memory.cs:67`:

`public const string FF8DIR = @"D:\SteamLibrary\steamapps\common\FINAL FANTASY VIII\Data\lang-en\";`

6. That's all. You can proceed with compilation. 

## Getting started (Linux/Mono) [Tested on Ubuntu]

1. Make sure your Linux is up to date

`sudo apt-get update`

`sudo apt-get upgrade`

2. Install latest MonoDevelop 

[MonoDevelop for Linux](https://www.monodevelop.com/download/#fndtn-download-lin)

3. Install Mono if needed

`sudo apt-get install Mono-complete Mono-devel`

4. Download MonoGame for Linux

[MonoGame for Linux development build](http://teamcity.monogame.net/repository/download/MonoGame_PackageMacAndLinux/latest.lastSuccessful/Linux/monogame-sdk.run?guest=1)

5. Set chmod +x and run the MonoGame installer as sudo

`chmod +x monogame-sdk.run`

`sudo ./monogame-sdk.run`

6. Clone the repository

`git clone https://github.com/makipl/openviii`

7. Open FF8.sln with MonoDevelop

8. If you encounter missing `Microsoft.XNA...` then please open NuGet package **Edit/Packages/Add Package**:

`MonoGame.Framework.DesktopGL`

`MonoGame.Framework.DesktopGL.Core`

`MonoGame.Framework.OpenGL`

9. That's all. You can now compile the executable. Make sure you type path at `Memory:cs:67`

## Development guidelines

1. Project is in in-dev prototype, therefore you can make new pull requests directly to main branch. 

2. ??

PS. Required FFmpeg dlls.
avcodec-58.dll
avdevice-58.dll
avfilter-7.dll
avformat-58.dll
avutil-56.dll
postproc-55.dll
swresample-3.dll
swscale-5.dll

