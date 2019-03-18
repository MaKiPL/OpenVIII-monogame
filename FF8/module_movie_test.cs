
using System;
using System.IO;
#if _WINDOWS
using AForge.Video.FFMPEG;
#endif
using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using FFmpeg.AutoGen;

namespace FF8
{
    internal static class Module_movie_test
    {
        private const int STATE_INIT = 0;
        private const int STATE_CLEAR = 1;
        private const int STATE_PLAYING = 2;
        private const int STATE_PAUSED = 3;
        private const int STATE_FINISHED = 4;
        private const int STATE_RETURN = 5;

        private static readonly string[] movieDirs = {
            MakiExtended.GetUnixFullPath(Path.Combine(Memory.FF8DIR, "../movies")), //this folder has most movies
            MakiExtended.GetUnixFullPath(Path.Combine(Memory.FF8DIR, "movies"))}; //this folder has rest of movies
        private static List<string> _movies = new List<string>();
        public static List<string> movies
        {
            get
            {
                if (_movies.Count == 0)
                {
                    foreach (string s in movieDirs)
                    {
                        _movies.AddRange(Directory.GetFiles(s, "*.avi"));
                    }
                }
                return _movies;
            }
        }
        public static int Index = 0;
        public static int returnState = Memory.MODULE_MAINMENU_DEBUG;
#if _WINDOWS
        //private static VideoFileReader vfr;
#endif

        static Ffcc ffcc;
        private static Bitmap frame = null;
        private static Texture2D lastFrame = null;
        private static int FPS=0;
        private static int frameRenderingDelay=0;
        private static int msElapsed;

        public static int movieState;
        internal static void Update()
        {
            switch (movieState)
            {
                case STATE_INIT:
                    movieState++;
                    InitMovie();
                    break;
                case STATE_CLEAR:
                    movieState++;
                    ClearScreen();
                    break;
                case STATE_PLAYING:
                    PlayingDraw();
                    break;
                case STATE_PAUSED:
                    break;
                case STATE_FINISHED:
                    movieState++;
                    FinishedDraw();
                    break;
                case STATE_RETURN:
                default:
                    Memory.module = returnState;
                    movieState = STATE_INIT;
                    lastFrame = null;
                    frame = null;
                    returnState = Memory.MODULE_MAINMENU_DEBUG; 
                    break;
            }
        }


        // The flush packet is a non-null packet with size 0 and data null

        private static void InitMovie()
        {
#if _WINDOWS
            //vfr = new VideoFileReader();
            //vfr.Open(movies[Index]);
            //FPS = vfr.FrameRate;
#endif
            //vfr.Open(Path.Combine(movieDirs[0] , "disc02_25h.avi"));
            //vfr.Open(Path.Combine(movieDirs[0], "disc00_30h.avi"));

            ffcc = new Ffcc(movies[Index],AVMediaType.AVMEDIA_TYPE_VIDEO,Ffcc.FfccMode.STATE_MACH);
            FPS = ffcc.FPS;
            try
            {
                frameRenderingDelay = (1000 / FPS) / 2;
            }
            catch(DivideByZeroException e)
            {
                TextWriter errorWriter = Console.Error;
                errorWriter.WriteLine(e.Message);
                errorWriter.WriteLine("Can not calc FPS, possibly FFMPEG dlls are missing or an error has occured");
                movieState = STATE_RETURN;
            }

        }

        internal static void Draw()
        {
            //switch (movieState)
            //{
                //case STATE_INIT:
                //    break;
                //case STATE_CLEAR:
                //    ClearScreen();
                //    movieState++;
                //    break;
                //case STATE_PLAYING:
                //    PlayingDraw();
                //    break;
                //case STATE_PAUSED:
                //    break;
                //case STATE_FINISHED:
                //    FinishedDraw();
                //    break;
                //default:
                    //Memory.module = Memory.MODULE_MAINMENU_DEBUG;
                    //break;

            //}
        }
        private static void ClearScreen()
        {
            Memory.spriteBatch.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
        }
        private static void FinishedDraw()
        {
            ClearScreen();
            Memory.SpriteBatchStartStencil();
            if(lastFrame != null)
                Memory.spriteBatch.Draw(lastFrame, new Microsoft.Xna.Framework.Rectangle(0, 0, Memory.graphics.PreferredBackBufferWidth, Memory.graphics.PreferredBackBufferHeight), Microsoft.Xna.Framework.Color.White);
            Memory.SpriteBatchEnd();
            //movieState = STATE_INIT;
            //Memory.module = Memory.MODULE_BATTLE_DEBUG;
        }
        //private static Bitmap lastframe = null;
        //private static Bitmap Frame { get => frame; set { lastframe = frame; frame = value; } }
        private static void PlayingDraw()
        {
            Texture2D frameTex = lastFrame;
            if (lastFrame != null && msElapsed < frameRenderingDelay)
            {
                msElapsed += Memory.gameTime.ElapsedGameTime.Milliseconds;
                //redraw previous frame or flickering happens.
            }
            else
            {
                msElapsed = 0;
                //                if (frame != null)
                //                    frame.Dispose();
                //#if _WINDOWS
                //                frame = vfr.ReadVideoFrame();
                //#endif

                int ret = ffcc.GetFrame();
                if (ret < 0)
                { 
                    movieState = STATE_FINISHED;
                    return;
                }
                frameTex = ffcc.FrameToTexture2D();
            }
            //if (frame == null)
            //{
            //    movieState = STATE_FINISHED;
            //    return;
            //}
            //Texture2D frameTex = new Texture2D(Memory.spriteBatch.GraphicsDevice, frame.Width, frame.Height, false, SurfaceFormat.Color); //GC will collect frameTex
            //BitmapData bmpdata = frame.LockBits(new Rectangle(0, 0, frame.Width, frame.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);// System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //byte[] texBuffer = new byte[bmpdata.Width * bmpdata.Height * 4]; //GC here
            //Marshal.Copy(bmpdata.Scan0, texBuffer, 0, texBuffer.Length);
            //frame.UnlockBits(bmpdata);
            //frameTex.SetData(texBuffer);

            //draw frame;
            Memory.SpriteBatchStartStencil();
            Memory.spriteBatch.Draw(frameTex, new Microsoft.Xna.Framework.Rectangle(0, 0, Memory.graphics.PreferredBackBufferWidth, Memory.graphics.PreferredBackBufferHeight), Microsoft.Xna.Framework.Color.White);
            Memory.SpriteBatchEnd();
            //backup previous frame. use if new frame unavailble
            lastFrame = frameTex;
            
        }
    }
}