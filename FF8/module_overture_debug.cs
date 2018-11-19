using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace FF8
{
    internal class module_overture_debug
    {
        private static OvertureInternalModule internalModule = OvertureInternalModule._0InitSound;
        private static ArchiveWorker aw;
        private const string names = "name";
        private const string loops = "loop";

        private static Texture2D splashTex;
        private static Texture2D white;

        enum OvertureInternalModule
        {
            _0InitSound,
            _1WaitBeforeFirst,
            _2PlaySequence,
            _3SequenceFinishedPlayMainMenu
        }

        private static double internalTimer = 0.0f;
        private static bool bNames = true; //by default we are starting with names
        private static int splashIndex = 0;
        private static int splashName = 13;
        private static int splashLoop = 13;

        private static bool bFadingIn = true; //by default first should fade in, wait, then fire fading out and wait for finish, then loop
        private static bool bWaitingSplash = false;
        private static float fSplashWait = 0.0f;
        private static bool bFadingOut = false;

        private static float Fade = 0;

        internal static void Update()
        {
            switch(internalModule)
            {
                case OvertureInternalModule._0InitSound:
                    InitSound();
                    break;
                case OvertureInternalModule._1WaitBeforeFirst:
                    WaitForFirst();
                    break;
                case OvertureInternalModule._2PlaySequence:
                    SplashUpdate(ref splashIndex);
                    break;
            }
        }

        private static void WaitForFirst()
        {
            if (internalTimer > 6.0f)
            {
                internalModule++;
                Console.WriteLine("MODULE_OVERTURE: DEBUG MODULE 2");
            }
            internalTimer += (double)Memory.gameTime.ElapsedGameTime.Milliseconds/1000.0d;

        }

        private static void InitSound()
        {
            Memory.musicIndex = 072; //Overture
            init_debugger_Audio.PlayMusic();
            white = new Texture2D(Memory.graphics.GraphicsDevice, 4, 4, false, SurfaceFormat.Color);
            byte[] whiteBuffer = new byte[16];
            for (int i = 0; i < 16; i++)
                whiteBuffer[i] = 255;
            internalModule++;
        }

        internal static void Draw()
        {
            switch(internalModule)
            {
                case OvertureInternalModule._0InitSound:
                    break;
                case OvertureInternalModule._1WaitBeforeFirst:
                    break;
                case OvertureInternalModule._2PlaySequence:
                    Memory.graphics.GraphicsDevice.Clear(Color.Black);
                    DrawSplash();
                    break; //actually this is our entry point for draw;
                case OvertureInternalModule._3SequenceFinishedPlayMainMenu:
                    DrawLogo(); //after this ends, jump into main menu module
                    break;
            }
        }

        private static void DrawLogo()
        {
            //fade to white
            if (!bWaitingSplash)
                Memory.graphics.GraphicsDevice.Clear(Color.White);
            else
                Memory.graphics.GraphicsDevice.Clear(Color.Black);
            Memory.SpriteBatchStartAlpha();
            Memory.spriteBatch.Draw(splashTex, new Microsoft.Xna.Framework.Rectangle(0, 0, Memory.graphics.GraphicsDevice.Viewport.Width, Memory.graphics.GraphicsDevice.Viewport.Height),
                new Microsoft.Xna.Framework.Rectangle(0, 0, splashTex.Width, splashTex.Height)
                , Color.White * Fade);
            if(bFadingIn)
                Fade += Memory.gameTime.ElapsedGameTime.Milliseconds / 5000.0f;
            if (bFadingOut)
                Fade -= Memory.gameTime.ElapsedGameTime.Milliseconds / 2000.0f;
            if(Fade < 0.0f)
            {
                bFadingIn = true;
                ReadSplash(true);
                bFadingOut = false;
            }
            if (bFadingIn && Fade > 1.0f && !bWaitingSplash)
                internalTimer += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
             if (internalTimer > 5.0f)
            {
                bWaitingSplash = true;
                bFadingOut = true;
            }
            Memory.SpriteBatchEnd();
            if (bWaitingSplash && Fade < 0.0f)
            {
                init_debugger_Audio.StopAudio();
                Memory.module = Memory.MODULE_MAINMENU_DEBUG;
            }

        }

        private static void DrawSplash()
        {
            if (splashTex == null)
                return;
            Memory.SpriteBatchStartAlpha();      
            Memory.spriteBatch.Draw(splashTex, new Microsoft.Xna.Framework.Rectangle(0, 0, Memory.graphics.GraphicsDevice.Viewport.Width, Memory.graphics.GraphicsDevice.Viewport.Height),
                new Microsoft.Xna.Framework.Rectangle(0, 0, splashTex.Width, splashTex.Height)
                , Color.White * Fade);
            Memory.SpriteBatchEnd();
        }

        internal static void SplashUpdate(ref int splashIndex)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                init_debugger_Audio.StopAudio();
                Memory.module = Memory.MODULE_MAINMENU_DEBUG;
            }
            if(aw == null)
                aw = new ArchiveWorker(Memory.Archives.A_MAIN);
            if (splashTex == null)
                ReadSplash();
            if(bFadingIn)
            {
                Fade += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 2f;
                if (Fade > 1.0f)
                {
                    Fade = 1.0f;
                    bFadingIn = false;
                    bWaitingSplash = true;
                }
            }
            if(bFadingOut)
            {
                if (splashLoop+1 >= 0x0F && splashName >= 0x0F)
                {
                    bFadingIn = false;
                    bFadingOut = true;
                    bWaitingSplash = false;
                    internalTimer = 0.0f;
                    Fade = 1.0f;
                    internalModule++;
                    return;
                }
                Fade -= Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 2f;
                if(Fade< 0.0f)
                {
                    bFadingIn = true;
                    bFadingOut = false;
                    Fade = 0.0f;
                    splashIndex++;
                    if (bNames)
                        splashName++;
                    else splashLoop++;
                    if(splashIndex > 1)
                    {
                        bNames = bNames ? false : true; //can I simply XOR the bool in C#?
                        splashIndex = 0;
                    }

                    ReadSplash();
                }
            }
            if(bWaitingSplash)
            {
                if (bNames)
                {
                    if (fSplashWait > 4.8f)
                    {
                        bWaitingSplash = false;
                        bFadingOut = true;
                        fSplashWait = 0.0f;
                    }
                }
                else
                {
                    if (fSplashWait > 6.5f)
                    {
                        bWaitingSplash = false;
                        bFadingOut = true;
                        fSplashWait = 0.0f;
                    }
                }
                fSplashWait += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            }
             //loop 01-14 + name01-14;
        }


        //Splash is 640x400 16BPP typical TIM with palette of ggg bbbbb a rrrrr gg
        internal static void ReadSplash(bool bLogo = false)
        {
            if (!bLogo)
            {
                if (splashName > 0x0f) return;
                string[] lof = aw.GetListOfFiles();
                string fileName;
                if (bNames)
                    fileName = lof.Where(x => x.ToLower().Contains($"{names}{splashName.ToString("D2")}")).First();
                else
                    fileName = lof.Where(x => x.ToLower().Contains($"{loops}{splashLoop.ToString("D2")}")).First();

                byte[] buffer = ArchiveWorker.GetBinaryFile(Memory.Archives.A_MAIN, fileName);
                uint uncompSize = BitConverter.ToUInt32(buffer, 0);
                buffer = LZSS.DecompressAll(buffer, (uint)buffer.Length);
                int width = 640;
                int height = 400;

                splashTex = new Texture2D(Memory.graphics.GraphicsDevice, 640, 400, false, SurfaceFormat.Color);
                byte[] rgbBuffer = new byte[splashTex.Width * splashTex.Height * 4];
                int innerBufferIndex = 0;
                for (int i = 0; i < rgbBuffer.Length; i += 4)
                {
                    if (innerBufferIndex + 1 >= buffer.Length) break;
                    ushort pixel = (ushort)((buffer[innerBufferIndex + 1] << 8) | buffer[innerBufferIndex]);
                    byte red = (byte)((pixel) & 0x1F);
                    byte green = (byte)((pixel >> 5) & 0x1F);
                    byte blue = (byte)((pixel >> 10) & 0x1F);
                    red = (byte)MathHelper.Clamp((red * 8), 0, 255);
                    green = (byte)MathHelper.Clamp((green * 8), 0, 255);
                    blue = (byte)MathHelper.Clamp((blue * 8), 0, 255);
                    rgbBuffer[i] = red;
                    rgbBuffer[i + 1] = green;
                    rgbBuffer[i + 2] = blue;
                    rgbBuffer[i + 3] = 255;//(byte)(((pixel >> 7) & 0x1) == 1 ? 255 : 0);
                    innerBufferIndex += 2;
                }
                splashTex.SetData(rgbBuffer);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            else
            {
                
            }
        }
    }
}