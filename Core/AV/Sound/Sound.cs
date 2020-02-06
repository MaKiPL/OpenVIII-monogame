using System.IO;
using System.Linq;

namespace OpenVIII.AV
{
    public static partial  class Sound
    {
        #region Fields

        public const int MaxChannels = 20;

        public static int EntriesCount => Entries?.Length ?? 0;

        private static int _currentChannel;

        private static Entry[] Entries;

        #endregion Fields

        #region Properties

        public static int CurrentChannel
        {
            get => _currentChannel;
            set
            {
                if (value >= MaxChannels)
                {
                    value = 0;
                }
                else if (value < 0)
                {
                    value = MaxChannels - 1;
                }

                _currentChannel = value;
            }
        }

        /// <summary>
        /// This is for short lived sound effects. The Larger the array is the more sounds can be
        /// played at once. If you want sounds to loop of have volume you'll need to have a
        /// SoundEffectInstance added to ffcc, and have those sounds be played like music where they
        /// loop in the background till stop.
        /// </summary>
        public static AV.Audio[] SoundChannels { get; } = new AV.Audio[MaxChannels];

        #endregion Properties

        #region Methods

        public static void Init()
        {
            Memory.Log.WriteLine($"{nameof(Sound)} :: {nameof(Init)}");
            string path = Path.Combine(Memory.FF8DIRdata, "Sound", "audio.fmt");
            Stream s = null;
            if (File.Exists(path))
            {
                s = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            else
            {
                ArchiveBase other = ArchiveZZZ.Load(Memory.Archives.ZZZ_OTHER);
                if (other.GetBinaryFile("audio.dat", true) != null) //cache's audio.dat
                    s = new MemoryStream(other.GetBinaryFile("audio.fmt"), false);
            }
            if (s != null)
            {

                // fs is disposed by binary reader
                using (BinaryReader br = new BinaryReader(s))
                {
                    Entries = new Entry[br.ReadUInt32()];
                    s.Seek(36, SeekOrigin.Current);
                    for (int i = 0; i < Entries.Length - 1; i++)
                    {
                        uint sz = br.ReadUInt32();
                        if (sz == 0)
                        {
                            s.Seek(34, SeekOrigin.Current); continue;
                        }

                        Entries[i] = new Entry
                        {
                            Size = sz,
                            Offset = br.ReadUInt32()
                        };
                        s.Seek(12, SeekOrigin.Current);
                        Entries[i].fillHeader(br);
                    }
                    s = null;

                }

                //EntriesCount = Entries == null ? 0 : Entries.Length;
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
        }

        public static void KillAudio() => SoundChannels?.Where(x => x != null && !x.IsDisposed).ForEach(action => action.Dispose());

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
        public static AV.Audio Play(int soundID, float volume = 1.0f, float pitch = 0.0f, float pan = 0.0f, bool persist = false, bool loop = false)
        {
            if (Entries == null || Entries[soundID].Size == 0)
            {
                return null;
            }
            AV.Audio ffcc = AV.Audio.Play(
                new AV.BufferData
                {
                    DataSeekLoc = Entries[soundID].Offset,
                    DataSize = Entries[soundID].Size,
                    HeaderSize = (uint)Entries[soundID].HeaderData.Length,
                },
                Entries[soundID].HeaderData, loop ? 0 : -1);
            if (!persist)
                SoundChannels[CurrentChannel++] = ffcc;
            ffcc.Play(volume, pitch, pan);
            return ffcc;
        }

        public static void StopSound()
        {
            //waveout.Stop();
        }

        #endregion Methods
    }
}