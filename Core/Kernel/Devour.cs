using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Devour Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Devour"/>
        [StructLayout(LayoutKind.Explicit, Size = 12, Pack = 0)]
        public class Devour
        {
            #region Fields

            public const int Count = 16;
            public const int ID = 28;
            public const int Size = 12;

            #endregion Fields

            #region Constructors

            public FF8String Description => Memory.Strings.ReadByOffset(Strings.FileID.Kernel, ID, _descriptionOffset);

            public override string ToString() => Description;

            public Devour(BinaryReader br, int i)
            {
                //Description = Memory.Strings.Read(Strings.FileID.KERNEL, ID, i);
                //br.BaseStream.Seek(2, SeekOrigin.Current);
                if (!br.Read(out _descriptionOffset)) return;
                Debug.WriteLine(Description); //TODO fix description
                //0x0000  2 bytes Offset to devour description
                if (!br.Read(out _damageOrHeal)) return;

                //0x0002  1 byte Damage or heal HP and Status

                //0x1E - Cure
                //0x1F - Damage
                _quantity = (Quantity)br.ReadByte();
                //0x0003  1 byte HP Heal / DMG Quantity Flag

                //0x00 - 0 %
                //0x01 - 6.25 %
                //0x02 - 12.50 %
                //0x04 - 25 %
                //0x08 - 50 %
                //0x10 - 100 %
                Statuses1 = (BattleOnlyStatuses)br.ReadUInt32();
                //0x0004  4 bytes status_1; //statuses 8-39
                Statuses0 = (PersistentStatuses)br.ReadUInt16();
                //0x0008  2 bytes status_0; //statuses 0-7

                StatFlags = (StatFlags)br.ReadByte();
                //0x000A  1 byte Raised Stat Flag

                //0x00 - None
                //0x01 - STR
                //0x02 - VIT
                //0x04 - MAG
                //0x08 - SPR
                //0x10 - SPD
                HP = br.ReadByte();
                //0x000B  1 byte Raised Stat HP Quantity
            }

            #endregion Constructors

            #region Properties

            [field: FieldOffset(0x3)]
            private readonly Quantity _quantity;

            [field: FieldOffset(0x2)]
            private readonly byte _damageOrHeal;

            public float Amount
            {
                get
                {
                    var a = 0f;
                    if (_quantity.HasFlag(Quantity._0625f)) a += .0625f;
                    if (_quantity.HasFlag(Quantity._1250f)) a += .1250f;
                    if (_quantity.HasFlag(Quantity._1f)) a += 1f;
                    if (_quantity.HasFlag(Quantity._25f)) a += .25f;
                    if (_quantity.HasFlag(Quantity._50f)) a += .50f;
                    return a;
                }
            }

            [field: FieldOffset(0x0)]
            private readonly ushort _descriptionOffset;

            /// <summary>
            /// True for heal, False for damage
            /// </summary>
            public bool DamageOrHeal
            {
                get
                {
                    switch (_damageOrHeal)
                    {
                        case 0x1E:
                            return true;

                        case 0x1F:
                            return false;

                        default:
                            throw new InvalidDataException($"{nameof(Devour)}::{nameof(DamageOrHeal)} Invalid value: 0x{_damageOrHeal:X2} to be 0x1E or 0x1F");
                    }
                }
            }

            [field: FieldOffset(0xB)]
            public byte HP { get; }

            [field: FieldOffset(0xA)]
            public StatFlags StatFlags { get; }

            [field: FieldOffset(0x8)]
            public PersistentStatuses Statuses0 { get; }

            [field: FieldOffset(0x4)]
            public BattleOnlyStatuses Statuses1 { get; }

            #endregion Properties

            #region Methods

            public static IReadOnlyList<Devour> Read(BinaryReader br)
                => Enumerable.Range(0, Count).Select(x => CreateInstance(br, x)).ToList();

            //public override string ToString() => Description;

            private static Devour CreateInstance(BinaryReader br, int i) => new Devour(br, i);

            #endregion Methods
        }
    }
}