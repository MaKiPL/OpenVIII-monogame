using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

#if _WINDOWS && !_X64
using DirectMidi;
#endif

using System.Runtime.InteropServices;
using NAudio.Vorbis;
using FFmpeg.AutoGen;
using System.Diagnostics;

namespace FF8
{
#pragma warning disable IDE1006 // Naming Styles

    internal static class init_debugger_Audio
#pragma warning restore IDE1006 // Naming Styles
    {
#if _WINDOWS && !_X64
        private static CDirectMusic cdm;
        private static CDLSLoader loader;
        private static CSegment segment;
        private static CAPathPerformance path;
        public static CPortPerformance cport; //public explicit
        private static COutputPort outport;
        private static CCollection ccollection;
        private static CInstrument[] instruments;
#endif

        private static byte[] getBytes(object aux)
        {
            int length = Marshal.SizeOf(aux);
            IntPtr ptr = Marshal.AllocHGlobal(length);
            byte[] myBuffer = new byte[length];

            Marshal.StructureToPtr(aux, ptr, true);
            Marshal.Copy(ptr, myBuffer, 0, length);
            Marshal.FreeHGlobal(ptr);

            return myBuffer;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct SoundEntry
        {
            public UInt32 Size;
            public UInt32 Offset;
            private UInt32 output_TotalSize => Size + 70; // Total bytes of file -8 because for some reason 8 bytes don't count
            private const UInt32 output_HeaderSize = 50; //Total bytes of Header
            private UInt32 output_DataSize => Size; //Total bytes of Data Section

            //public byte[] UNK; //12
            //public WAVEFORMATEX WAVFORMATEX; //18 header starts here
            //public ushort SamplesPerBlock; //2
            //public ushort ADPCM; //2
            //public ADPCMCOEFSET[] ADPCMCoefSets; //array should be of [ADPCM] size //7*4 = 28
            public byte[] HeaderData;

            public void fillHeader(BinaryReader br)
            {
                if (HeaderData == null)
                {
                    HeaderData = new byte[output_HeaderSize + 28];
                    using (MemoryStream ms = new MemoryStream(HeaderData))
                    {
                        ms.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
                        ms.Write(getBytes(output_TotalSize), 0, 4);
                        ms.Write(Encoding.ASCII.GetBytes("WAVEfmt "), 0, 8);
                        ms.Write(getBytes(output_HeaderSize), 0, 4);
                        ms.Write(br.ReadBytes((int)output_HeaderSize), 0, (int)output_HeaderSize);
                        ms.Write(Encoding.ASCII.GetBytes("data"), 0, 4);
                        ms.Write(getBytes(output_DataSize), 0, 4);
                    }
                }
            }
        }

#pragma warning disable CS0649

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct WAVEFORMATEX
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct ADPCMCOEFSET
        {
            public short iCoef1;
            public short iCoef2;
        };

#pragma warning restore CS0649

        private static SoundEntry[] soundEntries;
        public static int soundEntriesCount;

        public const int S_OK = 0x00000000;
        public const int MaxSoundChannels = 20;

        /// <summary>
        /// This is for short lived sound effects. The Larger the array is the more sounds can be
        /// played at once. If you want sounds to loop of have volume you'll need to have a
        /// SoundEffectInstance added to ffcc, and have those sounds be played like music where they
        /// loop in the background till stop.
        /// </summary>
        public static Ffcc[] SoundChannels { get; } = new Ffcc[MaxSoundChannels];

        public static int CurrentSoundChannel
        {
            get => _currentSoundChannel;
            set
            {
                if (value >= MaxSoundChannels)
                {
                    value = 0;
                }
                else if (value < 0)
                {
                    value = MaxSoundChannels - 1;
                }

                _currentSoundChannel = value;
            }
        }

        internal static void DEBUG()
        {
            string dmusic_pt = "", RaW_ogg_pt = "", music_pt = "", music_wav_pt = "";
            //Roses and Wine V07 moves most of the sgt files to dmusic_backup
            //it leaves a few files behind. I think because RaW doesn't replace everything.
            //ogg files stored in:
            RaW_ogg_pt = MakiExtended.GetUnixFullPath(Path.Combine(Memory.FF8DIR, "../../RaW/GLOBAL/Music"));
            if (!Directory.Exists(RaW_ogg_pt))
            {
                RaW_ogg_pt = null;
            }
            // From what I gather the OGG files and the sgt files have the same numerical prefix. I
            // might try to add the functionality to the debug screen monday.

            dmusic_pt = MakiExtended.GetUnixFullPath(Path.Combine(Memory.FF8DIR, "../Music/dmusic_backup/"));
            if (!Directory.Exists(dmusic_pt))
            {
                dmusic_pt = null;
            }

            music_pt = MakiExtended.GetUnixFullPath(Path.Combine(Memory.FF8DIR, "../Music/dmusic/"));
            if (!Directory.Exists(music_pt))
            {
                music_pt = null;
            }

            music_wav_pt = MakiExtended.GetUnixFullPath(Path.Combine(Memory.FF8DIR, "../Music/"));
            if (!Directory.Exists(music_wav_pt))
            {
                music_wav_pt = null;
            }

            // goal of dicmusic is to be able to select a track by prefix. it adds an list of files
            // with the same prefix. so you can later on switch out which one you want.
            if (RaW_ogg_pt != null)
            {
                Memory.musices = Directory.GetFiles(RaW_ogg_pt, "*.ogg");
                foreach (string m in Memory.musices)
                {
                    if (ushort.TryParse(Path.GetFileName(m).Substring(0, 3), out ushort key))
                    {
                        //mismatched prefix's go here
                        if (key == 512)
                        {
                            key = 0; //loser.ogg and sgt don't match.
                        }

                        if (!Memory.dicMusic.ContainsKey(key))
                        {
                            Memory.dicMusic.Add(key, new List<string> { m });
                        }
                        else
                        {
                            Memory.dicMusic[key].Add(m);
                        }
                    }
                }
            }
            if (dmusic_pt != null)
            {
                Memory.musices = Directory.GetFiles(dmusic_pt, "*.sgt");

                foreach (string m in Memory.musices)
                {
                    if (ushort.TryParse(Path.GetFileName(m).Substring(0, 3), out ushort key))
                    {
                        if (!Memory.dicMusic.ContainsKey(key))
                        {
                            Memory.dicMusic.Add(key, new List<string> { m });
                        }
                        else
                        {
                            Memory.dicMusic[key].Add(m);
                        }
                    }
                    else
                    {
                        if (!Memory.dicMusic.ContainsKey(999)) //gets any music w/o prefix
                        {
                            Memory.dicMusic.Add(999, new List<string> { m });
                        }
                        else
                        {
                            Memory.dicMusic[999].Add(m);
                        }
                    }
                }
            }
            if (music_pt != null)
            {
                Memory.musices = Directory.GetFiles(music_pt, "*.sgt");

                foreach (string m in Memory.musices)
                {
                    if (ushort.TryParse(Path.GetFileName(m).Substring(0, 3), out ushort key))
                    {
                        if (!Memory.dicMusic.ContainsKey(key))
                        {
                            Memory.dicMusic.Add(key, new List<string> { m });
                        }
                        else
                        {
                            Memory.dicMusic[key].Add(m);
                        }
                    }
                    else
                    {
                        if (!Memory.dicMusic.ContainsKey(999)) //gets any music w/o prefix
                        {
                            Memory.dicMusic.Add(999, new List<string> { m });
                        }
                        else
                        {
                            Memory.dicMusic[999].Add(m);
                        }
                    }
                }
            }
            if (music_wav_pt != null)
            {
                Memory.musices = Directory.GetFiles(music_pt, "*.wav");

                foreach (string m in Memory.musices)
                {
                    if (ushort.TryParse(Path.GetFileName(m).Substring(0, 3), out ushort key))
                    {
                        if (!Memory.dicMusic.ContainsKey(key))
                        {
                            Memory.dicMusic.Add(key, new List<string> { m });
                        }
                        else
                        {
                            Memory.dicMusic[key].Add(m);
                        }
                    }
                    else
                    {
                        if (!Memory.dicMusic.ContainsKey(999)) //gets any music w/o prefix
                        {
                            Memory.dicMusic.Add(999, new List<string> { m });
                        }
                        else
                        {
                            Memory.dicMusic[999].Add(m);
                        }
                    }
                }
            }
        }

        //I messed around here as figuring out how things worked probably didn't need to mess with this.
        internal static void DEBUG_SoundAudio()
        {
            string path = Path.Combine(Memory.FF8DIR, "../Sound/audio.fmt");
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                soundEntries = new SoundEntry[br.ReadUInt32()];
                fs.Seek(36, SeekOrigin.Current);
                for (int i = 0; i < soundEntries.Length - 1; i++)
                {
                    UInt32 sz = br.ReadUInt32();
                    if (sz == 0)
                    {
                        fs.Seek(34, SeekOrigin.Current); continue;
                    }

                    soundEntries[i] = new SoundEntry
                    {
                        Size = sz,
                        Offset = br.ReadUInt32()
                    };
                    fs.Seek(12, SeekOrigin.Current);
                    soundEntries[i].fillHeader(br);
                }
            }
            soundEntriesCount = soundEntries.Length;
        }

        internal static void PlaySound(int soundID)
        {
            if (soundEntries == null || soundEntries[soundID].Size == 0)
            {
                return;
            }
            SoundChannels[CurrentSoundChannel] = new Ffcc(
                new Ffcc.Buffer_Data { DataSeekLoc = soundEntries[soundID].Offset, DataSize = soundEntries[soundID].Size, HeaderSize = (uint)soundEntries[soundID].HeaderData.Length },
                soundEntries[soundID].HeaderData,
                Path.Combine(Memory.FF8DIR, "../Sound/audio.dat"));
            SoundChannels[CurrentSoundChannel++].Play();
        }

        public static void StopSound()
        {
            //waveout.Stop();
        }

        internal static void Update()
        {
            //checks to see if music buffer is running low and getframe triggers a refill.
            //if (ffccMusic != null && !ffccMusic.Ahead)
            //{
            //    ffccMusic.Next();
            //}
            //if played in task we don't need to do this.
        }

        //callable test

        public static byte[] ReadFullyByte(Stream stream)
        {
            // following formula goal is to calculate the number of bytes to make buffer. might be wrong.
            long size = stream.Length; // stream.Length should be in bytes. will error later if short.
            int start = 0;
            byte[] buffer = new byte[size];
            int read = 0;
            //do
            //{
            read = stream.Read(buffer, start, buffer.Length);
            start++;
            //}
            //while (read == 0 && start < size);
            if (read == 0)
            {
                return null;
            }

            if (read < size)
            {
                Array.Resize<byte>(ref buffer, read);
            }

            return buffer;
        }

        public static byte[] ReadFullyFloat(VorbisWaveReader stream)
        {
            // following formula goal is to calculate the number of bytes to make buffer. might be wrong.
            long size = (stream.Length / sizeof(float)) + 100; //unsure why but read was > than size so added 100; will error if the size is too small.

            float[] buffer = new float[size];

            int read = stream.Read(buffer, 0, buffer.Length);
            return GetSamplesWaveData(buffer, read);
        }

        public static byte[] GetSamplesWaveData(byte[] samples, int samplesCount)
        {
            float[] f = new float[(samplesCount / sizeof(float))];
            int i = 0;
            for (int n = 0; n < samples.Length; n += sizeof(float))
            {
                f[i++] = BitConverter.ToSingle(samples, n);
            }
            return GetSamplesWaveData(f, samplesCount / sizeof(float));
        }

        public static byte[] GetSamplesWaveData(float[] samples, int samplesCount)
        { // converts 32 bit float samples to 16 bit pcm. I think :P
            // https://stackoverflow.com/questions/31957211/how-to-convert-an-array-of-int16-sound-samples-to-a-byte-array-to-use-in-monogam/42151979#42151979
            byte[] pcm = new byte[samplesCount * 2];
            int sampleIndex = 0,
                pcmIndex = 0;

            while (sampleIndex < samplesCount)
            {
                short outsample = (short)(samples[sampleIndex] * short.MaxValue);
                pcm[pcmIndex] = (byte)(outsample & 0xff);
                pcm[pcmIndex + 1] = (byte)((outsample >> 8) & 0xff);

                sampleIndex++;
                pcmIndex += 2;
            }

            return pcm;
        }

        private static bool musicplaying = false;
        private static int lastplayed = -1;

        public static void PlayStopMusic()
        {
            if (!musicplaying || lastplayed != Memory.MusicIndex)
            {
                PlayMusic();
            }
            else
            {
                StopAudio();
            }
        }

        private static Ffcc ffccMusic = null; // testing using class to play music instead of Naudio / Nvorbis
        private static int _currentSoundChannel;

        public static void PlayMusic()
        {
            string ext = "";

            if (Memory.dicMusic[Memory.MusicIndex].Count > 0)
            {
                ext = Path.GetExtension(Memory.dicMusic[Memory.MusicIndex][0]).ToLower();
            }

            string pt = Memory.dicMusic[Memory.MusicIndex][0];

            StopAudio();

            switch (ext)
            {
                case ".ogg":
                    //ffccMusic = new Ffcc(@"c:\eyes_on_me.wav", AVMediaType.AVMEDIA_TYPE_AUDIO, Ffcc.FfccMode.STATE_MACH);
                    if (ffccMusic != null)
                        ffccMusic.Dispose();
                    ffccMusic = new Ffcc(pt, AVMediaType.AVMEDIA_TYPE_AUDIO, Ffcc.FfccMode.STATE_MACH);
                    ffccMusic.PlayInTask(.5f);
                    break;

                case ".sgt":

                    if (!MakiExtended.IsLinux)
                    {
#if _WINDOWS && !_X64
                        if (cdm == null)
                        {
                            cdm = new CDirectMusic();
                            cdm.Initialize();
                            loader = new CDLSLoader();
                            loader.Initialize();
                            loader.LoadSegment(pt, out segment);
                            ccollection = new CCollection();
                            string pathDLS = Path.Combine(Memory.FF8DIR, "../Music/dmusic_backup/FF8.dls");
                            if (!File.Exists(pathDLS))
                            {
                                pathDLS = Path.Combine(Memory.FF8DIR, "../Music/dmusic/FF8.dls");
                            }

                            loader.LoadDLS(pathDLS, out ccollection);
                            uint dwInstrumentIndex = 0;
                            while (ccollection.EnumInstrument(++dwInstrumentIndex, out INSTRUMENTINFO iInfo) == S_OK)
                            {
                                Debug.WriteLine(iInfo.szInstName);
                            }
                            instruments = new CInstrument[dwInstrumentIndex];

                            path = new CAPathPerformance();
                            path.Initialize(cdm, null, null, DMUS_APATH.DYNAMIC_3D, 128);
                            cport = new CPortPerformance();
                            cport.Initialize(cdm, null, null);
                            outport = new COutputPort();
                            outport.Initialize(cdm);

                            uint dwPortCount = 0;
                            INFOPORT infoport;
                            do
                            {
                                outport.GetPortInfo(++dwPortCount, out infoport);
                            }
                            while ((infoport.dwFlags & DMUS_PC.SOFTWARESYNTH) == 0);

                            outport.SetPortParams(0, 0, 0, SET.REVERB | SET.CHORUS, 44100);
                            outport.ActivatePort(infoport);

                            cport.AddPort(outport, 0, 1);

                            for (int i = 0; i < dwInstrumentIndex; i++)
                            {
                                ccollection.GetInstrument(out instruments[i], i);
                                outport.DownloadInstrument(instruments[i]);
                            }
                            segment.Download(cport);
                            cport.PlaySegment(segment);
                        }
                        else
                        {
                            cport.Stop(segment);
                            segment.Dispose();
                            //segment.ConnectToDLS
                            loader.LoadSegment(pt, out segment);
                            segment.Download(cport);
                            cport.PlaySegment(segment);
                            cdm.Dispose();
                        }

                        //GCHandle.Alloc(cdm, GCHandleType.Pinned);
                        //GCHandle.Alloc(loader, GCHandleType.Pinned);
                        //GCHandle.Alloc(segment, GCHandleType.Pinned);
                        //GCHandle.Alloc(path, GCHandleType.Pinned);
                        //GCHandle.Alloc(cport, GCHandleType.Pinned);
                        //GCHandle.Alloc(outport, GCHandleType.Pinned);
                        //GCHandle.Alloc(infoport, GCHandleType.Pinned);
#endif
                    }
                    break;
            }

            musicplaying = true;
            lastplayed = Memory.MusicIndex;
        }

        public static void KillAudio()
        {
            //if (Sound != null && !Sound.IsDisposed)
            //{
            //    Sound.Dispose();
            //}
            for (int i = 0; i < MaxSoundChannels; i++)
            {
                if (SoundChannels[i] != null && !SoundChannels[i].isDisposed)
                {
                    SoundChannels[i].Dispose();
                    SoundChannels[i] = null;
                }
            }

            try
            {
                if (MakiExtended.IsLinux)
                {
#if _WINDOWS && !_X64
                    cport.StopAll();
                    cport.Dispose();
                    ccollection.Dispose();
                    loader.Dispose();
                    outport.Dispose();
                    path.Dispose();
                    cdm.Dispose();
#endif
                }
            }
            catch
            {
            }
        }

        public static void StopAudio()
        {
            musicplaying = false;
            if (ffccMusic != null)
            {
                ffccMusic.Dispose();
                ffccMusic = null;
            }

#if _WINDOWS && !_X64
            try
            {
                if (!MakiExtended.IsLinux)
                {
                    cport.StopAll();
                }
            }
            catch { }
#endif
        }

        private static List<byte[]> ProcessTrackList(byte[] item2)
        {
            PseudoBufferedStream pbs = new PseudoBufferedStream(item2);
            string head = Encoding.ASCII.GetString(BitConverter.GetBytes(pbs.ReadUInt()));

            head = Encoding.ASCII.GetString(BitConverter.GetBytes(pbs.ReadUInt())); //RIFF start
            List<byte[]> Tracks = new List<byte[]>();
            //now process tracks
            while (head == "RIFF")
            {
                uint vara = pbs.ReadUInt();
                byte[] buf = pbs.ReadBytes(vara);
                Tracks.Add(buf);
                if (pbs.Tell() == pbs.Length)
                {
                    break;
                }

                head = Encoding.ASCII.GetString(BitConverter.GetBytes(pbs.ReadUInt()));
            }
            return Tracks;
        }

        private static string SGT_ReadUNAM(Tuple<string, byte[]> uNFO)
        {
            PseudoBufferedStream pbs = new PseudoBufferedStream(uNFO.Item2);
            string unfo = Encoding.ASCII.GetString(BitConverter.GetBytes(pbs.ReadUInt()));
            string unam = Encoding.ASCII.GetString(BitConverter.GetBytes(pbs.ReadUInt()));
            uint vara = pbs.ReadUInt();
            return Encoding.Unicode.GetString(pbs.ReadBytes(vara));
        }

        private static Tuple<string, byte[]> GetSGTSection_pbs(byte[] buffer)
        {
            PseudoBufferedStream pbs = new PseudoBufferedStream(buffer);
            string head = Encoding.ASCII.GetString(BitConverter.GetBytes(pbs.ReadUInt()));
            uint vara = pbs.ReadUInt();
            byte[] buf = pbs.ReadBytes(vara);
            return new Tuple<string, byte[]>(head, buf);
        }

        private static Tuple<string, byte[]> GetSGTSection(BinaryReader br)
        {
            string head = Encoding.ASCII.GetString(br.ReadBytes(4));
            uint vara = br.ReadUInt32();
            byte[] segData = br.ReadBytes((int)vara);
            return new Tuple<string, byte[]>(head, segData);
        }
    }
}