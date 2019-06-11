# About OpenVIII

## What OpenVIII is?

OpenVIII is an open-source complete **Final Fantasy VIII** game engine rewrite from scratch powered in OpenGL making it possible to play vanilla game on wide wariety of platforms including **Windows 32/64 bit**, **Linux 32/64 bit** and even **mobile**!

## Why was OpenVIII started?

There are multiple answers to that questions: 
* Modding possibilities were limited due to engine 16-bit approach including static memory locations. The PC engine implementation was unstable when modders tried to replace objects, models, textures with higher details and resolutions. 
* Vanilla game engine used highly outdated technologies that forced the users to be able to play only on Windows. (On Linux user had to play with Wine)
* Extreme resolutions like 4K, 8K, Eyefinity were impossible
* FF Community contain a lot of amazing artists, but their works could not be imported into the game due to complexity and object handling- we wanted to change that and allow modders to be able to replace content using popular flie formats without worrying about poly count, BPP, palettes and more
* The 20th birthday of the game resulted in almost no attention from the producer. All of the major games in the franchise were ported to other platforms leaving FFVIII the only game that was missing. The company didn't address any of the questions about leaving one of their child behind. Just recently the Final Fantasy VIII Remastered was announced finally keeping up with all the other games. This project was started long before that and is part of our love to the game.

## Is OpenVIII free? Is it legal?

Yes. This project is free and open-source. It means you are able to look, modify and run the code without any charges. This project contains ONLY code that we wrote on our own. We CAN'T and we DON'T host any kind of asset that is licensed by **Square Enix**. We are in no way affiliated and/or related to the IP holders. This projects **REQUIRES** a full, legal copy of the Final Fantasy VIII game. This project is only our engine made to work with original files of the game. We do not support piracy. All our work is voluntary and we do not earn any money from it- no matter if directly or from donations. 

## Is Steam version enough to play with OpenVIII? Can I use PC2000 version?

Yes. The Steam release is the best option to be used with OpenVIII. Other than that we also fully support the first PC release.

## Which game languages are available?

Any that your game works on. OpenVIII is a game engine, we do not have any kind of text in our assets. Everything is read directly from your game catalogue. If you installed French version of the game, then OpenVIII will display the game in French.

## What are the main NEW features of OpenVIII?

Compared to vanilla Steam release so far we introduced these features:

* Unlimited resolution - You can play in 64x64 up to 8k if your monitor supports it
* Linux native support - You no longer have to play with Wine emulator. OpenVIII based on OpenGL delivers the game in native code- with direct support for your platform drivers. 
* [WIP] Current known graphical mods support - We are succesfully introducing current vanilla mods supports including GUI/Menu overhaul, music replacement mods and more! We are in direct contact with all the mods authors planning how can we integrate their work
* [WIP] Current gen audio - You can now play DirectX Music segments on all platforms (WIP); We also natively support loopable OGG music replacements making it possible to change any music you want by simply drag&drop operation. Thanks to OpenVIII you will be able to change, edit, export to Midi .SGT segments, replace soundfonts and much more!
* FFMPEG video module - Due to FFMPEG integration we are able to support wide amount of video formats. You are able to play .mp4, .bik and even replace the videos with your own without worrying about the codec. 
* No more frame limits - All actions are no more tied to specific framerate. You can now enjoy the game with fully real-time animation blending making it possible to play the game in unlimited framerate making every motion smooth. [SEE IN ACTION!](https://www.youtube.com/watch?v=J9v_CpdkkPY)


## What are planned features for OpenVIII?

Our main objective is to finish every single module to make the game fully playable from start to the end. Our second objective is to deliver the best and most user-friendly modding approach allowing non-tech users to be able to do what was always impossible. The main future features we want to introduce so far are:

* Shader Model 3 - Every 3D scene would be enriched wit the possibility of real-time shadows, lightning and materials containing normal and specular maps.
* Easy assets modding - with specialized toolking non-tech user would be able to replace ANY kind of game asset with a simple drag&drop operations not worrying about file format. We want the artists to be able to replace for example whole battle stage with high-poly FBX/OBJ exported without any addons straight from 3D modelling softwares. Same behaviour applicable to materials, textures and 2D art- no more complex file systems, worrying about palettes, proper BPP...
* Android/iOS support- OpenVIII so far supports variety of inputs: keyboard, gamepads out-of-box and even playing with mouse! With that approach we would be able to introduce the smartphone version much faster


# Current progress

#### These screenshots were taken on Linux machine in 1280x720 resolution. Keep in mind that it doesn't show everything that was done. There's A LOT more including video support, music support, audio support, cards, in-game menu and many more! Screenshots taken at 11/06/2019
![Load menu](https://i.postimg.cc/RVSzcGnm/Screenshot-from-2019-06-11-11-45-14.png)![Galbadian soldier](https://i.postimg.cc/rwzXHmJY/Screenshot-from-2019-06-11-11-44-19.png)
![Fight at Balamb](https://i.postimg.cc/5NsWpvwC/Screenshot-from-2019-06-11-11-44-38.png)![World with worldmap](https://i.postimg.cc/2SJRND9j/Screenshot-from-2019-06-11-11-46-24.png)


# Contact

If you need to contact us- please do so by making an issue on the github page or getting in touch via github profiles of project contributors. 
