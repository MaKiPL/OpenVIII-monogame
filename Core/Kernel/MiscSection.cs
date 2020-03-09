using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Misc Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Misc-section"/>
        public sealed class MiscSection
        {
            #region Fields

            public const int Count = 1;

            public const int ID = 29;

            #endregion Fields

            #region Constructors

            private MiscSection(BinaryReader br, int i)
            {
                StatusTimers = br.ReadBytes(14);
                AtbSpeedMultiplier = br.ReadByte();
                DeadTimer = br.ReadByte();
                StatusLimitEffects = br.ReadBytes(32);
                DuelTimersAndStartMoves = br.ReadBytes(8);
                ShotTimers = br.ReadBytes(4);
            }

            #endregion Constructors

            #region Properties

            public byte AtbSpeedMultiplier { get; }

            public byte DeadTimer { get; }

            public byte[] DuelTimersAndStartMoves { get; }

            public byte[] ShotTimers { get; }

            public byte[] StatusLimitEffects { get; }

            public byte[] StatusTimers { get; }

            #endregion Properties

            #region Methods

            private static MiscSection CreateInstance(BinaryReader br, int i) => new MiscSection(br, i);

            public static IReadOnlyList<MiscSection> Read(BinaryReader br)
                => Enumerable.Range(0, Count).Select(x => CreateInstance(br, x)).ToList();

            #endregion Methods
        }
    }
}