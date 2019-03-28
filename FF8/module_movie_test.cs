
using FFmpeg.AutoGen;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

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
        private const int STATE_RESET = 6;

        private static readonly string[] movieDirs = {
            MakiExtended.GetUnixFullPath(Path.Combine(Memory.FF8DIR, "../movies")), //this folder has most movies
            MakiExtended.GetUnixFullPath(Path.Combine(Memory.FF8DIR, "movies"))}; //this folder has rest of movies
        private static List<string> _movies = new List<string>();
        /// <summary>
        /// Movie file list
        /// </summary>
        public static List<string> Movies
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

        private static Bitmap Frame { get; set; } = null;
        private static Ffcc Ffccvideo { get; set; } = null;
        private static Texture2D frameTex { get; set; } = null;
        private static Ffcc Ffccaudio { get; set; } = null;
        public static int ReturnState { get; set; } = Memory.MODULE_MAINMENU_DEBUG;
        /// <summary>
        /// Index in movie file list
        /// </summary>
        public static int Index { get; set; } = 0;
        private static double FPS { get; set; } = 0;
        private static int FrameRenderingDelay { get; set; } = 0;
        private static int MsElapsed { get; set; } = 0;
        public static int MovieState { get; set; } = STATE_INIT;

        internal static void Update()
        {
            if (Input.Button(Buttons.Okay) || Input.Button(Buttons.Cancel) || Input.Button(Keys.Space))
            {
                Input.ResetInputLimit();
                //init_debugger_Audio.StopAudio();
                //Memory.module = Memory.MODULE_MAINMENU_DEBUG;
                MovieState = STATE_RETURN;
            }
#if DEBUG
            // lets you move through all the feilds just holding left or right. it will just loop when it runs out.
            if (Input.Button(Buttons.Left))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                Module_main_menu_debug.MoviePointer--;
                Reset();
            }
            if (Input.Button(Buttons.Right))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                Module_main_menu_debug.MoviePointer++;
                Reset();
            }
#endif
            switch (MovieState)
            {
                case STATE_INIT:
                    MovieState++;
                    InitMovie();
                    Ffccaudio.GetFrame(); // primes the audio buffer with one frame of data
                    break;
                case STATE_CLEAR:
                    break;
                case STATE_PLAYING:
                    if (Ffccaudio.BehindFrame())
                    {
                        // if we are behind the timer get the next frame of audio.
                        Ffccaudio.GetFrame();
                    }

                    if (Ffccaudio != null)
                    {
                        Ffccaudio.PlaySound();
                    }
                    if (Ffccvideo != null)
                    {
                        Ffccvideo.PlaySound();
                    }

                    break;
                case STATE_PAUSED:
                    //todo add a function to pause sound
                    //pausing the stopwatch will cause the video to pause because it calcs the current frame based on time.
                    break;
                case STATE_FINISHED:
                    break;
                case STATE_RESET:
                    Reset();
                    break;
                case STATE_RETURN:
                default:
                    Reset();
                    Memory.module = ReturnState;
                    ReturnState = Memory.MODULE_MAINMENU_DEBUG;
                    break;
            }
        }
        private static void Reset()
        {
            if (Ffccaudio != null)
            {
                Ffccaudio.Dispose();
            }
            if (Ffccvideo != null)
            {
                Ffccvideo.Dispose();
            }

            MovieState = STATE_INIT;
            frameTex = null;
            Frame = null;
            Ffccaudio = null;
            Ffccvideo = null;
        }


        // The flush packet is a non-null packet with size 0 and data null
        private static void InitMovie()
        {

            //vfr.Open(Path.Combine(movieDirs[0] , "disc02_25h.avi"));
            //vfr.Open(Path.Combine(movieDirs[0], "disc00_30h.avi"));

            //Ffccaudio = new Ffcc(@"c:\eyes_on_me.wav", AVMediaType.AVMEDIA_TYPE_AUDIO, Ffcc.FfccMode.PROCESS_ALL);

            Ffccaudio = new Ffcc(Movies[Index], AVMediaType.AVMEDIA_TYPE_AUDIO, Ffcc.FfccMode.STATE_MACH); //Ffcc.FfccMode.PROCESS_ALL);
            //using (FileStream fs =File.OpenRead(@"C:\Users\pcvii\source\repos\ConsoleApp1\ConsoleApp1\bin\Debug\audio.wav"))
            //{
            //    SoundEffect se = new SoundEffect(init_debugger_Audio.ReadFullyByte(fs),44100,AudioChannels.Stereo);
            //    //SoundEffect se = SoundEffect.FromStream(fs);
            //            see = se.CreateInstance();
            //            see.Play();

            //}
            Ffccvideo = new Ffcc(Movies[Index], AVMediaType.AVMEDIA_TYPE_VIDEO, Ffcc.FfccMode.STATE_MACH);
            FPS = Ffccvideo.FPS;
            if (FPS == 0)
            {
                TextWriter errorWriter = Console.Error;
                errorWriter.WriteLine("Can not calc FPS, possibly FFMPEG dlls are missing or an error has occured");
                MovieState = STATE_RETURN;
            }

        }

        internal static void Draw()
        {
            switch (MovieState)
            {
                case STATE_INIT:
                    break;
                case STATE_CLEAR:
                    MovieState++;
                    ClearScreen();
                    break;
                case STATE_PLAYING:
                    PlayingDraw();
                    break;
                case STATE_PAUSED:
                    break;
                case STATE_FINISHED:
                    MovieState++;
                    FinishedDraw();
                    break;
                case STATE_RESET:
                    break;
                case STATE_RETURN:
                default:
                    break;
            }

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
            if (frameTex != null)
            {
                Memory.spriteBatch.Draw(frameTex, new Microsoft.Xna.Framework.Rectangle(0, 0, Memory.graphics.PreferredBackBufferWidth, Memory.graphics.PreferredBackBufferHeight), Microsoft.Xna.Framework.Color.White);
                frameTex.Dispose();
                frameTex = null;
            }

            Memory.SpriteBatchEnd();
            //movieState = STATE_INIT;
            //Memory.module = Memory.MODULE_BATTLE_DEBUG;
        }
        //private static Bitmap lastframe = null;
        //private static Bitmap Frame { get => frame; set { lastframe = frame; frame = value; } }
        private static void PlayingDraw()
        {
            if (frameTex == null || Ffccvideo.BehindFrame())
            {
                MsElapsed = 0;

                int ret = Ffccvideo.GetFrame();
                if (ret < 0)
                {
                    MovieState = STATE_FINISHED;
                    return;
                }
                if (frameTex != null)
                    frameTex.Dispose();
                frameTex = Ffccvideo.FrameToTexture2D();
            }

                //draw frame;
                Memory.SpriteBatchStartStencil();
                Memory.spriteBatch.Draw(frameTex, new Microsoft.Xna.Framework.Rectangle(0, 0, Memory.graphics.PreferredBackBufferWidth, Memory.graphics.PreferredBackBufferHeight), Microsoft.Xna.Framework.Color.White);
                Memory.SpriteBatchEnd();
                //backup previous frame. use if new frame unavailble
            
        }
    }
}
