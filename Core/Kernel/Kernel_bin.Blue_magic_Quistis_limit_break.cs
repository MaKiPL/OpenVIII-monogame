using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        public enum Blue_Magic : byte
        {
            Laser_Eye,
            Ultra_Waves,
            Electrocute,
            LV_Death,
            Degenerator,
            Aqua_Breath,
            Micro_Missiles,
            Acid,
            Gatling_Gun,
            Fire_Breath,
            Bad_Breath,
            White_Wind,
            Homing_Laser,
            Mighty_Guard,
            Ray_Bomb,
            Shockwave_Pulsar,

            None = 0xFF,
        }
        #region Classes

        /// <summary>
        /// Blue magic (Quistis limit break)
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Blue-magic-%28Quistis-limit-break%29"/>
        public class Blue_magic_Quistis_limit_break
        {
            #region Fields

            public const int count = 16;
            public const int id = 19;
            private List<Quistis_limit_break_parameters> _crisis_Levels;

            public Blue_magic_Quistis_limit_break()
            {
            }

            public Blue_magic_Quistis_limit_break(BinaryReader br, Blue_Magic i) => Read(br, i);
            #endregion Fields

            #region Properties

            public Attack_Flags Attack_Flags { get; private set; }

            public Attack_Type Attack_Type { get; private set; }

            public IReadOnlyList<Quistis_limit_break_parameters> Crisis_Levels { get => _crisis_Levels; }

            public byte Crit { get; private set; }

            public FF8String Description { get; private set; }

            public Element Element { get; private set; }

            public Magic_ID MagicID { get; private set; }
            public Blue_Magic ID { get; private set; }
            public FF8String Name { get; private set; }

            public byte Status_Attack { get; private set; }

            public byte Unknown0 { get; private set; }

            public byte[] Unknown1 { get; private set; }

            public byte Unknown2 { get; private set; }

            public byte Unknown3 { get; private set; }

            #endregion Properties

            #region Methods

            public static Dictionary<Blue_Magic,Blue_magic_Quistis_limit_break> Read(BinaryReader br)
            {
                var ret = new Dictionary<Blue_Magic, Blue_magic_Quistis_limit_break>(count);

                for (Blue_Magic i = 0; (byte)i < count; i++)
                {
                    var tmp = new Blue_magic_Quistis_limit_break(br,i);
                    ret.Add(i, tmp);
                }
                return ret;
            }

            public void Read(BinaryReader br, Blue_Magic i)
            {
                ID = i;
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, (byte)i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, (byte)i * 2 + 1);
                //0x0002	2 bytes Offset to description
                br.BaseStream.Seek(4, SeekOrigin.Current);
                MagicID = (Magic_ID)br.ReadUInt16();
                //0x0004  2 bytes Magic ID
                Unknown0 = br.ReadByte();
                //0x0006  1 byte Unknown
                Attack_Type = (Attack_Type)br.ReadByte();
                //0x0007  1 byte Attack Type
                Unknown1 = br.ReadBytes(2);
                //0x0008  2 bytes Unknown
                Attack_Flags = (Attack_Flags)br.ReadByte();
                //0x000A  1 byte Attack Flags
                Unknown2 = br.ReadByte();
                //0x000B  1 byte Unknown
                Element = (Element)br.ReadByte();
                //0x000C  1 byte Element
                Status_Attack = br.ReadByte();
                //0x000D  1 byte Status Attack
                Crit = br.ReadByte();
                //0x000E  1 byte Crit Bonus
                Unknown3 = br.ReadByte();
                //0x000F  1 byte Unknown
                long current = br.BaseStream.Position;

                br.BaseStream.Seek(Memory.Strings[Strings.FileID.KERNEL].GetFiles().subPositions[Quistis_limit_break_parameters.id] + Quistis_limit_break_parameters.size * (byte)i, SeekOrigin.Begin);
                _crisis_Levels = Quistis_limit_break_parameters.Read(br);
                br.BaseStream.Seek(current, SeekOrigin.Begin);
            }

            public override string ToString() => Name;

            #endregion Methods
        }

        #endregion Classes
    }
}