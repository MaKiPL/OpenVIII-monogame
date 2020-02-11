namespace OpenVIII.AV
{
    using FFmpeg.AutoGen;
    using System.Threading.Tasks;

    public class Audio : Ffcc
    {
        #region Methods

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        public static Audio Load(string filename, int loopstart = -1) => Load<Audio>(filename, AVMediaType.AVMEDIA_TYPE_AUDIO, FfccMode.STATE_MACH, loopstart);

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        /// <remarks>
        /// Could be better, but theres alot of hoops to jump through.
        /// </remarks>
        /// <see cref="https://stackoverflow.com/questions/9604633/reading-a-file-located-in-memory-with-libavformat"/>
        /// <seealso cref="http://www.ffmpeg.org/doxygen/trunk/doc_2examples_2avio_reading_8c-example.html"/>
        /// <seealso cref="https://stackoverflow.com/questions/24758386/intptr-to-callback-function"/>
        public static unsafe Audio Load(BufferData buffer_Data, byte[] headerData, int loopstart = -1, FfccMode ffccMode = FfccMode.PROCESS_ALL)
        {
            Audio r = new Audio();

            void play(BufferData* d)
            {
                r.LoadFromRAM(d);
                r.Init(null, AVMediaType.AVMEDIA_TYPE_AUDIO, ffccMode, loopstart);
                ffmpeg.avformat_free_context(r.Decoder.Format);
                //ffmpeg.avio_context_free(&Decoder._format->pb); //CTD
                r.Decoder.Format = null;
            }
            if (ffccMode == FfccMode.PROCESS_ALL || true)
            {
                if (headerData != null)
                    fixed (byte* tmp = &headerData[0])
                    {
                        lock (r.Decoder)
                        {
                            buffer_Data.SetHeader(tmp);
                            play(&buffer_Data);
                        }
                        r.Dispose(false);
                    }
                else
                    play(&buffer_Data);
            }
            return r;
        }
        public static unsafe Audio Load(BufferData* buffer_Data, byte[] headerData, int loopstart = -1, FfccMode ffccMode = FfccMode.PROCESS_ALL)
        {
            Audio r = new Audio();

            void play(BufferData* d)
            {
                r.LoadFromRAM(d);
                r.Init(null, AVMediaType.AVMEDIA_TYPE_AUDIO, ffccMode, loopstart);
                ffmpeg.avformat_free_context(r.Decoder.Format);
                //ffmpeg.avio_context_free(&Decoder._format->pb); //CTD
                r.Decoder.Format = null;
            }
            if (ffccMode == FfccMode.PROCESS_ALL || true)
            {
                if (headerData != null)
                    fixed (byte* tmp = &headerData[0])
                    {
                        lock (r.Decoder)
                        {
                            buffer_Data->SetHeader(tmp);
                            play(buffer_Data);
                        }
                        r.Dispose(false);
                    }
                else
                    play(buffer_Data);
            }
            return r;
        }

            #endregion Methods
        }
}