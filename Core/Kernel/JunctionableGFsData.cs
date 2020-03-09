using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Junctionable GFs Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Junctionable-GFs"/>
        public class JunctionableGFsData
        {
            #region Fields

            public const int Count = 16;

            public const int ID = 2;

            #endregion Fields

            #region Constructors

            private JunctionableGFsData(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2);
                Description = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2 + 1);

                br.BaseStream.Seek(4, SeekOrigin.Current);

                MagicID = (MagicID)br.ReadUInt16();             //0x0004  2 bytes[[Magic ID
                AttackType = (AttackType)br.ReadByte();           //0x0006  1 byte  Attack type
                GFPower = br.ReadByte();              //0x0007  1 byte  GF power(used in damage formula)
                Unknown0 = br.ReadBytes(2);            //0x0008  2 bytes Unknown
                AttackFlags = (AttackFlags)(br.ReadByte());          //0x000A  1 byte  Attack Flags
                Unknown1 = br.ReadBytes(2);            //0x000B  2 bytes Unknown
                Element = (Element)br.ReadByte();               //0x000D  1 byte[[Element
                Statuses0 = (Persistent_Statuses)br.ReadUInt16();           //0x000E  2 bytes[[Statuses 0
                Statuses1 = (BattleOnlyStatuses)br.ReadUInt32();           //0x0010  4 bytes[[Statuses 1
                HPMod = br.ReadByte();         //0x0014  1 byte  GF HP Modifier(used in GF HP formula)
                Unknown21 = br.ReadBytes(3);            //0x0015  3 bytes Unknown
                ExpPerLevel = (ushort)((br.ReadByte()) * 10); //0x0018
                Unknown22 = br.ReadBytes(2);            //0x0019  2 bytes Unknown
                StatusAttack = br.ReadByte();         //0x001B  1 byte  Status attack enabler
                Ability = new OrderedDictionary<Abilities, Unlocker>(21);
                for (i = 0; i < 21; i++)
                {
                    Unlocker val = (Unlocker)br.ReadByte();
                    br.BaseStream.Seek(1, SeekOrigin.Current);
                    Abilities key = (Abilities)br.ReadUInt16();
                    Ability.Add(key, val);
                }
                //doomtrain shows this is a decimal number. i got formula from code.
                GFCompatibility = Enumerable.Range(0, 16)
                    .ToDictionary(x => (GFs)x, x => (100 - Convert.ToDecimal(br.ReadByte())) / 5);

                Unknown3 = br.ReadBytes(2);            //0x0080  2 bytes Unknown
                PowerMod = br.ReadByte();              //0x0082  1 byte  Power Mod(used in damage formula)
                LevelMod = br.ReadByte();              //0x0083  1 byte  Level Mod(used in damage formula)
            }

            #endregion Constructors

            #region Properties

            public OrderedDictionary<Abilities, Unlocker> Ability { get; }

            ///0x000A  1 byte  Attack Flags
            public AttackFlags AttackFlags { get; }

            ///0x0006  1 byte  Attack type
            public AttackType AttackType { get; }

            ///0x0002  2 bytes Offset to GF attack description
            public FF8String Description { get; }

            ///0x000D  1 byte[[Element
            public Element Element { get; }

            ///0x18  1 byte *10;
            public ushort ExpPerLevel { get; }

            /// <summary>
            /// <para>0x0070  1 byte  Quezacotl compatibility</para>
            /// <para>0x0071  1 byte  Shiva compatibility</para>
            /// <para>0x0072  1 byte  Ifrit compatibility</para>
            /// <para>0x0073  1 byte  Siren compatibility</para>
            /// <para>0x0074  1 byte  Brothers compatibility</para>
            /// <para>0x0075  1 byte  Diablos compatibility</para>
            /// <para>0x0076  1 byte  Carbuncle compatibility</para>
            /// <para>0x0077  1 byte  Leviathan compatibility</para>
            /// <para>0x0078  1 byte  Pandemona compatibility</para>
            /// <para>0x0079  1 byte  Cerberus compatibility</para>
            /// <para>0x007A  1 byte  Alexander compatibility</para>
            /// <para>0x007B  1 byte  Doomtrain compatibility</para>
            /// <para>0x007C  1 byte  Bahamut compatibility</para>
            /// <para>0x007D  1 byte  Cactuar compatibility</para>
            /// <para>0x007E  1 byte  Tonberry compatibility</para>
            /// <para>0x007F  1 byte  Eden compatibility</para>
            /// </summary>
            public IReadOnlyDictionary<GFs, decimal> GFCompatibility { get; }

            ///0x0007  1 byte  GF power(used in damage formula)
            public byte GFPower { get; }

            ///0x0014  1 byte  GF HP Modifier(used in GF HP formula)
            public byte HPMod { get; }

            ///0x0083  1 byte  Level Mod(used in damage formula)
            public byte LevelMod { get; }

            ///0x0004  2 bytes[[Magic ID
            public MagicID MagicID { get; }

            ///0x0000  2 bytes Offset to GF attack name
            public FF8String Name { get; }

            ///0x0082  1 byte  Power Mod(used in damage formula)
            public byte PowerMod { get; }

            ///0x001B  1 byte  Status attack enabler
            public byte StatusAttack { get; }

            ///0x000E  2 bytes[[Statuses 0
            public Persistent_Statuses Statuses0 { get; }

            ///0x0010  4 bytes[[Statuses 1
            public BattleOnlyStatuses Statuses1 { get; }

            ///0x0008  2 bytes Unknown
            public byte[] Unknown0 { get; }

            ///0x000B  2 bytes Unknown
            public byte[] Unknown1 { get; }

            ///0x0015  3 bytes Unknown
            public byte[] Unknown21 { get; }

            ///0x0019  2 bytes Unknown
            public byte[] Unknown22 { get; }

            ///0x0080  2 bytes Unknown
            public byte[] Unknown3 { get; }

            #endregion Properties

            #region Methods

            public static JunctionableGFsData CreateInstance(BinaryReader br, int i) => new JunctionableGFsData(br, i);

            public static IReadOnlyDictionary<GFs, JunctionableGFsData> Read(BinaryReader br)
                => Enumerable.Range(0, Count).ToDictionary(x => (GFs)x, x => CreateInstance(br, x));

            public override string ToString() => Name;

            #endregion Methods
        }
    }
}