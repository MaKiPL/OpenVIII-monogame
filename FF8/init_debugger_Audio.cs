using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
#if _WINDOWS
using DirectMidi;
#endif
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using NAudio.Wave;

namespace FF8
{
#pragma warning disable IDE1006 // Naming Styles
    internal static class init_debugger_Audio
#pragma warning restore IDE1006 // Naming Styles
    {
        public enum AudioState
        {
            INIT,
            ELSE,
            PLAYINGSOUND
        }
        public static AudioState State = AudioState.INIT;
#if _WINDOWS
        private struct Midi
        {
            public static CDirectMusic cdm;
            public static CDLSLoader loader;
            public static CSegment segment;
            public static CAPathPerformance path;
            public static CPortPerformance cport; //public explicit
            public static COutputPort outport;
            public static CCollection ccollection;
            public static CInstrument[] instruments;
        }
#endif

        private struct SoundEntry
        {
            public int Size;
            public int Offset;
            public byte[] UNK; //12
            public byte[] WAVFORMATEX; //18
            public ushort SamplesPerBlock;
            public ushort ADPCM;
            public byte[] ADPCMCoefSets; //28
        }
#pragma warning disable CS0649
        private struct WAVEFORMATEX
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
        }
#pragma warning restore CS0649

        private static SoundEntry[] soundEntries;
        public static int soundEntriesCount;
        private static Ffcc ffccMusic = null; // testing using class to play music instead of Naudio / Nvorbis
        private static bool musicplaying = false;
        private static int lastplayed = -1;

        public static readonly DynamicSoundEffectInstance MusicSound = new DynamicSoundEffectInstance(44100, AudioChannels.Stereo);

        public const int S_OK = 0x00000000;


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
            // From what I gather the OGG files and the sgt files have the same numerical prefix.
            // I might try to add the functionality to the debug screen monday.

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

            // goal of dicmusic is to be able to select a track by prefix. 
            // it adds an list of files with the same prefix. so you can later on switch out which one you want.
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
                    int sz = br.ReadInt32();
                    if (sz == 0)
                    {
                        fs.Seek(34, SeekOrigin.Current); continue;
                    }

                    soundEntries[i] = new SoundEntry()
                    {
                        Size = sz,
                        Offset = br.ReadInt32(),
                        UNK = br.ReadBytes(12),
                        WAVFORMATEX = br.ReadBytes(18),
                        SamplesPerBlock = br.ReadUInt16(),
                        ADPCM = br.ReadUInt16(),
                        ADPCMCoefSets = br.ReadBytes(28)
                    };
                }
            }
            soundEntriesCount = soundEntries.Length;
        }
        public static SoundEffect MovieSound { get; set; }
        public static SoundEffectInstance MovieSoundInstance { get; set; }
        private static readonly Dictionary<int, SoundEffect> Sounds = new Dictionary<int, SoundEffect>();
        private static readonly Dictionary<int, SoundEffectInstance> SoundInstances = new Dictionary<int, SoundEffectInstance>();
        //internal static void CreateSoundEffects() //524 MB of just uncompessed sound. probably bad idea.
        //{
        //    if (soundEntries == null)
        //    {
        //        return;
        //    }
        //    for (int soundID = 0; soundID < soundEntries.Length; soundID++)
        //    {
        //        if (soundEntries[soundID].Size == 0)
        //        {
        //            continue;
        //        }
        //        using (FileStream fs = new FileStream(Path.Combine(Memory.FF8DIR, "../Sound/audio.dat"), FileMode.Open, FileAccess.Read))
        //        using (BinaryReader br = new BinaryReader(fs))
        //        {
        //            fs.Seek(soundEntries[soundID].Offset, SeekOrigin.Begin);

        //            GCHandle gc = GCHandle.Alloc(soundEntries[soundID].WAVFORMATEX, GCHandleType.Pinned);
        //            WAVEFORMATEX format = (WAVEFORMATEX)Marshal.PtrToStructure(gc.AddrOfPinnedObject(), typeof(WAVEFORMATEX));
        //            gc.Free();
        //            byte[] rawBuffer = br.ReadBytes(soundEntries[soundID].Size);

        //            using (RawSourceWaveStream raw = new RawSourceWaveStream(new MemoryStream(rawBuffer), new AdpcmWaveFormat((int)format.nSamplesPerSec, format.nChannels)))
        //            {
        //                byte[] buffer;


        //                if (!MakiExtended.IsLinux)
        //                {
        //                    using (WaveStream a = WaveFormatConversionStream.CreatePcmStream(raw))
        //                    {
        //                        buffer = ReadFullyByte(a);
        //                    }
        //                }
        //                else
        //                {
        //                    buffer = ReadFullyByte(raw);
        //                }

        //                if (buffer != null)
        //                {
        //                    Sounds.Add(soundID,new SoundEffect(buffer, raw.WaveFormat.SampleRate, (AudioChannels)raw.WaveFormat.Channels));
        //                }
        //                else
        //                {
        //                    Sounds.Add(soundID,new SoundEffect(ReadFullyByte(raw), (int)format.nSamplesPerSec / 2, (AudioChannels)format.nChannels));
        //                }
        //            }
        //        }
        //    }
        //}
        internal static void PlaySound(int soundID)
        {
            if (soundEntries == null)
            {
                return;
            }

            if (soundEntries[soundID].Size == 0)
            {
                return;
            }
            State = AudioState.PLAYINGSOUND;
            using (FileStream fs = new FileStream(Path.Combine(Memory.FF8DIR, "../Sound/audio.dat"), FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                fs.Seek(soundEntries[soundID].Offset, SeekOrigin.Begin);

                GCHandle gc = GCHandle.Alloc(soundEntries[soundID].WAVFORMATEX, GCHandleType.Pinned);
                WAVEFORMATEX format = (WAVEFORMATEX)Marshal.PtrToStructure(gc.AddrOfPinnedObject(), typeof(WAVEFORMATEX));
                gc.Free();
                byte[] rawBuffer = br.ReadBytes(soundEntries[soundID].Size);

                using (RawSourceWaveStream raw = new RawSourceWaveStream(new MemoryStream(rawBuffer), new AdpcmWaveFormat((int)format.nSamplesPerSec, format.nChannels)))
                {
                    byte[] buffer;


                    if (!MakiExtended.IsLinux)
                    {
                        using (WaveStream a = WaveFormatConversionStream.CreatePcmStream(raw))
                        {
                            buffer = ReadFullyByte(a);
                        }
                    }
                    else
                    {
                        buffer = ReadFullyByte(raw);
                    }
                    if (Sounds.ContainsKey(soundID) && !Sounds[soundID].IsDisposed)
                    {
                        if (SoundInstances.ContainsKey(soundID) && !SoundInstances[soundID].IsDisposed)
                        {
                            SoundInstances[soundID].Play();
                            return;
                        }
                        else
                        {
                            SoundInstances[soundID] = Sounds[soundID].CreateInstance();
                        }
                    }
                    else if (SoundInstances.ContainsKey(soundID) && !SoundInstances[soundID].IsDisposed)
                    {
                        SoundInstances[soundID].Dispose();
                    }

                    if (Sounds.ContainsKey(soundID))
                    {
                        Sounds.Remove(soundID);
                    }

                    if (SoundInstances.ContainsKey(soundID))
                    {
                        SoundInstances.Remove(soundID);
                    }

                    if (buffer != null)
                    {

                        Sounds.Add(soundID, new SoundEffect(buffer, raw.WaveFormat.SampleRate, (AudioChannels)raw.WaveFormat.Channels));
                    }
                    else
                    {
                        Sounds.Add(soundID, new SoundEffect(ReadFullyByte(raw), (int)format.nSamplesPerSec / 2, (AudioChannels)format.nChannels));
                    }
                    SoundInstances.Add(soundID, Sounds[soundID].CreateInstance());
                    SoundInstances[soundID].Play();
                }
            }
        }

        private static readonly Stopwatch Timer = new Stopwatch();
        public static void StopSound()
        {
            //waveout.Stop();
        }
        internal static void Update()
        {
            switch (State)
            {
                case AudioState.INIT:
                    State++;
                    //CreateSoundEffects();
                    Update();
                    break;
                case AudioState.PLAYINGSOUND:
                    //if(!Timer.IsRunning)
                    //    Timer.Start();
                    //else if(Timer.ElapsedMilliseconds >= 1000)
                    State++;
                    break;
                default:
                    ////SoundState.Stopped does not trigger after sound played. it triggers early leaving sound cut off if i run this.
                    ////so I decided to leave the auto dispose off for now. Plan is if it becomes a problem that too many sounds are loaded we can purge them.
                    //if (SoundInstances.Count > 1)
                    //{
                    //    foreach (KeyValuePair<int, SoundEffectInstance> e in SoundInstances)
                    //    {
                    //        if (e.Key == 0) // most used sound ever.
                    //        {
                    //            continue;
                    //        }

                    //        if (!e.Value.IsDisposed || e.Value.State == SoundState.Stopped)
                    //        {
                    //            //remove sound from memory
                    //            Sounds[e.Key].Dispose();
                    //            e.Value.Dispose();
                    //            //pop old instance off. might not need to.
                    //            //SoundInstances.Remove(e.Key); //cannot do this in a foreach loop.
                    //        }
                    //    }
                    //}

                    break;
            }
            if (ffccMusic != null && ffccMusic.LOOPSTART > 0 && ffccMusic.BehindFrame())
            {
                ffccMusic.GetFrame();
            }
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
        //public static byte[] ReadFullyFloat(VorbisWaveReader stream)
        //{
        //    // following formula goal is to calculate the number of bytes to make buffer. might be wrong.
        //    long size = (stream.Length / sizeof(float)) +100; //unsure why but read was > than size so added 100; will error if the size is too small.

        //    float[] buffer = new float[size];

        //    int read = stream.Read(buffer, 0, buffer.Length);
        //    return GetSamplesWaveData(buffer, read);
        //}
        //public static byte[] GetSamplesWaveData(byte[] samples, int samplesCount)
        //{
        //    float[] f = new float[(samplesCount / sizeof(float))];
        //    int i = 0;
        //    for (int n = 0; n < samples.Length; n += sizeof(float))
        //    {
        //        f[i++] = BitConverter.ToSingle(samples, n);
        //    }
        //    return GetSamplesWaveData(f, samplesCount / sizeof(float));
        //}
        //public static byte[] GetSamplesWaveData(float[] samples, int samplesCount)
        //{ // converts 32 bit float samples to 16 bit pcm. I think :P
        //    // https://stackoverflow.com/questions/31957211/how-to-convert-an-array-of-int16-sound-samples-to-a-byte-array-to-use-in-monogam/42151979#42151979
        //    var pcm = new byte[samplesCount * 2];
        //    int sampleIndex = 0,
        //        pcmIndex = 0;

        //    while (sampleIndex < samplesCount)
        //    {
        //        var outsample = (short)(samples[sampleIndex] * short.MaxValue);
        //        pcm[pcmIndex] = (byte)(outsample & 0xff);
        //        pcm[pcmIndex + 1] = (byte)((outsample >> 8) & 0xff);

        //        sampleIndex++;
        //        pcmIndex += 2;
        //    }

        //    return pcm;
        //}


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
                    ffccMusic = new Ffcc(pt, FFmpeg.AutoGen.AVMediaType.AVMEDIA_TYPE_AUDIO, Ffcc.FfccMode.STATE_MACH);
                    ffccMusic.GetFrame(); // prime the pump
                    ffccMusic.StartTimer(); //also starts playing audio because it's hard to tell which stream the audio was sent too.
                    break;
                case ".sgt":

                    if (!MakiExtended.IsLinux)
                    {
#if _WINDOWS
                        if (Midi.cdm == null)
                        {
                            Midi.cdm = new CDirectMusic();
                            Midi.cdm.Initialize();
                            Midi.loader = new CDLSLoader();
                            Midi.loader.Initialize();
                            Midi.loader.LoadSegment(pt, out Midi.segment);
                            Midi.ccollection = new CCollection();
                            string pathDLS = Path.Combine(Memory.FF8DIR, "../Music/dmusic_backup/FF8.dls");
                            if (!File.Exists(pathDLS))
                            {
                                pathDLS = Path.Combine(Memory.FF8DIR, "../Music/dmusic/FF8.dls");
                            }

                            Midi.loader.LoadDLS(pathDLS, out Midi.ccollection);
                            uint dwInstrumentIndex = 0;
                            while (Midi.ccollection.EnumInstrument(++dwInstrumentIndex, out INSTRUMENTINFO iInfo) == S_OK)
                            {
                                Debug.WriteLine(iInfo.szInstName);
                            }
                            Midi.instruments = new CInstrument[dwInstrumentIndex];

                            Midi.path = new CAPathPerformance();
                            Midi.path.Initialize(Midi.cdm, null, null, DMUS_APATH.DYNAMIC_3D, 128);
                            Midi.cport = new CPortPerformance();
                            Midi.cport.Initialize(Midi.cdm, null, null);
                            Midi.outport = new COutputPort();
                            Midi.outport.Initialize(Midi.cdm);

                            uint dwPortCount = 0;
                            INFOPORT infoport;
                            do
                            {
                                Midi.outport.GetPortInfo(++dwPortCount, out infoport);
                            }
                            while ((infoport.dwFlags & DMUS_PC.SOFTWARESYNTH) == 0);

                            Midi.outport.SetPortParams(0, 0, 0, SET.REVERB | SET.CHORUS, 44100);
                            Midi.outport.ActivatePort(infoport);

                            Midi.cport.AddPort(Midi.outport, 0, 1);

                            for (int i = 0; i < dwInstrumentIndex; i++)
                            {
                                Midi.ccollection.GetInstrument(out Midi.instruments[i], i);
                                Midi.outport.DownloadInstrument(Midi.instruments[i]);
                            }
                            Midi.segment.Download(Midi.cport);
                            Midi.cport.PlaySegment(Midi.segment);
                        }
                        else
                        {
                            Midi.cport.Stop(Midi.segment);
                            Midi.segment.Dispose();
                            //segment.ConnectToDLS
                            Midi.loader.LoadSegment(pt, out Midi.segment);
                            Midi.segment.Download(Midi.cport);
                            Midi.cport.PlaySegment(Midi.segment);
                            Midi.cdm.Dispose();
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
        public static void StopAudio()
        {
            musicplaying = false;
            if (ffccMusic != null)
            {
                ffccMusic.StopTimer();
            }

            if(!MusicSound.IsDisposed)
            MusicSound.Stop();
            if (!MovieSound.IsDisposed && !MovieSoundInstance.IsDisposed)
                MovieSoundInstance.Stop();

#if _WINDOWS
            try
            {
                if (!MakiExtended.IsLinux)
                {
                    Midi.cport.StopAll();
                }
            }
            catch { }
#endif
        }
        public static void KillAudio()
        {
            if(!MusicSound.IsDisposed)
            MusicSound.Dispose();
            if(!MovieSound.IsDisposed)
            MovieSound.Dispose();
            if(!MovieSoundInstance.IsDisposed)
            MovieSoundInstance.Dispose();
            try
            {

                if (MakiExtended.IsLinux)
                {
#if _WINDOWS
                    Midi.cport.StopAll();
                    Midi.cport.Dispose();
                    Midi.ccollection.Dispose();
                    Midi.loader.Dispose();
                    Midi.outport.Dispose();
                    Midi.path.Dispose();
                    Midi.cdm.Dispose();
#endif
                }

            }
            catch
            {
            }
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
