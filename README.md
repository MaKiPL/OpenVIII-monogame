<p align="center">
  <img src="https://cdn.discordapp.com/attachments/552838120895283210/588452586110058509/Open_VIII_Logo-MCINDUS-2.png">
</p>

# OpenVIII
Open source VIII engine implementation in C#

Check our website to find screenshots and more info at: https://makipl.github.io/OpenVIII-monogame/

## Getting started (Windows)

Requirements: MonoGame + Visual Studio

1. Download and install **Visual Studio 2017** (2015 is not supported) and **NET Framework 4.6**

2. Clone the repository:

`git clone https://github.com/MaKiPL/OpenVIII-monogame.git`

3. Download and install the **development build** of MonoGame:
[MonoGame for Visual Studio](http://teamcity.monogame.net/repository/download/MonoGame_PackagingWindows/latest.lastSuccessful/MonoGameSetup.exe?guest=1)

4. If you get an "Unable to load DLL 'FreeImage'" error, download and install:
[Visual C++ Redistributable Packages for Visual Studio 2013](https://www.microsoft.com/en-us/download/details.aspx?id=40784)

5. In Visual Studio 2017, while the solution is open, go to Tools > NuGet Package Manager > Manage NuGet Packages for solution. Make sure the required packages are installed and everything is up to date. There should be a notice on this screen if a package isn't installed. There will be a number in a box next to Updates if there are out of date packages.

6. Make sure you add the Final Fantasy VIII path to the array at `WindowsGameLocationProvider.cs:36`. On Windows the code tries to detect the install path via the registry. If it fails, it'll fall back to the array.

7. That's all. You can now compile the executable.

## Getting started (Linux/Mono) [Tested on Ubuntu]

1. Make sure your Linux is up to date. Due to the FFmpeg dependency, we require Ubuntu Cosmos.
```sh
sudo apt update
sudo apt upgrade
```
2.Clone the repository

```sh
git clone https://github.com/MaKiPL/OpenVIII-monogame.git
cd OpenVIII-monogame
```
2. Install dependencies
```sh
## Installing ffmpeg and mono
sudo apt-get --assume-yes install nuget mono-complete mono-devel gtk-sharp3 zip ffmpeg
echo ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true | sudo debconf-set-selections
sudo apt-get --assume-yes install ttf-mscorefonts-installer
## Installing monogame 3.7.1
wget https://github.com/MonoGame/MonoGame/releases/download/v3.7.1/monogame-sdk.run
chmod +x monogame-sdk.run
sudo ./monogame-sdk.run --noexec --keep --target ./monogame
cd monogame
echo Y | sudo ./postinstall.sh
cd ..  
## Get missing Nuget Packages
nuget restore
```
3. Build from command line (optional):
```sh
msbuild $Env:APPVEYOR_BUILD_FOLDER/OpenGL$Env:operatingsystem /property:Configuration=Debug$Env:operatingsystem  /property:Platform=$Env:platform
#$Env:APPVEYOR_BUILD_FOLDER is just a folder you want to build to.
#$Env:operatingsystem = Linux
#$Env:platform = x86 or x64
#please customize the command for what you want to do.
```
4. Install an IDE
    1. Latest version of MonoDevelop
  [MonoDevelop for Linux](https://www.monodevelop.com/download/#fndtn-download-lin)
    2. The new version 3.8 of Monogame is recommending [VSCODE](https://code.visualstudio.com/docs/languages/csharp). It may work with Monogame 3.7.1. This [reddit](https://www.reddit.com/r/monogame/comments/cst49i/the_ultimate_guide_to_getting_started_with/) post talks about getting things working. Though it might be easier to stick with Monodevelop. I haven't had a chance to test vscode out.

5. Open `FF8.sln` with your IDE.

6. If you encounter missing `Microsoft.XNA...` then please open NuGet package **Edit/Packages/Add Package**:

`MonoGame.Framework.DesktopGL`

`MonoGame.Framework.DesktopGL.Core`

`MonoGame.Framework.OpenGL`

7. Make sure you add the Final Fantasy VIII path to the array at `LinuxGameLocationProvider.cs:18`
    * https://github.com/MaKiPL/OpenVIII-monogame/issues/181

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

