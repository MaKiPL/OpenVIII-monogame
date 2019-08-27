using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Duel (Zell limit break)
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Duel-%28Zell-limit-break%29"/>
        /// <seealso cref="https://finalfantasy.fandom.com/wiki/Duel"/>
        public class Duel_Zell_limit_break
        {
            public const int count = 10;
            public const int id = 22;
            public const int size = 32;

            public override string ToString() => Name;

            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public Magic_ID MagicID { get; private set; }
            public Attack_Type Attack_Type { get; private set; }
            public byte Attack_Power { get; private set; }
            public Attack_Flags Attack_Flags { get; private set; }
            public byte Unknown0 { get; private set; }
            public Target Target { get; private set; }
            public byte Unknown1 { get; private set; }
            public byte Hit_Count { get; private set; }
            public Element Element { get; private set; }
            public byte Element_Percent { get; private set; }
            public byte Status_Attack { get; private set; }
            public Buttons[] Button_Combo { get; private set; }
            public Persistant_Statuses Statuses0 { get; private set; }
            public Battle_Only_Statuses Statuses1 { get; private set; }

            public void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                br.BaseStream.Seek(4, SeekOrigin.Current);
                MagicID = (Magic_ID)br.ReadUInt16();
                //0x0004  2 bytes Magic ID
                Attack_Type = (Attack_Type)br.ReadByte();
                //0x0006  1 byte Attack type
                Attack_Power = br.ReadByte();
                //0x0007  1 byte Attack power
                Attack_Flags = (Attack_Flags)br.ReadByte();
                //0x0008  1 byte Attack flags
                Unknown0 = br.ReadByte();
                //0x0009  1 byte Unknown
                Target = (Target)br.ReadByte();
                //0x000A  1 byte Target Info
                Unknown1 = br.ReadByte();
                //0x000B  1 bytes Unknown
                Hit_Count = br.ReadByte();
                //0x000C  1 byte Hit count
                Element = (Element)br.ReadByte();
                //0x000D  1 byte Element Attack
                Element_Percent = br.ReadByte();
                //0x000E  1 byte Element Attack %
                Status_Attack = br.ReadByte();
                //0x000F  1 byte Status Attack Enabler
                Button_Combo = new Buttons[5];
                for (int b = 0; b < Button_Combo.Length; b++)
                {
                    Button_Flags key = (Button_Flags)br.ReadUInt16();
                    if (Input.Convert_Button.ContainsKey(key))
                        Button_Combo[b] = Input.Convert_Button[key];
                }
                //0x0010  2 bytes Sequence Button 1
                //0x0012  2 bytes Sequence Button 2
                //0x0014  2 bytes Sequence Button 3
                //0x0016  2 bytes Sequence Button 4
                //0x0018  2 bytes Sequence Button 5
                Statuses0 = (Persistant_Statuses)br.ReadUInt16();
                //0x001A  2 bytes status_0; //statuses 0-7
                Statuses1 = (Battle_Only_Statuses)br.ReadUInt32();
                //0x001C  4 bytes status_1; //statuses 8-39
            }
            public static List<Duel_Zell_limit_break> Read(BinaryReader br)
            {
                var ret = new List<Duel_Zell_limit_break>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Duel_Zell_limit_break();
                    tmp.Read(br, i);
                    ret.Add(tmp);
                }
                return ret;
            }
        }
    }
}