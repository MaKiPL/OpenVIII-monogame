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

        public static bool MusicPlaying => musicplaying;

        public static void Init()
        {
            // PC 2000 version has an CD audio track for eyes on me. I don't think we can play that.
            const int unkPrefix = 999;
            const int altLoserPrefix = 512;
            const int loserPrefix = 0;
            const int eyesOnMePrefix = 513;
            const int altEyesOnMePrefix = 22;
            string[] ext = { ".ogg", ".sgt", ".wav", ".mp3" };
            //Roses and Wine V07 moves most of the sgt files to dmusic_backup
            //it leaves a few files behind. I think because RaW doesn't replace everything.
            //ogg files stored in:
            string RaW_ogg_pt = Extended.GetUnixFullPath(Path.Combine(Memory.FF8DIR, "RaW", "GLOBAL", "Music"));
            // From what I gather the OGG files and the sgt files have the same numerical prefix. I
            // might try to add the functionality to the debug screen monday.

            string dmusic_pt = Extended.GetUnixFullPath(Path.Combine(Memory.FF8DIRdata, "Music", "dmusic_backup"));
            string music_pt = Extended.GetUnixFullPath(Path.Combine(Memory.FF8DIRdata, "Music", "dmusic"));
            string music_wav_pt = Extended.GetUnixFullPath(Path.Combine(Memory.FF8DIRdata, "Music"));

            // goal of dicmusic is to be able to select a track by prefix. it adds an list of files
            // with the same prefix. so you can later on switch out which one you want.
            AddMusicPath(RaW_ogg_pt);
            AddMusicPath(music_wav_pt);
            AddMusicPath(dmusic_pt);
            AddMusicPath(music_pt);
            if (!Memory.dicMusic.ContainsKey(eyesOnMePrefix) && Memory.dicMusic.ContainsKey(altEyesOnMePrefix))
            {
                Memory.dicMusic.Add(eyesOnMePrefix, Memory.dicMusic[altEyesOnMePrefix]);
            }
            void AddMusicPath(string p)
            {
                if (!string.IsNullOrWhiteSpace(p) && Directory.Exists(p))
                {
                    foreach (string m in Directory.GetFiles(p).Where(x => ext.Any(y => x.EndsWith(y, StringComparison.OrdinalIgnoreCase))))
                    {
                        AddMusic(m);
                    }
                }
            }
            void AddMusic(string m)
            {
                if (ushort.TryParse(Path.GetFileName(m).Substring(0, 3), out ushort key))
                {
                    //mismatched prefix's go here
                    if (key == altLoserPrefix)
                    {
                        key = loserPrefix; //loser.ogg and sgt don't match.
                    }
                }
                else if (m.IndexOf("eyes_on_me", StringComparison.OrdinalIgnoreCase) >= 0)
                    key = eyesOnMePrefix;
                else
                    key = unkPrefix;

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

        //I messed around here as figuring out how things worked probably didn't need to mess with this.
        public static void Init_SoundAudio()
        {
            string path = Path.Combine(Memory.FF8DIRdata, "Sound", "audio.fmt");
            if (File.Exists(path))
            {
                FileStream fs = null;

                // fs is disposed by binary reader
                using (BinaryReader br = new BinaryReader(fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    soundEntries = new SoundEntry[br.ReadUInt32()];
                    fs.Seek(36, SeekOrigin.Current);
                    for (int i = 0; i < soundEntries.Length - 1; i++)
                    {
                        uint sz = br.ReadUInt32();
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
                    fs = null;
                }
            }

            soundEntriesCount = soundEntries == null ? 0 : soundEntries.Length;
            ////export sounds
            //int item = 0;
            //using (BinaryReader br = new BinaryReader(File.OpenRead(Path.Combine(Memory.FF8DIRdata, "Sound", "audio.dat"))))
            //    foreach (SoundEntry s in soundEntries)
            //    {
            //        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "FF8Sounds"));
            //        if (s.HeaderData == null || s.Size == 0)
            //        {
            //            using (FileStream fc = File.Create(Path.Combine(Path.GetTempPath(), "FF8Sounds", $"{item++}.txt")))
            //            using (BinaryWriter bw = new BinaryWriter(fc))
            //            {
            //                bw.Write($"There is no info maybe audio.fmt listed {s.Size} size or there was an issue.");
            //            }
            //            continue;
            //        }

            // using (FileStream fc = File.Create(Path.Combine(Path.GetTempPath(), "FF8Sounds",
            // $"{item++}.wav"))) using (BinaryWriter bw = new BinaryWriter(fc)) {
            // bw.Write(s.HeaderData); br.BaseStream.Seek(s.Offset, SeekOrigin.Begin);
            // bw.Write(br.ReadBytes((int)s.Size)); } }
        }

        /// <summary>
        /// Play sound effect
        /// </summary>
        /// <param name="soundID">
        /// ID number of sound
        /// <para>The real game uses soundID + 1, so you may need to -1 from any scripts.</para>
        /// </param>
        /// <param name="persist">
        /// <para>If set sound will not be saved to SoundChannels</para>
        /// <para>
        /// It will be up to the calling object to keep track of the sound object that is returned.
        /// </para>
        /// </param>
        /// <param name="loop">If loop, sound will loop from the set sample number.</param>
        public static Ffcc PlaySound(int soundID, float volume = 1.0f, float pitch = 0.0f, float pan = 0.0f, bool persist = false, bool loop = false)
        {
            if (soundEntries == null || soundEntries[soundID].Size == 0)
            {
                return null;
            }
            Ffcc ffcc = FfAudio.Play(
                new Ffcc.Buffer_Data { DataSeekLoc = soundEntries[soundID].Offset, DataSize = soundEntries[soundID].Size, HeaderSize = (uint)soundEntries[soundID].HeaderData.Length },
                soundEntries[soundID].HeaderData,
                Path.Combine(Memory.FF8DIRdata, "Sound", "audio.dat"), loop ? 0 : -1);
            if (!persist)
                SoundChannels[CurrentSoundChannel++] = ffcc;
            ffcc.Play(volume, pitch, pan);
            return ffcc;
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

        public static void PlayStopMusic(ushort? index = null, float volume = 0.5f, float pitch = 0.0f, float pan = 0.0f)
        {
            if (!musicplaying || lastplayed != Memory.MusicIndex)
            {
                PlayMusic(index: index, volume: volume, pitch: pitch, pan: pan);
            }
            else
            {
                StopMusic();
            }
        }

        private static Ffcc ffccMusic = null; // testing using class to play music instead of Naudio / Nvorbis
        private static int _currentSoundChannel;

        public static void PlayMusic(ushort? index = null, float volume = 0.5f, float pitch = 0.0f, float pan = 0.0f, bool loop = true)
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
                case ".wav":
                default:
                    //ffccMusic = new Ffcc(@"c:\eyes_on_me.wav", AVMediaType.AVMEDIA_TYPE_AUDIO, Ffcc.FfccMode.STATE_MACH);
                    if (ffccMusic != null)
                        ffccMusic.Dispose();
                    ffccMusic = FfAudio.Create(pt, loop ? 0 : -1);
                    if (!loop)
                        ffccMusic.LOOPSTART = -1;
                    ffccMusic.PlayInTask(volume, pitch, pan);
                    break;

                case ".sgt":
#if _X64
                    if (fluid_Midi == null)
                        fluid_Midi = new Fluid_Midi();
                    fluid_Midi.ReadSegmentFileManually(pt);
                    fluid_Midi.Play();
#else
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
                        dm_Midi.Play(pt,loop);
                    }
#endif

                    break;
            }

            musicplaying = true;
            lastplayed = Memory.MusicIndex;
        }

        public static void KillAudio()
        {
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