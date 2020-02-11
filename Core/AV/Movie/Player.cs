using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace OpenVIII.Movie
{
    public class Player : IDisposable
    {
        #region Fields

        public static readonly int[] LetterBox = new int[] { 101, 103, 104 };
        private static Files Files;

        private STATE _state;

        private AV.Audio Audio;

        private bool disposedValue = false;

        private bool SuppressDraw;

        private Texture2D Texture;

        private AV.Video Video;

        #endregion Fields

        #region Destructors

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~Player()
        {
            //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        #endregion Destructors

        #region Events

        public event EventHandler<STATE> StateChanged;

        #endregion Events

        #region Properties

        public int ID { get; private set; }

        // To detect redundant calls
        public bool IsDisposed => disposedValue;

        public STATE STATE
        {
            get => _state; private set
            {
                _state = value;
                StateChanged?.Invoke(this, value);
            }
        }

        #endregion Properties

        #region Methods

        public static Player Load(int ID, bool OverlayingModels = false)
        {
            AV.Audio ffccAudioFromZZZ = null;
            if (Files.ZZZ)
            {
                ArchiveZZZ a = (ArchiveZZZ)ArchiveZZZ.Load(Memory.Archives.ZZZ_OTHER);
                ArchiveZZZ.FileData fd = a.GetFileData(Files[ID]);

                AV.Audio ffcc = AV.Audio.Load(
                    new AV.BufferData
                    {
                        DataSeekLoc = fd.Offset,
                        DataSize = fd.Size,
                        HeaderSize = 0,
                        Target = AV.BufferData.TargetFile.other_zzz
                    },
                    null, -1);

                //ffcc.Play(volume, pitch, pan);
                ffccAudioFromZZZ = ffcc;
            }
            Player Player = new Player()
            {
                ID = ID,
                STATE = STATE.LOAD,
                Video = Files.Exists(ID) ? AV.Video.Load(Files[ID]) : null,
                Audio = Files.ZZZ ? ffccAudioFromZZZ : Files.Exists(ID) ? AV.Audio.Load(Files[ID]) : null,
                SuppressDraw = !OverlayingModels
            };
            Player.STATE++;
            if (Player.Video == null && Player.Audio == null)
                return Player = null;
            return Player;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() =>
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

        public void Draw()
        {
            switch (STATE)
            {
                case STATE.LOAD:
                    break;

                case STATE.CLEAR:
                    STATE++;
                    ClearScreen();
                    break;

                case STATE.STARTPLAY:
                case STATE.PLAYING:
                    PlayingDraw();
                    break;

                case STATE.PAUSED:
                    break;

                case STATE.FINISHED:
                    STATE++;
                    PlayingDraw();
                    break;

                case STATE.RESET:
                    break;

                case STATE.RETURN:
                default:
                    break;
            }
        }

        public void PlayingDraw()
        {
            if (Texture == null)
            {
                return;
            }
            //draw frame;
            Viewport vp = Memory.graphics.GraphicsDevice.Viewport;
            Memory.SpriteBatchStartStencil(ss: SamplerState.AnisotropicClamp);//by default xna filters all textures SamplerState.PointClamp disables that. so video is being filtered why playing.
            ClearScreen();
            Rectangle dst = new Rectangle(new Point(0), (new Vector2(Texture.Width, Texture.Height) * Memory.Scale(Texture.Width, Texture.Height, LetterBox.Contains(ID) ? Memory.ScaleMode.FitHorizontal : Memory.ScaleMode.FitBoth)).ToPoint());
            dst.Offset(Memory.Center.X - dst.Center.X, Memory.Center.Y - dst.Center.Y);
            Memory.spriteBatch.Draw(Texture, dst, Microsoft.Xna.Framework.Color.White);
            Memory.SpriteBatchEnd();
        }

        public void STOP()
        {
            Audio.Dispose();
            Video.Dispose();
            STATE = STATE.RETURN;
        }

        public void Update()
        {
            switch (STATE)
            {
                case STATE.LOAD:
                    break;

                case STATE.CLEAR:
                    break;

                case STATE.STARTPLAY:
                    STATE++;
                    if (Audio != null)
                    {
                        Audio.PlayInTask();
                    }
                    if (Video != null)
                    {
                        Video.Play();
                    }
                    break;

                case STATE.PLAYING:
                    //if (Audio != null && !Audio.Ahead)
                    //{
                    //    // if we are behind the timer get the next frame of audio.
                    //    Audio.Next();
                    //}
                    if (Video == null)
                        STATE = STATE.FINISHED;
                    else if (Video.Behind)
                    {
                        if (Video.Next() < 0)
                        {
                            STATE = STATE.FINISHED;
                            //Memory.SuppressDraw = true;
                            break;
                        }
                        else if (Texture != null)
                        {
                            Texture.Dispose();
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            Texture = null;
                        }
                    }
                    else
                    {
                        //Memory next frame is skipped.
                        Memory.SuppressDraw = SuppressDraw;
                    }
                    if (Texture == null)
                    {
                        if (Video != null)
                        {
                            if (Memory.State?.Fieldvars != null)
                                Memory.State.Fieldvars.FMVFrames = (ulong)Video.CurrentFrameNum;
                            Texture = Video.Texture2D();
                        }
                    }
                    break;

                case STATE.PAUSED:
                    //todo add a function to pause sound
                    //pausing the stopwatch will cause the video to pause because it calcs the current frame based on time.
                    break;

                case STATE.FINISHED:
                    break;

                case STATE.RESET:
                    break;

                case STATE.RETURN:
                default:

                    break;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                if (!(Video?.IsDisposed?? true))
                    Video.Dispose();
                if (!(Audio?.IsDisposed?? true))
                    Audio.Dispose();
                if (Texture != null && !Texture.IsDisposed)
                    Texture.Dispose();
                disposedValue = true;
            }
        }

        private static void ClearScreen() => Memory.spriteBatch.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);

        #endregion Methods

        // TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);
    }
}