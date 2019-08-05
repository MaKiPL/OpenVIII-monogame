using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenVIII.Encoding.Tags;
using System;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public static class Module_overture_debug
    {
        private static OverturepublicModule publicModule = OverturepublicModule._4Squaresoft;
        private static ArchiveWorker aw;
        private const string names = "name";
        private const string loops = "loop";

        private static Texture2D splashTex = null;
        private static Texture2D white = null;

        private enum OverturepublicModule
        {
            _0InitSound,
            _1WaitBeforeFirst,
            _2PlaySequence,
            _3SequenceFinishedPlayMainMenu,
            _4Squaresoft
        }

        private static double publicTimer;
        private static bool bNames = true; //by default we are starting with names
        private static int splashIndex, splashName = 1, splashLoop = 1;

        private static bool bFadingIn = true; //by default first should fade in, wait, then fire fading out and wait for finish, then loop
        private static bool bWaitingSplash, bFadingOut;
        private static float fSplashWait, Fade;

        public static void Update()
        {
            if (Input2.DelayedButton(FF8TextTagKey.Confirm) || Input2.DelayedButton(FF8TextTagKey.Cancel) || Input2.DelayedButton(Keys.Space))
            {
                init_debugger_Audio.StopMusic();
                Memory.module = Memory.MODULE_MAINMENU_DEBUG;

                if (splashTex != null && !splashTex.IsDisposed)
                    splashTex.Dispose();
                if (white != null && !white.IsDisposed)
                    white.Dispose();
            }
            switch (publicModule)
            {
                case OverturepublicModule._0InitSound:
                    InitSound();
                    break;

                case OverturepublicModule._1WaitBeforeFirst:
                    Memory.SuppressDraw = true;
                    WaitForFirst();
                    break;

                case OverturepublicModule._2PlaySequence:
                    SplashUpdate(ref splashIndex);
                    break;
            }
        }

        public static void ResetModule()
        {
            publicModule = 0;
            publicTimer = 0.0f;
            bFadingIn = true;
            bWaitingSplash = false;
            fSplashWait = 0.0f;
            bFadingOut = false;
            Fade = 0;
            Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);
            Memory.module = Memory.MODULE_OVERTURE_DEBUG;
            publicModule = OverturepublicModule._4Squaresoft;
            Module_movie_test.ReturnState = Memory.MODULE_OVERTURE_DEBUG;
            aw = null; // was getting exception when running the overture again as the aw target changed.
        }

        private static void WaitForFirst()
        {
            if (publicTimer > 6.0f)
            {
                publicModule++;
                Console.WriteLine("MODULE_OVERTURE: DEBUG MODULE 2");
            }
            publicTimer += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0d;
        }

        private static void InitSound()
        {
            Memory.MusicIndex = 79;//79; //Overture
            init_debugger_Audio.PlayMusic();
            Memory.MusicIndex = ushort.MaxValue; // reset pos after playing overture; will loop back to start if push next

            if (white != null && !white.IsDisposed)
                white.Dispose();
            white = new Texture2D(Memory.graphics.GraphicsDevice, 4, 4, false, SurfaceFormat.Color);
            byte[] whiteBuffer = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                whiteBuffer[i] = 255;
            }

            publicModule++;
        }

        public static void Draw()
        {
            switch (publicModule)
            {
                case OverturepublicModule._0InitSound:
                case OverturepublicModule._1WaitBeforeFirst:
                    Memory.graphics.GraphicsDevice.Clear(Color.Black);
                    break;

                case OverturepublicModule._2PlaySequence:
                    Memory.graphics.GraphicsDevice.Clear(Color.Black);
                    DrawSplash();
                    break; //actually this is our entry point for draw;
                case OverturepublicModule._3SequenceFinishedPlayMainMenu:
                    DrawLogo(); //after this ends, jump into main menu module
                    break;

                case OverturepublicModule._4Squaresoft:
                    publicModule = OverturepublicModule._0InitSound;
                    Module_movie_test.Index = 103;//103;
                    Module_movie_test.ReturnState = Memory.MODULE_OVERTURE_DEBUG;
                    Memory.module = Memory.MODULE_MOVIETEST;
                    break;
            }
        }

        private static void DrawLogo()
        {
            //fade to white
            if (!bWaitingSplash)
            {
                Memory.graphics.GraphicsDevice.Clear(Color.White);
            }
            else
            {
                Memory.graphics.GraphicsDevice.Clear(Color.Black);
            }

            Memory.SpriteBatchStartAlpha(ss: SamplerState.AnisotropicClamp);
            Memory.spriteBatch.Draw(splashTex,
                new Rectangle(0, 0, Memory.graphics.GraphicsDevice.Viewport.Width, Memory.graphics.GraphicsDevice.Viewport.Height),
                new Rectangle(0, 0, splashTex.Width, splashTex.Height)
                , Color.White * Fade);
            if (bFadingIn)
            {
                Fade += Memory.gameTime.ElapsedGameTime.Milliseconds / 5000.0f;
            }

            if (bFadingOut)
            {
                Fade -= Memory.gameTime.ElapsedGameTime.Milliseconds / 2000.0f;
            }

            if (Fade < 0.0f)
            {
                bFadingIn = true;
                ReadSplash(true);
                bFadingOut = false;
            }
            if (bFadingIn && Fade > 1.0f && !bWaitingSplash)
            {
                publicTimer += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            }

            if (publicTimer > 5.0f)
            {
                bWaitingSplash = true;
                bFadingOut = true;
            }
            Memory.SpriteBatchEnd();
            if (bWaitingSplash && Fade < 0.0f)
            {
                init_debugger_Audio.StopMusic();
                Memory.module = Memory.MODULE_MAINMENU_DEBUG;
                if (splashTex != null && !splashTex.IsDisposed)
                    splashTex.Dispose();
                if (white != null && !white.IsDisposed)
                    white.Dispose();
            }
        }

        private static void DrawSplash()
        {
            if (splashTex == null)
            {
                return;
            }

            Memory.SpriteBatchStartAlpha(ss: SamplerState.AnisotropicClamp);
            Memory.spriteBatch.Draw(splashTex,
                new Rectangle(0, 0, Memory.graphics.GraphicsDevice.Viewport.Width, Memory.graphics.GraphicsDevice.Viewport.Height),
                new Rectangle(0, 0, splashTex.Width, splashTex.Height)
                , Color.White * Fade);
            Memory.SpriteBatchEnd();
        }

        public static void SplashUpdate(ref int _splashIndex)
        {
            if (aw == null)
            {
                aw = new ArchiveWorker(Memory.Archives.A_MAIN);
            }

            if (splashTex == null)
            {
                ReadSplash();
            }

            if (bFadingIn)
            {
                Fade += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 2f;
                if (Fade > 1.0f)
                {
                    Fade = 1.0f;
                    bFadingIn = false;
                    bWaitingSplash = true;
                }
            }
            if (bFadingOut)
            {
                if (splashLoop + 1 >= 0x0F && splashName >= 0x0F)
                {
                    bFadingIn = false;
                    bFadingOut = true;
                    bWaitingSplash = false;
                    publicTimer = 0.0f;
                    Fade = 1.0f;
                    publicModule++;
                    return;
                }
                Fade -= Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 2f;
                if (Fade < 0.0f)
                {
                    bFadingIn = true;
                    bFadingOut = false;
                    Fade = 0.0f;
                    _splashIndex++;
                    if (bNames)
                    {
                        splashName++;
                    }
                    else
                    {
                        splashLoop++;
                    }

                    if (_splashIndex > 1)
                    {
                        bNames = !bNames;
                        _splashIndex = 0;
                    }

                    ReadSplash();
                }
            }
            if (bWaitingSplash)
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
                Memory.SuppressDraw = true;
                fSplashWait += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            }
            //loop 01-14 + name01-14;
        }

        //Splash is 640x400 16BPP typical TIM with palette of ggg bbbbb a rrrrr gg
        public static void ReadSplash(bool bLogo = false)
        {
            string[] lof = aw.GetListOfFiles();
            string filename;
            if (splashName > 0x0f)
            {
                return;
            }
            filename = !bLogo
                ? bNames
                    ? lof.First(x => x.ToLower().Contains($"{names}{splashName.ToString("D2")}"))
                    : lof.First(x => x.ToLower().Contains($"{loops}{splashLoop.ToString("D2")}"))
                : lof.First(x => x.ToLower().Contains($"ff8.lzs"));

            byte[] buffer = ArchiveWorker.GetBinaryFile(Memory.Archives.A_MAIN, filename);
            uint uncompSize = BitConverter.ToUInt32(buffer, 0);
            buffer = buffer.Skip(4).ToArray(); //hotfix for new LZSS
            buffer = LZSS.DecompressAllNew(buffer);

            if (splashTex != null && !splashTex.IsDisposed)
                splashTex.Dispose();

            splashTex = TIM2.Overture(buffer);
            //using (FileStream fs = File.Create(Path.Combine("D:\\main", Path.GetFileNameWithoutExtension(filename) + ".png")))
            //    splashTex.SaveAsPng(fs, splashTex.Width, splashTex.Height);

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}