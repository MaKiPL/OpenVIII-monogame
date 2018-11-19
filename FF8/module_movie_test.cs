using Microsoft.Xna.Framework.Media;
using System;
using System.IO;
using AForge.Video.FFMPEG;
using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FF8
{
    internal class module_movie_test
    {
        private const int STATE_INIT = 0;
        private const int STATE_PLAYING = 1;
        private const int STATE_PAUSED = 2;
        private const int STATE_FINISHED = 3;

        private static string movieDir = Path.Combine(Memory.FF8DIR, "../movies");

        private static VideoFileReader vfr;

        private static Bitmap frame;
        private static Texture2D lastFrame;
        private static int FPS;
        //private static int currentFrame;
        private static int frameRenderingDelay;
        private static int msElapsed;

        public static int movieState = STATE_INIT;
        internal static void Update()
        {
            switch(movieState)
            {
                case STATE_INIT:
                    InitMovie();
                    break;
                case STATE_PLAYING:
                    break;

            }
        }

        private static void InitMovie()
        {
            vfr = new VideoFileReader();
            vfr.Open(movieDir + "/disc02_25h.avi");
            FPS = vfr.FrameRate;
            frameRenderingDelay = (1000 / FPS)/2; 
            //frame = new Bitmap(vfr.Width, vfr.Height);
            movieState++;
        }

        internal static void Draw()
        {
            switch(movieState)
            {
                case STATE_PLAYING:
                    PlayingDraw();
                    break;
                case STATE_PAUSED:
                    break;
                case STATE_FINISHED:
                    FinishedDraw();
                    break;
            }
        }

        private static void FinishedDraw()
        {
            Memory.spriteBatch.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
            Memory.SpriteBatchStart();
            Memory.spriteBatch.Draw(lastFrame, new Microsoft.Xna.Framework.Rectangle(0, 0, Memory.graphics.PreferredBackBufferWidth, Memory.graphics.PreferredBackBufferHeight), Microsoft.Xna.Framework.Color.White);
            Memory.SpriteBatchEnd();
            movieState = STATE_INIT;
            Memory.module = Memory.MODULE_BATTLE_DEBUG;
        }

        private static void PlayingDraw()
        {

            if (lastFrame != null && msElapsed < frameRenderingDelay)
            {
                msElapsed += Memory.gameTime.ElapsedGameTime.Milliseconds;
                return;
            }
            msElapsed = 0;
            Memory.spriteBatch.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
            frame = vfr.ReadVideoFrame();
            if(frame == null)
            {
                movieState = STATE_FINISHED;
                return;
            }
            //frame.Dispose(); //reinitalizing, so dispose
            Texture2D frameTex = new Texture2D(Memory.spriteBatch.GraphicsDevice, frame.Width, frame.Height, false, SurfaceFormat.Bgra32); //GC will collect frameTex
            BitmapData bmpdata = frame.LockBits(new Rectangle(0, 0, frame.Width, frame.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            byte[] texBuffer = new byte[bmpdata.Width * bmpdata.Height * 4]; //GC here
            Marshal.Copy(bmpdata.Scan0, texBuffer,0, texBuffer.Length);
            frame.UnlockBits(bmpdata);
            frameTex.SetData(texBuffer);
            Memory.SpriteBatchStart();
            Memory.spriteBatch.Draw(frameTex, new Microsoft.Xna.Framework.Rectangle(0, 0, Memory.graphics.PreferredBackBufferWidth, Memory.graphics.PreferredBackBufferHeight), Microsoft.Xna.Framework.Color.White);
            Memory.SpriteBatchEnd();
            lastFrame = frameTex;
        }
    }
}