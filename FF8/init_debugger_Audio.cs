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
using System.Linq;

namespace FF8
{
#pragma warning disable IDE1006 // Naming Styles

    public static class init_debugger_Audio
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
                        ms.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, 4);
                        ms.Write(getBytes(output_TotalSize), 0, 4);
                        ms.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "), 0, 8);
                        ms.Write(getBytes(output_HeaderSize), 0, 4);
                        ms.Write(br.ReadBytes((int)output_HeaderSize), 0, (int)output_HeaderSize);
                        ms.Write(System.Text.Encoding.ASCII.GetBytes("data"), 0, 4);
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

        public static void Init()
        {
            string dmusic_pt = "", RaW_ogg_pt = "", music_pt = "", music_wav_pt = "";
            //Roses and Wine V07 moves most of the sgt files to dmusic_backup
            //it leaves a few files behind. I think because RaW doesn't replace everything.
            //ogg files stored in:
            RaW_ogg_pt = Extended.GetUnixFullPath(Path.Combine(Memory.FF8DIR, "RaW/GLOBAL/Music"));
            if (!Directory.Exists(RaW_ogg_pt))
            {
                RaW_ogg_pt = null;
            }
            // From what I gather the OGG files and the sgt files have the same numerical prefix. I
            // might try to add the functionality to the debug screen monday.

            dmusic_pt = Extended.GetUnixFullPath(Path.Combine(Memory.FF8DIRdata, "Music","dmusic_backup"));
            if (!Directory.Exists(dmusic_pt))
            {
                dmusic_pt = null;
            }

            music_pt = Extended.GetUnixFullPath(Path.Combine(Memory.FF8DIRdata, "Music","dmusic"));
            if (!Directory.Exists(music_pt))
            {
                music_pt = null;
            }

            music_wav_pt = Extended.GetUnixFullPath(Path.Combine(Memory.FF8DIRdata, "Music"));
            if (!Directory.Exists(music_wav_pt))
            {
                music_wav_pt = null;
            }

            // goal of dicmusic is to be able to select a track by prefix. it adds an list of files
            // with the same prefix. so you can later on switch out which one you want.
            if (RaW_ogg_pt != null)
            {
                Memory.musices = Directory.GetFiles(RaW_ogg_pt).Where(x=> x.EndsWith(".ogg",StringComparison.OrdinalIgnoreCase)).ToArray();
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
                Memory.musices = Directory.GetFiles(dmusic_pt).Where(x => x.EndsWith(".sgt", StringComparison.OrdinalIgnoreCase)).ToArray();

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
                Memory.musices = Directory.GetFiles(music_pt).Where(x => x.EndsWith(".sgt", StringComparison.OrdinalIgnoreCase)).ToArray();

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
                Memory.musices = Directory.GetFiles(music_wav_pt).Where(x => x.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)).ToArray();

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
        public static void Init_SoundAudio()
        {
            string path = Path.Combine(Memory.FF8DIRdata, "Sound","audio.fmt");
            if(File.Exists(path))
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

            soundEntriesCount = soundEntries ==null ? 0: soundEntries.Length;
        }

        public static void PlaySound(int soundID)
        {
            if (soundEntries == null || soundEntries[soundID].Size == 0)
            {
                return;
            }
            SoundChannels[CurrentSoundChannel] = new Ffcc(
                new Ffcc.Buffer_Data { DataSeekLoc = soundEntries[soundID].Offset, DataSize = soundEntries[soundID].Size, HeaderSize = (uint)soundEntries[soundID].HeaderData.Length },
                soundEntries[soundID].HeaderData,
                Path.Combine(Memory.FF8DIRdata, "Sound","audio.dat"));
            SoundChannels[CurrentSoundChannel++].Play();
        }

        public static void StopSound()
        {
            //waveout.Stop();
        }

        public static void Update()
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
                StopMusic();
            }
        }

        private static Ffcc ffccMusic = null; // testing using class to play music instead of Naudio / Nvorbis
        private static int _currentSoundChannel;

        public static void PlayMusic()
        {
            string ext = "";
            bool bFakeLinux = false; //set to force linux behaviour on windows; To delete after Linux music playable

            if (Memory.dicMusic.Count > 0 && Memory.dicMusic[Memory.MusicIndex].Count > 0)
            {
                ext = Path.GetExtension(Memory.dicMusic[Memory.MusicIndex][0]).ToLower();
            }
            else
                return;

            string pt = Memory.dicMusic[Memory.MusicIndex][0];

            StopMusic();

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
                    if(Extended.IsLinux || bFakeLinux)
                    {
                        ReadSegmentFileManually(pt);
                        break;
                    }
                    if (!Extended.IsLinux)
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
                            string pathDLS = Path.Combine(Memory.FF8DIRdata, "Music/dmusic_backup/FF8.dls");
                            if (!File.Exists(pathDLS))
                            {
                                pathDLS = Path.Combine(Memory.FF8DIRdata, "Music/dmusic/FF8.dls");
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

                            outport.SetPortParams(0, 0, 0, DirectMidi.SET.REVERB | DirectMidi.SET.CHORUS, 44100);
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
                if (Extended.IsLinux)
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

        public static void StopMusic()
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
                if (!Extended.IsLinux)
                {
                    cport.StopAll();
                }
            }
            catch { }
#endif
        }
        //MUSIC_TIME=LONG->int32; REFERENCE_TIME=LONGLONG->long
        [StructLayout(LayoutKind.Sequential, Pack =1, Size =24)]
        struct DMUS_IO_SEGMENT_HEADER
        {
            public uint dwRepeats;
            public int mtLength;
            public int mtPlayStart;
            public int mtLoopStart;
            public int mtLoopEnd;
            public uint dwResolution;
        }

        [StructLayout(LayoutKind.Sequential, Pack =1, Size =8)]
        struct DMUS_IO_VERSION
        {
            uint dwVersionMS;
            uint dwVersionLS;
        }

        [StructLayout(LayoutKind.Sequential, Pack =1, Size =32)]
        struct DMUS_IO_TRACK_HEADER
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst =16)]
            byte[] guidClassID;
            uint dwPosition;
            uint dwGroup;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst =4)]
            char[] _ckid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            char[] _fccType;

            public string ckid { get => new string(_ckid); }
            public string fccType { get => new string(_fccType); }

        }

        static DMUS_IO_SEGMENT_HEADER segh = new DMUS_IO_SEGMENT_HEADER();
        static DMUS_IO_VERSION vers = new DMUS_IO_VERSION();
        static List<DMUS_IO_TRACK_HEADER> trkh;
        /// <summary>
        /// [LINUX]: This method manually reads DirectMusic Segment files
        /// </summary>
        /// <param name="pt"></param>
        private static void ReadSegmentFileManually(string pt)
        {
            using (FileStream fs = new FileStream(pt, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                if(ReadFourCc(br) != "RIFF")
                {
                    Console.WriteLine($"init_debugger_Audio::ReadSegmentFileManually: NOT RIFF!");
                    return;
                }
                fs.Seek(4, SeekOrigin.Current);
                if(ReadFourCc(br) != "DMSG")
                {
                    Console.WriteLine($"init_debugger_Audio::ReadSegmentFileManually: Broken structure. Expected DMSG!");
                    return;
                }
                ReadSegmentForm(fs, br);
            }
        }

        private static void ReadSegmentForm(FileStream fs, BinaryReader br)
        {
            string fourCc;
            trkh = new List<DMUS_IO_TRACK_HEADER>();
            if ((fourCc = ReadFourCc(br)) != "segh")
                { Console.WriteLine($"init_debugger_Audio::ReadSegmentForm: Broken structure. Expected segh, got={fourCc}");return;}
            uint chunkSize = br.ReadUInt32();
            if (chunkSize != Marshal.SizeOf(segh))
                { Console.WriteLine($"init_debugger_Audio::ReadSegmentForm: chunkSize={chunkSize} is different than DMUS_IO_SEGMENT_HEADER sizeof={Marshal.SizeOf(segh)}");return;}
            segh = Extended.ByteArrayToStructure<DMUS_IO_SEGMENT_HEADER>(br.ReadBytes((int)chunkSize));
            if((fourCc = ReadFourCc(br)) != "guid")
                {Console.WriteLine($"init_debugger_Audio::ReadSegmentForm: expected guid, got={fourCc}");return;}
            byte[] guid = br.ReadBytes(br.ReadInt32());
            if ((fourCc = ReadFourCc(br)) != "LIST")
                { Console.WriteLine($"init_debugger_Audio::ReadSegmentForm: expected LIST, got={fourCc}");return;}
            //let's skip segment data for now, looks like it's not needed, it's not even oficially a part of segh
            fs.Seek(br.ReadUInt32(), SeekOrigin.Current);
            if ((fourCc = ReadFourCc(br)) != "vers")
                { Console.WriteLine($"init_debugger_Audio::ReadSegmentForm: expected vers, got={fourCc}"); return;}
            if ((chunkSize = br.ReadUInt32()) != Marshal.SizeOf(vers))
                { Console.WriteLine($"init_debugger_Audio::ReadSegmentForm: vers expected sizeof={Marshal.SizeOf(vers)}, got={chunkSize}");return;}
            vers = Extended.ByteArrayToStructure<DMUS_IO_VERSION>(br.ReadBytes((int)chunkSize));
            if ((fourCc = ReadFourCc(br)) != "LIST")
                { Console.WriteLine($"init_debugger_Audio::ReadSegmentForm: expected LIST, got={fourCc}");return;}
            //this list should now contain metadata like name, authors and etc. It's completely useless in this project scope
            fs.Seek(br.ReadUInt32(), SeekOrigin.Current); //therefore let's just skip whole UNFO and etc.
            if ((fourCc = ReadFourCc(br)) != "LIST")
                { Console.WriteLine($"init_debugger_Audio::ReadSegmentForm: expected LIST, got={fourCc}"); return; }
            chunkSize = br.ReadUInt32();
            if ((fourCc = ReadFourCc(br)) != "trkl")
            { Console.WriteLine($"init_debugger_Audio::ReadSegmentForm: expected trkl, got={fourCc}"); return; }
            //at this point we are free to read the file up to the end by reading all available DMTK RIFFs;
            uint eof = (uint)fs.Position + chunkSize-4;
            while(fs.Position < eof)
            {
                if ((fourCc = ReadFourCc(br)) != "RIFF")
                { Console.WriteLine($"init_debugger_Audio::ReadSegmentForm: expected RIFF, got={fourCc}"); return; }
                chunkSize = br.ReadUInt32();
                long skipTell = fs.Position;
                Console.WriteLine($"RIFF entry: {ReadFourCc(br)}/{ReadFourCc(br)}");
                trkh.Add(Extended.ByteArrayToStructure<DMUS_IO_TRACK_HEADER>(br.ReadBytes((int)br.ReadUInt32())));
                //TODO HERE
                //this seek below is to ensure that no critical behaviour happens and every RIFF header is read correctly
                fs.Seek(skipTell+chunkSize, SeekOrigin.Begin);

            }
        }

        private static string ReadFourCc(BinaryReader br) => new string(br.ReadChars(4));
    }
}