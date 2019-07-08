<p align="center">
  <img src="https://cdn.discordapp.com/attachments/552838120895283210/588452586110058509/Open_VIII_Logo-MCINDUS-2.png">
</p>

# OpenVIII
Open source VIII engine implementation in C#

Check our website to find screenshots and more info at: https://makipl.github.io/OpenVIII/

Progress:
Current state- DEBUG ONLY; pre-prototype, may contain DIRTY code and file reverse engineering work in progress with a lot of trash. 

## PROGRESS IN-DEV PROTOTYPES (may be different due to bugs/ insufficient testing)

MODULE OVERTURE

⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛ 100%

MODULE MAIN MENU

⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛ 100%

MODULE IN-GAME MENU

⬛⬛⬛⬛⬛⬛⬜⬜⬜⬜ 60%

MODULE BATTLE

⬛⬛⬛⬛⬛⬜⬜⬜⬜⬜ 55% 

MODULE TRIPLE TRIAD

⬛⬛⬜⬜⬜⬜⬜⬜⬜⬜ 20%   

MODULE FIELD

⬛⬛⬛⬛⬛⬛⬜⬜⬜⬜ 60% 

MODULE FMV

⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛ 100% 

MODULE WORLD MAP

⬛⬛⬛⬛⬛⬜⬜⬜⬜⬜ 50%


## Getting started (Windows)

Requirements: MonoGame + Visual Studio

1. Download and install **Visual Studio 2017** (2015 is not supported) and **NET Framework 4.6**

2. Clone whole repository:

`git clone https://github.com/MaKiPL/OpenVIII.git`

3. Download and install **development build** of MonoGame:
[MonoGame for Visual Studio](http://teamcity.monogame.net/repository/download/MonoGame_PackagingWindows/latest.lastSuccessful/MonoGameSetup.exe?guest=1)

4. If you get "Unable to load DLL 'FreeImage'", Download and install:
[Visual C++ Redistributable Packages for Visual Studio 2013](https://www.microsoft.com/en-us/download/details.aspx?id=40784)

5. In Visual Studio 2017, while the solution is open, goto Tools > NuGet Package Manager > Manage NuGet Packages for solution. Make sure the required packages are installed and everything is up to date. There should be a notice on this screen if a package isn't installed. There will be a number in a box next to Updates if there are out of date packages.

6. Make sure you add Final Fantasy VIII path to array at `WindowsGameLocationProvider.cs:36`. On Windows the code tries to detect the install path via the registry. If it fails, it'll fall back to the array.

7. That's all. You can now compile the executable.

## Getting started (Linux/Mono) [Tested on Ubuntu]

1. Make sure your Linux is up to date. Due to FFmpeg dependency we require Ubuntu Cosmos

`sudo apt-get update`

`sudo apt-get upgrade`

2. Install latest MonoDevelop 

[MonoDevelop for Linux](https://www.monodevelop.com/download/#fndtn-download-lin)

3. Install Mono if needed

`sudo apt-get install mono-complete mono-devel`

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

9. Make sure you add Final Fantasy VIII path to array at `LinuxGameLocationProvider.cs:18`

10. That's all. You can now compile the executable.

## Development guidelines

1. Project is in in-dev prototype, therefore you can make new pull requests directly to main branch. 

2. ??

PS. Required FFmpeg dlls. (available on Ubuntu Cosmos via `sudo apt-get install ffmpeg`)
<br/>
* avcodec-58.dll
* avdevice-58.dll
* avfilter-7.dll
* avformat-58.dll
* avutil-56.dll
* postproc-55.dll
* swresample-3.dll
* swscale-5.dll
<br/>
I'd like to thanks everyone involved in this project!
