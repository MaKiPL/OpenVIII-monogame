using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Rinoa limit breaks (part 2)
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Rinoa-limit-breaks-%28part-2%29"/>
        public class Rinoa_limit_breaks_part_2
        {
            public const int count = 5; //wiki says 33
            public const int id = 25;
            public const int size = 20;

            public override string ToString() => Name;

            public FF8String Name { get; private set; }
            public MagicID MagicID { get; private set; }
            public AttackType Attack_Type { get; private set; }
            public byte Attack_Power { get; private set; }
            public AttackFlags Attack_Flags { get; private set; }
            public byte Unknown0 { get; private set; }
            public Target Target { get; private set; }
            public byte Unknown1 { get; private set; }
            public byte Hit_Count { get; private set; }
            public Element Element { get; private set; }
            public byte Element_Percent { get; private set; }
            public byte Status_Attack { get; private set; }
            public Persistent_Statuses Statuses0 { get; private set; }
            public BattleOnlyStatuses Statuses1 { get; private set; }
            public Angelo Angelo { get; private set; }
            public int ID { get; private set; }

            public void Read(BinaryReader br,int i)
            {
                ID = i;
                switch(i)
                {
                    case 0:
                        Angelo = Angelo.Cannon;
                        break;
                    case 1:
                        Angelo = Angelo.Strike;
                        break;
                    case 2:
                        Angelo = Angelo.Invincible_Moon;
                        break;
                    case 3:
                        Angelo = Angelo.Wishing_Star;
                        break;
                    case 4:
                        Angelo = Angelo.Angel_Wing;
                        break;
                    default:
                        Angelo = Angelo.None;
                        break;
                }
                ushort offset = br.ReadUInt16();
                Name = new FF8StringReference(Memory.Strings[Strings.FileID.Kernel].GetArchive(), Memory.Strings[Strings.FileID.Kernel].GetFileNames()[0],
                    Memory.Strings[Strings.FileID.Kernel].GetFiles().SubPositions[(int)((Strings.Kernel)Memory.Strings[Strings.FileID.Kernel]).StringLocations[id].Item1]+offset,settings: (FF8StringReference.Settings.Namedic | FF8StringReference.Settings.MultiCharByte));
                //0x0000	2 bytes Offset to name
                //br.BaseStream.Seek(2, SeekOrigin.Current);
                MagicID = (MagicID)br.ReadUInt16();
                //0x0002  2 bytes Magic ID
                Attack_Type = (AttackType)br.ReadByte();
                //0x0004  1 byte Attack type
                Attack_Power = br.ReadByte();
                //0x0005  1 byte Attack power
                Attack_Flags = (AttackFlags)br.ReadByte();
                //0x0006  1 byte Attack flags
                Unknown0 = br.ReadByte();
                //0x0007  1 byte Unknown
                Target = (Target)br.ReadByte();
                //0x0008  1 byte Target info
                Unknown1 = br.ReadByte();
                //0x0009  1 byte Unknown
                Hit_Count = br.ReadByte();
                //0x000A  1 byte Hit Count
                Element = (Element)br.ReadByte();
                //0x000B  1 byte Element Attack
                Element_Percent = br.ReadByte();
                //0x000C  1 byte Element Attack %
                Status_Attack = br.ReadByte();
                //0x000D  1 byte Status Attack Enabler
                Statuses0 = (Persistent_Statuses)br.ReadUInt16();
                //0x000E  2 bytes status_0; //statuses 0-7
                Statuses1 = (BattleOnlyStatuses)br.ReadUInt32();
                //0x0010  4 bytes status_1; //statuses 8-39
            }
            public static List<Rinoa_limit_breaks_part_2> Read(BinaryReader br)
            {
                var ret = new List<Rinoa_limit_breaks_part_2>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Rinoa_limit_breaks_part_2();
                    tmp.Read(br,i);
                    ret.Add(tmp);
                }
                return ret;
            }
        }
    }
}
