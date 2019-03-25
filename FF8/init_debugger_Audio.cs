using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
#if _WINDOWS
using DirectMidi;
#endif
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using NAudio;
using NAudio.Wave;
using System.Reflection;
using NAudio.Vorbis;

namespace FF8
{
#pragma warning disable IDE1006 // Naming Styles
    internal static class init_debugger_Audio
#pragma warning restore IDE1006 // Naming Styles
    {

#if _WINDOWS
        private static CDirectMusic cdm;
        private static CDLSLoader loader;
        private static CSegment segment;
        private static CAPathPerformance path;
        public static CPortPerformance cport; //public explicit
        private static COutputPort outport;
        private static CCollection ccollection;
        private static CInstrument[] instruments;
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


        public const int S_OK = 0x00000000;


        internal static void DEBUG()
        {
            string dmusic_pt = "", RaW_ogg_pt = "", music_pt = "", music_wav_pt = "";
            //Roses and Wine V07 moves most of the sgt files to dmusic_backup
            //it leaves a few files behind. I think because RaW doesn't replace everything.
            //ogg files stored in:
            RaW_ogg_pt = MakiExtended.GetUnixFullPath(Path.Combine(Memory.FF8DIR, "../../RaW/GLOBAL/Music"));
            if (!Directory.Exists(RaW_ogg_pt))
                RaW_ogg_pt = null;
            // From what I gather the OGG files and the sgt files have the same numerical prefix.
            // I might try to add the functionality to the debug screen monday.

            dmusic_pt = MakiExtended.GetUnixFullPath(Path.Combine(Memory.FF8DIR, "../Music/dmusic_backup/"));
            if (!Directory.Exists(dmusic_pt))
                dmusic_pt = null;

            music_pt = MakiExtended.GetUnixFullPath(Path.Combine(Memory.FF8DIR, "../Music/dmusic/"));
            if (!Directory.Exists(music_pt))
                music_pt = null;

            music_wav_pt = MakiExtended.GetUnixFullPath(Path.Combine(Memory.FF8DIR, "../Music/"));
            if (!Directory.Exists(music_wav_pt))
                music_wav_pt = null;

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
                        if (key == 512) key = 0; //loser.ogg and sgt don't match.
                        if (!Memory.dicMusic.ContainsKey(key))
                            Memory.dicMusic.Add(key, new List<string> { m });
                        else
                            Memory.dicMusic[key].Add(m);
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
                            Memory.dicMusic.Add(key, new List<string> { m });
                        else
                            Memory.dicMusic[key].Add(m);
                    }
                    else
                    {
                        if (!Memory.dicMusic.ContainsKey(999)) //gets any music w/o prefix
                            Memory.dicMusic.Add(999, new List<string> { m });
                        else
                            Memory.dicMusic[999].Add(m);
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
                            Memory.dicMusic.Add(key, new List<string> { m });
                        else
                            Memory.dicMusic[key].Add(m);
                    }
                    else
                    {
                        if (!Memory.dicMusic.ContainsKey(999)) //gets any music w/o prefix
                            Memory.dicMusic.Add(999, new List<string> { m });
                        else
                            Memory.dicMusic[999].Add(m);
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
                            Memory.dicMusic.Add(key, new List<string> { m });
                        else
                            Memory.dicMusic[key].Add(m);
                    }
                    else
                    {
                        if (!Memory.dicMusic.ContainsKey(999)) //gets any music w/o prefix
                            Memory.dicMusic.Add(999, new List<string> { m });
                        else
                            Memory.dicMusic[999].Add(m);
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
                for (int i = 0; i < soundEntries.Length-1; i++)
                {
                    int sz = br.ReadInt32();
                    if(sz == 0) {
                        fs.Seek(34, SeekOrigin.Current); continue; }

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
        internal static void PlaySound(int soundID)
        {
            if (soundEntries == null)
                return;
            if (soundEntries[soundID].Size == 0) return;


            using (FileStream fs = new FileStream(Path.Combine(Memory.FF8DIR, "../Sound/audio.dat"), FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                fs.Seek(soundEntries[soundID].Offset, SeekOrigin.Begin);
                //List<byte[]> sfxBufferList = new List<byte[]>();
                //sfxBufferList.Add(Encoding.ASCII.GetBytes("RIFF"));
                //sfxBufferList.Add(BitConverter.GetBytes
                //    (soundEntries[soundID].Size + 36));
                //sfxBufferList.Add(Encoding.ASCII.GetBytes("WAVEfmt "));
                //sfxBufferList.Add(BitConverter.GetBytes
                //    (18 + 0));
                //sfxBufferList.Add(soundEntries[soundID].WAVFORMATEX);
                //sfxBufferList.Add(Encoding.ASCII.GetBytes("data"));
                //sfxBufferList.Add(BitConverter.GetBytes(soundEntries[soundID].Size));
                GCHandle gc = GCHandle.Alloc(soundEntries[soundID].WAVFORMATEX, GCHandleType.Pinned);
                WAVEFORMATEX format = (WAVEFORMATEX)Marshal.PtrToStructure(gc.AddrOfPinnedObject(), typeof(WAVEFORMATEX));
                gc.Free();
                byte[] rawBuffer = br.ReadBytes(soundEntries[soundID].Size);
                //sfxBufferList.Add(rawBuffer);
                //byte[] sfxBuffer = sfxBufferList.SelectMany(x => x).ToArray();


                //WaveFileReader rad = new WaveFileReader(new MemoryStream(sfxBuffer));
                //passing WAVEFORMATEX struct params makes playing all sounds possible

                //string strDLLpath = Assembly.GetAssembly().CodeBase.Substring(8);
                //if (File.Exists("Msacm32.dll"))
                //try { 
                RawSourceWaveStream raw = new RawSourceWaveStream(new MemoryStream(rawBuffer), new AdpcmWaveFormat((int)format.nSamplesPerSec, format.nChannels));
                byte[] buffer;
                if (!MakiExtended.IsLinux)
                {
                    WaveStream a = WaveFormatConversionStream.CreatePcmStream(raw);
                    //    WaveOut waveout = new WaveOut();
                    //    waveout.Init(a);
                    //    waveout.Play();
                    //}
                    //catch
                    buffer = ReadFullyByte(a);
                }
                else
                {
                    buffer = ReadFullyByte(raw);
                }
                //{
                //try
                //{
                if (buffer != null)
                {
                    SoundEffect se = new SoundEffect(buffer, raw.WaveFormat.SampleRate, (AudioChannels)raw.WaveFormat.Channels);
                    se.Play();
                }
                else
                {
                    //number 28 broken
                    SoundEffect se = new SoundEffect(ReadFullyByte(raw), (int)format.nSamplesPerSec / 2, (AudioChannels)format.nChannels);
                }

                //    }
                //    catch {
                //        try
                //        {
                //            WaveOut waveout = new WaveOut();
                //            waveout.Init(a);
                //            waveout.Play();

                //        }
                //        catch
                //        {
                //            SoundEffect se = new SoundEffect(rawBuffer, (int)format.nSamplesPerSec / 2, (AudioChannels)format.nChannels);
                //        }
                //}
                //SoundEffect se = new SoundEffect(rawBuffer, (int)format.nSamplesPerSec / 2, (AudioChannels)format.nChannels);

                //}

//                SoundEffect se = new SoundEffect(rawBuffer, (int)format.nSamplesPerSec/2, (AudioChannels)format.nChannels);
  //              se.Play();



                //libZPlay.ZPlay zplay = new libZPlay.ZPlay();

                //zplay.OpenFile("D:\\test.wav", libZPlay.TStreamFormat.sfAutodetect);
                //zplay.StartPlayback();
                //SoundEffect se = new SoundEffect(sfxBuffer, 22050, AudioChannels.Mono);
                //sei.Play();
                //se.Play(1.0f, 0.0f, 0.0f);
                //se.Dispose();
            }
        }

        

        public static void StopSound()
        {
            //waveout.Stop();
        }

        internal static void Update()
        {
            if(ffccMusic!=null && ffccMusic.BehindFrame())
                ffccMusic.GetFrame();
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
                return null;
            if (read < size)
                Array.Resize<byte>(ref buffer, read);
            return buffer;
        }
        public static byte[] ReadFullyFloat(VorbisWaveReader stream)
        {
            // following formula goal is to calculate the number of bytes to make buffer. might be wrong.
            long size = (stream.Length / sizeof(float)) +100; //unsure why but read was > than size so added 100; will error if the size is too small.

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
            var pcm = new byte[samplesCount * 2];
            int sampleIndex = 0,
                pcmIndex = 0;

            while (sampleIndex < samplesCount)
            {
                var outsample = (short)(samples[sampleIndex] * short.MaxValue);
                pcm[pcmIndex] = (byte)(outsample & 0xff);
                pcm[pcmIndex + 1] = (byte)((outsample >> 8) & 0xff);

                sampleIndex++;
                pcmIndex += 2;
            }

            return pcm;
        }

        private static SoundEffectInstance see;
        private static bool musicplaying = false;
        private static int lastplayed = -1;

        public static void PlayStopMusic()
        {
            if (!musicplaying || lastplayed != Memory.MusicIndex) PlayMusic();
            else StopAudio(); 
        }
        private static Ffcc ffccMusic = null; // testing using class to play music instead of Naudio / Nvorbis
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
                    //vorbis wave reader uncompresses the 32 bit float wav data from ogg files
                    ffccMusic = new Ffcc(pt, FFmpeg.AutoGen.AVMediaType.AVMEDIA_TYPE_AUDIO, Ffcc.FfccMode.STATE_MACH);
                    var vorbisStream = new VorbisWaveReader(pt);
                    //read fully float reads float[] from vorbis stream and then uses another function to convert to 16 bit pcm
                    //byte[] fileStream = ReadFullyFloat(vorbisStream);
                    int loopstart = -1;
                    int looplen = 0; // 0 length till play till end of song.
                    int loopend = -1;
                    foreach (var c in vorbisStream.Comments)
                    {
                        string[] items = ((string)c).Split('=');
                        switch (items[0])
                        {
                            case "LOOPSTART":
                                int.TryParse(items[1], out loopstart);
                                break;
                            case "LOOPEND":
                                int.TryParse(items[1], out loopend); // I haven't seen this used yet. if it is then break and make sure works.
                                break;
                            case "LOOPLENGTH":
                                int.TryParse(items[1], out looplen); // I haven't seen this used yet. if it is then break and make sure works
                                break;
                                
                        }
                    }
                    if (loopend > 0)
                        looplen = loopend - loopstart; // end - start = length assuming end is the samplecount point of end of loop and not length.
                    //SoundEffect se;
                    //if (loopstart >= 0)
                    //    se = new SoundEffect(fileStream, 0, fileStream.Length, vorbisStream.WaveFormat.SampleRate, (AudioChannels)vorbisStream.WaveFormat.Channels, loopstart, 0);
                    //else
                    //    se = new SoundEffect(fileStream, vorbisStream.WaveFormat.SampleRate, (AudioChannels)vorbisStream.WaveFormat.Channels);
                    //see = se.CreateInstance();
                    //see.IsLooped = loopstart >= 0;
                    //see.Play();
                    ffccMusic.GetFrame();
                    ffccMusic.PlaySound();
                    break ;
                case ".sgt":

                    if (!MakiExtended.IsLinux)
                    {
#if _WINDOWS                        
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
                                pathDLS = Path.Combine(Memory.FF8DIR, "../Music/dmusic/FF8.dls");
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
                                outport.GetPortInfo(++dwPortCount, out infoport);
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
            lastplayed = (int)Memory.MusicIndex;
        }

        public static void KillAudio()
        {
            try
            {
                see.Stop();
                see.Dispose();
                if (MakiExtended.IsLinux)
                {
#if _WINDOWS
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
                ffccMusic.StopSound();
            try
            {
                see.Stop();
            }
            catch{}
            try
            {
                see.Dispose();
            }
            catch { }
#if _WINDOWS
            try
            {
                if (!MakiExtended.IsLinux)

                    cport.StopAll();
            }
            catch {}
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
                    break;
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
