<p align="center">
  <img src="https://cdn.discordapp.com/attachments/552838120895283210/588452586110058509/Open_VIII_Logo-MCINDUS-2.png">
</p>

# OpenVIII
Open source VIII engine implementation in C#

Check our website to find screenshots and more info at: https://makipl.github.io/OpenVIII/

## Getting started (Windows)

Requirements: MonoGame + Visual Studio

1. Download and install **Visual Studio 2017** (2015 is not supported) and **NET Framework 4.6**

2. Clone the repository:

`git clone https://github.com/MaKiPL/OpenVIII.git`

3. Download and install the **development build** of MonoGame:
[MonoGame for Visual Studio](http://teamcity.monogame.net/repository/download/MonoGame_PackagingWindows/latest.lastSuccessful/MonoGameSetup.exe?guest=1)

4. If you get an "Unable to load DLL 'FreeImage'" error, download and install:
[Visual C++ Redistributable Packages for Visual Studio 2013](https://www.microsoft.com/en-us/download/details.aspx?id=40784)

5. In Visual Studio 2017, while the solution is open, right click on Core > Properties and make sure Core project targets .NET Framework 4.6. Visual Studio might try to target the newest available .NET Framework on your machine by default which will mess next step.

6. Go to Tools > NuGet Package Manager > Manage NuGet Packages for solution. Make sure the required packages are installed and everything is up to date. There should be a notice on this screen if a package isn't installed. There will be a number in a box next to Updates if there are out of date packages.

7. Make sure you add the Final Fantasy VIII path to the array at `WindowsGameLocationProvider.cs:36`. On Windows the code tries to detect the install path via the registry. If it fails, it'll fall back to the array.

8. That's all. You can now compile the executable.

## Getting started (Linux/Mono) [Tested on Ubuntu]

1. Make sure your Linux is up to date. Due to the FFmpeg dependency, we require Ubuntu Cosmos.

`sudo apt-get update`

`sudo apt-get upgrade`

2. Install the latest version of MonoDevelop 

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

7. Open `FF8.sln` with MonoDevelop

8. If you encounter missing `Microsoft.XNA...` then please open NuGet package **Edit/Packages/Add Package**:

`MonoGame.Framework.DesktopGL`

`MonoGame.Framework.DesktopGL.Core`

`MonoGame.Framework.OpenGL`

9. Make sure you add the Final Fantasy VIII path to the array at `LinuxGameLocationProvider.cs:18`

10. That's all. You can now compile the executable.


## Command-Line Arguments
1. Enable log file.

`log=true`

2. Force a FF8 Directory Path.

`dir="Path_To_FF8"`

3. Force a different data folder.

`data="Path_To_Data"`

4. Force language code.

`lang=xx`


## Development guidelines

1. This project is currently in active development, therefore you can make new pull requests directly to main branch. 

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
I'd like to thank everyone involved in this project!

