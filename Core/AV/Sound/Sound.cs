using System.IO;
using System.Linq;

namespace OpenVIII.AV
{
    public static partial  class Sound
    {
        #region Fields

        public const int MaxChannels = 20;

        public static int EntriesCount => _entries?.Length ?? 0;

        private static int _currentChannel;

        private static Entry[] _entries;

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
        public static Audio[] SoundChannels { get; } = new Audio[MaxChannels];

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
                ArchiveBase other = ArchiveZzz.Load(Memory.Archives.ZZZ_OTHER);
                if (other.GetBinaryFile("audio.dat", true) != null) //cache's audio.dat
                    s = new MemoryStream(other.GetBinaryFile("audio.fmt"), false);
            }
            if (s != null)
            {

                // fs is disposed by binary reader
                using (BinaryReader br = new BinaryReader(s))
                {
                    _entries = new Entry[br.ReadUInt32()];
                    s.Seek(36, SeekOrigin.Current);
                    for (int i = 0; i < _entries.Length - 1; i++)
                    {
                        uint sz = br.ReadUInt32();
                        if (sz == 0)
                        {
                            s.Seek(34, SeekOrigin.Current); continue;
                        }

                        _entries[i] = new Entry
                        {
                            Size = sz,
                            Offset = br.ReadUInt32()
                        };
                        s.Seek(12, SeekOrigin.Current);
                        _entries[i].fillHeader(br);
                    }
                }
            }
        }

        public static void KillAudio() => SoundChannels?.Where(x => x != null && !x.IsDisposed).ForEach(action => action.Dispose());

        /// <summary>
        /// Play sound effect
        /// </summary>
        /// <param name="soundId">
        /// BattleID number of sound
        /// <para>The real game uses soundID + 1, so you may need to -1 from any scripts.</para>
        /// </param>
        /// <param name="persist">
        /// <para>If set sound will not be saved to SoundChannels</para>
        /// <para>
        /// It will be up to the calling object to keep track of the sound object that is returned.
        /// </para>
        /// </param>
        /// <param name="loop">If loop, sound will loop from the set sample number.</param>
        public static Audio Play(int soundId, float volume = 1.0f, float pitch = 0.0f, float pan = 0.0f, bool persist = false, bool loop = false)
        {
            if (_entries == null || _entries[soundId].Size == 0)
            {
                return null;
            }
            Audio ffcc = Audio.Load(
                new BufferData
                {
                    DataSeekLoc = _entries[soundId].Offset,
                    DataSize = _entries[soundId].Size,
                    HeaderSize = (uint)_entries[soundId].HeaderData.Length,
                },
                _entries[soundId].HeaderData, loop ? 0 : -1);
            if (!persist)
                SoundChannels[CurrentChannel++] = ffcc;
            ffcc.Play(volume, pitch, pan);
            return ffcc;
        }

        #endregion Methods
    }
}