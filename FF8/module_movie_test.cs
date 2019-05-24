
using FFmpeg.AutoGen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FF8
{
    public static class Module_movie_test
    {
        private const int STATE_LOAD = 0;
        private const int STATE_CLEAR = 1;
        private const int STATE_STARTPLAY = 2;
        private const int STATE_PLAYING = 3;
        private const int STATE_PAUSED = 4;
        private const int STATE_FINISHED = 5;
        private const int STATE_RETURN = 6;
        private const int STATE_RESET = 7;

        private static string[] movieDirs;

        private static List<string> _movies;
        public static void Init()
        {
            movieDirs = new string[] {
                Extended.GetUnixFullPath(Path.Combine(Memory.FF8DIRdata, "movies")), //this folder has most movies
                Extended.GetUnixFullPath(Path.Combine(Memory.FF8DIRdata_lang, "movies")) //this folder has rest of movies
            };
            _movies = new List<string>();
            foreach (string s in movieDirs)
            {
                if (Directory.Exists(s))
                {
                    _movies.AddRange(Directory.GetFiles(s, "*", SearchOption.AllDirectories).Where(x =>
                      x.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) ||
                      x.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) ||
                      x.EndsWith(".bik", StringComparison.OrdinalIgnoreCase)));
                }
            }
            ReturnState = Memory.MODULE_MAINMENU_DEBUG;
        }
        /// <summary>
        /// Movie file list
        /// </summary>
        public static List<string> Movies
        {
            get
            {
                return _movies;
            }
        }

        //private static Bitmap Frame { get; set; } = null;
        private static Ffcc FfccVideo { get; set; }
        private static Texture2D frameTex { get; set; }
        private static Ffcc FfccAudio { get; set; }
        public static int ReturnState { get; set; }
        /// <summary>
        /// Index in movie file list
        /// </summary>
        public static int Index { get; set; } = 0;
        private static double FPS { get; set; } = 0;
        private static int FrameRenderingDelay { get; set; } = 0;
        private static int MsElapsed { get; set; } = 0;
        public static int MovieState { get; set; } = STATE_LOAD;

        public static void Update()
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
                case STATE_LOAD:
                    MovieState++;
                    LoadMovie();
                    break;
                case STATE_CLEAR:
                    break;
                case STATE_STARTPLAY:
                    MovieState++;
                    if (FfccAudio != null)
                    {
                        FfccAudio.PlayInTask();
                    }
                    if (FfccVideo != null)
                    {
                        FfccVideo.Play();
                    }
                    break;
                case STATE_PLAYING:
                    //if (FfccAudio != null && !FfccAudio.Ahead)
                    //{
                    //    // if we are behind the timer get the next frame of audio.
                    //    FfccAudio.Next();
                    //}
                    if (FfccVideo==null)
                        MovieState = STATE_FINISHED;
                    else if (FfccVideo.Behind)
                    {
                        if (FfccVideo.Next() < 0)
                        {
                            MovieState = STATE_FINISHED;
                            //Memory.SuppressDraw = true;
                            break;
                        }
                        else if (frameTex != null)
                        {
                            frameTex.Dispose();
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            frameTex = null;
                        }
                    }
                    else
                    {
                        Memory.SuppressDraw = true;
                    }
                    if (frameTex == null)
                    {
                        if(FfccVideo!=null)
                            frameTex = FfccVideo.Texture2D();
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
            if (FfccAudio != null)
            {
                FfccAudio.Dispose();
            }
            FfccAudio = null;
            if (FfccVideo != null)
            {
                FfccVideo.Dispose();
            }
            FfccVideo = null;

            MovieState = STATE_LOAD;
            if (frameTex != null && !frameTex.IsDisposed)
            {
                frameTex.Dispose();
            }

            frameTex = null;
            GC.Collect();
        }


        // The flush packet is a non-null packet with size 0 and data null
        private static void LoadMovie()
        {
            if (Movies != null && Index < Movies.Count)
            {
                FfccAudio = new Ffcc(Movies[Index], AVMediaType.AVMEDIA_TYPE_AUDIO, Ffcc.FfccMode.STATE_MACH);
                FfccVideo = new Ffcc(Movies[Index], AVMediaType.AVMEDIA_TYPE_VIDEO, Ffcc.FfccMode.STATE_MACH);

                FPS = FfccVideo.FPS;
                if (Math.Abs(FPS) < double.Epsilon)
                {
                    TextWriter errorWriter = Console.Error;
                    errorWriter.WriteLine("Can not calc FPS, possibly FFMPEG dlls are missing or an error has occured");
                    MovieState = STATE_RETURN;
                }
            }
        }

        public static void Draw()
        {
            switch (MovieState)
            {
                case STATE_LOAD:
                    break;
                case STATE_CLEAR:
                    MovieState++;
                    ClearScreen();
                    break;
                case STATE_STARTPLAY:                    
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
        }
        private static void ClearScreen()
        {
            Memory.spriteBatch.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
        }
        private static void FinishedDraw()
        {
            ClearScreen();

            if (frameTex != null)
            {
                Memory.SpriteBatchStartStencil();
                Memory.spriteBatch.Draw(frameTex, new Microsoft.Xna.Framework.Rectangle(0, 0, Memory.graphics.GraphicsDevice.Viewport.Width, Memory.graphics.GraphicsDevice.Viewport.Height), Microsoft.Xna.Framework.Color.White);
                Memory.SpriteBatchEnd();
            }
            //movieState = STATE_INIT;
            //Memory.module = Memory.MODULE_BATTLE_DEBUG;
        }
        //private static Bitmap lastframe = null;
        //private static Bitmap Frame { get => frame; set { lastframe = frame; frame = value; } }
        private static void PlayingDraw()
        {
            if (frameTex == null)
            {
                return;
            }
            //draw frame;
            Viewport vp = Memory.graphics.GraphicsDevice.Viewport;
            Memory.SpriteBatchStartStencil(ss: SamplerState.AnisotropicClamp);//by default xna filters all textures SamplerState.PointClamp disables that. so video is being filtered why playing.
            ClearScreen();
            Rectangle dst = new Rectangle(new Point(0), (new Vector2(frameTex.Width, frameTex.Height) * Memory.Scale(frameTex.Width, frameTex.Height)).ToPoint());
            dst.Offset(Memory.Center.X - dst.Center.X,Memory.Center.Y - dst.Center.Y);
            Memory.spriteBatch.Draw(frameTex,dst, Microsoft.Xna.Framework.Color.White);
            Memory.SpriteBatchEnd();
            
        }
    }
}
