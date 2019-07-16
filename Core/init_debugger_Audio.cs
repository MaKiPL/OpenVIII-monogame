using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenVIII
{
#pragma warning disable IDE1006 // Naming Styles

    public static class init_debugger_Audio
#pragma warning restore IDE1006 // Naming Styles
    {
        private static DM_Midi dm_Midi;
        private static Fluid_Midi fluid_Midi;

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
                    using (BinaryWriter bw = new BinaryWriter(new MemoryStream(HeaderData)))
                    {
                        bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                        bw.Write(output_TotalSize);
                        bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                        bw.Write(output_HeaderSize);
                        bw.Write(br.ReadBytes((int)output_HeaderSize));
                        bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                        bw.Write(output_DataSize);
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
            // PC 2000 version has an CD audio track for eyes on me. I don't think we can play that.
            const int unkPrefix = 999;
            const int altLoserPrefix = 512;
            const int loserPrefix = 0;
            const int eyesOnMePrefix = 513;
            string dmusic_pt = "", RaW_ogg_pt = "", music_pt = "", music_wav_pt = "";
            //Roses and Wine V07 moves most of the sgt files to dmusic_backup
            //it leaves a few files behind. I think because RaW doesn't replace everything.
            //ogg files stored in:
            RaW_ogg_pt = Extended.GetUnixFullPath(Path.Combine(Memory.FF8DIR, "RaW", "GLOBAL", "Music"));
            if (!Directory.Exists(RaW_ogg_pt))
            {
                RaW_ogg_pt = null;
            }
            // From what I gather the OGG files and the sgt files have the same numerical prefix. I
            // might try to add the functionality to the debug screen monday.

            dmusic_pt = Extended.GetUnixFullPath(Path.Combine(Memory.FF8DIRdata, "Music", "dmusic_backup"));
            if (!Directory.Exists(dmusic_pt))
            {
                dmusic_pt = null;
            }

            music_pt = Extended.GetUnixFullPath(Path.Combine(Memory.FF8DIRdata, "Music", "dmusic"));
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
                Memory.musices = Directory.GetFiles(RaW_ogg_pt).Where(x => x.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)).ToArray();
                foreach (string m in Memory.musices)
                {
                    if (ushort.TryParse(Path.GetFileName(m).Substring(0, 3), out ushort key))
                    {
                        //mismatched prefix's go here
                        if (key == altLoserPrefix)
                        {
                            key = loserPrefix; //loser.ogg and sgt don't match.
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
                        if (!Memory.dicMusic.ContainsKey(unkPrefix)) //gets any music w/o prefix
                        {
                            Memory.dicMusic.Add(unkPrefix, new List<string> { m });
                        }
                        else
                        {
                            Memory.dicMusic[unkPrefix].Add(m);
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
                        if (!Memory.dicMusic.ContainsKey(unkPrefix)) //gets any music w/o prefix
                        {
                            Memory.dicMusic.Add(unkPrefix, new List<string> { m });
                        }
                        else
                        {
                            Memory.dicMusic[unkPrefix].Add(m);
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
                        if (m.IndexOf("eyes_on_me", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (!Memory.dicMusic.ContainsKey(eyesOnMePrefix))
                            {
                                Memory.dicMusic.Add(eyesOnMePrefix, new List<string> { m });
                            }
                            else
                            {
                                Memory.dicMusic[eyesOnMePrefix].Add(m);
                            }
                        }
                        else if (!Memory.dicMusic.ContainsKey(unkPrefix)) //gets any music w/o prefix
                        {
                            Memory.dicMusic.Add(unkPrefix, new List<string> { m });
                        }
                        else
                        {
                            Memory.dicMusic[unkPrefix].Add(m);
                        }
                    }
                }
            }
        }

        //I messed around here as figuring out how things worked probably didn't need to mess with this.
        public static void Init_SoundAudio()
        {
            string path = Path.Combine(Memory.FF8DIRdata, "Sound", "audio.fmt");
            if (File.Exists(path))
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

            soundEntriesCount = soundEntries == null ? 0 : soundEntries.Length;
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
                Path.Combine(Memory.FF8DIRdata, "Sound", "audio.dat"));
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

        public static void PlayMusic(ushort? index = null)
        {
            Memory.MusicIndex = index ?? Memory.MusicIndex;

            if (musicplaying && lastplayed == Memory.MusicIndex) return;
            string ext = "";

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
#if _X64
                    if (fluid_Midi == null)
                        fluid_Midi = new Fluid_Midi();
                    fluid_Midi.ReadSegmentFileManually(pt);
                    fluid_Midi.Play();
                    break;
#endif
                    if (Extended.IsLinux)
                    {
                        fluid_Midi.ReadSegmentFileManually(pt);
                        fluid_Midi.Play();
                        break;
                    }
                    else
                    {
                        if (dm_Midi == null)
                            dm_Midi = new DM_Midi();
                        dm_Midi.Play(pt);
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
            SoundChannels.Where(x => x != null).ForEach(action => action.Dispose());

            if (dm_Midi != null)
                dm_Midi.Dispose();
            if (fluid_Midi != null)
                fluid_Midi.Dispose();
        }

        public static void StopMusic()
        {
            musicplaying = false;
            if (ffccMusic != null)
            {
                ffccMusic.Dispose();
                ffccMusic = null;
            }
#if !_X64
            if (dm_Midi != null)
                dm_Midi.Stop();
#else
            if (fluid_Midi != null)
                fluid_Midi.Stop();
#endif
        }
    }
}