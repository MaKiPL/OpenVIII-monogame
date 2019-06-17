using System.Collections.Generic;
using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        #region Classes

        /// <summary>
        /// Blue Magic Parameters - Four per spell for each crisis level. Higher Crisis Levels do more damage and/or have more effects.
        /// </summary>
        /// <see cref="https://finalfantasy.fandom.com/wiki/Blue_Magic_(Final_Fantasy_VIII)"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Quistis-limit-break-parameters"/>
        public class Quistis_limit_break_parameters
        {
            #region Fields

            public const int count = 4;//64 total but I want to add these to the Blue_magic_Quistis_limit_break in an array
            public const int id = 20;
            public const int size = 8;

            #endregion Fields

            #region Constructors

            public Quistis_limit_break_parameters() { }
            public Quistis_limit_break_parameters(BinaryReader br, byte i) => Read(br,i);

            #endregion Constructors

            #region Properties

            public byte Attack_Param { get; private set; }
            public byte Crisis_Level { get; private set; }
            public byte Attack_Power { get; private set; }
            public Persistant_Statuses Statuses0 { get; private set; }
            public Battle_Only_Statuses Statuses1 { get; private set; }

            #endregion Properties

            #region Methods

            public static List<Quistis_limit_break_parameters> Read(BinaryReader br)
            {
                var ret = new List<Quistis_limit_break_parameters>(count);

                for (byte i = 0; i < count; i++)
                {
                    var tmp = new Quistis_limit_break_parameters(br,i);
                    ret.Add(tmp);
                }
                return ret;
            }

            public void Read(BinaryReader br, byte i)
            {
                Statuses1 = (Battle_Only_Statuses)br.ReadUInt32();
                //0x0000  4 bytes Status 1
                Statuses0 = (Persistant_Statuses)br.ReadUInt16();
                //0x0004  2 bytes Status 0
                Attack_Power = br.ReadByte();
                //0x0006  1 bytes Attack Power
                Attack_Param = br.ReadByte();
                //0x0007  1 byte Attack Param
                Crisis_Level = i;
            }

            #endregion Methods
        }

        #endregion Classes
    }
}