using System;
using System.IO;
namespace FF8
{

    internal static partial class Saves
    {
        internal class Data
        {
            internal ushort LocationID;//0x0004
            internal ushort firstcharacterscurrentHP;//0x0006
            internal ushort firstcharactersmaxHP;//0x0008
            internal ushort savecount;//0x000A
            internal uint AmountofGil;//0x000C

            /// <summary>
            /// Stored playtime in seconds. Made into timespan for easy parsing.
            /// </summary>
            internal TimeSpan timeplayed;//0x0020

            internal byte firstcharacterslevel;//0x0024

            /// <summary>
            /// 0xFF = blank; The value should cast to Faces.ID
            /// </summary>
            internal Faces.ID[] charactersportraits;//0x0025//0x0026//0x0027

            /// <summary>
            /// 12 characters 0x00 terminated
            /// </summary>
            internal FF8String Squallsname;//0x0028 //12 characters 0x00 terminated

            internal FF8String Rinoasname;//0x0034 //12 characters 0x00 terminated
            internal FF8String Angelosname;//0x0040 //12 characters 0x00 terminated
            internal FF8String Bokosname;//0x004C //12 characters 0x00 terminated

            // 0 = Disc 1
            internal uint CurrentDisk;//0x0058

            internal uint Currentsave;//0x005C

            internal GFData[] GFs; // 0x0060 -> 0x045C //68 bytes per 16 total
            internal CharacterData[] Characters; // 0x04A0 -> 0x08C8 //152 bytes per 8 total
            internal Shop[] Shops; //0x0960 //400 bytes
            internal byte[] Configuration; //0x0AF0 //20 bytes
            internal Characters[] Party; //0x0B04 // 4 bytes 0xFF terminated.
            internal byte[] KnownWeapons; //0x0B08 // 4 bytes
            internal FF8String Grieversname; //0x0B0C // 12 bytes

            internal ushort Unknown1; //0x0B18  (always 7966?)
            internal ushort Unknown2; //0x0B1A
            internal uint AmountofGil2; //0x0B1C
            internal uint AmountofGil_Laguna; //0x0B20
            internal ushort LimitBreakQuistis; //0x0B24
            internal ushort LimitBreakZell; //0x0B26
            internal byte LimitBreakIrvine; //0x0B28
            internal byte LimitBreakSelphie; //0x0B29
            internal byte LimitBreakAngelocompleted; //0x0B2A
            internal byte LimitBreakAngeloknown; //0x0B2B
            internal byte[] LimitBreakAngelopoints; //0x0B2C
            internal byte[] Itemsbattleorder; //0x0B34
            internal Item[] Items; //0x0B54 198 items (Item ID and Quantity)
            internal TimeSpan Gametime; //0x0CE0
            internal uint Countdown; //0x0CE4
            internal uint Unknown3; //0x0CE8
            internal uint Battlevictorycount; //0x0CEC
            internal ushort Unknown4; //0x0CF0
            internal ushort Battlebattleescaped; //0x0CF2
            internal uint Unknown5; //0x0CF4
            internal uint BattleTonberrykilledcount; //0x0CF8
            internal bool BattleTonberrySrkilled; //0x0CFC (yeah, this is a boolean)
            internal uint Unknown6; //0x0D00
            internal uint BattleR1; //0x0D04 First "Bug" battle (R1 tip)
            internal uint BattleELEMENTAL; //0x0D08 First "Bomb" battle (Elemental tip)
            internal uint BattleMENTAL; //0x0D0C  First "T-Rex" battle (Mental tip)
            internal uint BattleIRVINE; //0x0D10 First "Irvine" battle (Irvine's limit break tip)
            internal byte[] BattleMAGIC; //0x0D14 Magic drawn once
            internal byte[] BattleSCAN; //0x0D1C Ennemy scanned once
            internal byte BattleRAUTO; //0x0D30 Renzokuken auto
            internal byte BattleRINDICATOR; //0x0D31 Renzokuken indicator
            internal byte BattleUNK; //0x0D32 dream/Odin/Phoenix/Gilgamesh/Angelo disabled/Angel Wing enabled/???/???
            internal byte[] Tutorialinfos; //0x0D33
            internal byte SeeDtestlevel; //0x0D43
            internal uint Unknown7; //0x0D44
            internal Characters[] Party2; //0x0D48 (last byte always = 255)
            internal uint Unknown8; //0x0D4C
            internal ushort Module; //0x0D50 (1= field, 2= worldmap, 3= battle)
            internal ushort Currentfield; //0x0D52
            internal ushort Previousfield; //0x0D54
            internal short[] CoordX; //0x0D56 signed  (party1, party2, party3)
            internal short[] CoordY; //0x0D5C signed  (party1, party2, party3)
            internal ushort[] Triangle_ID; //0x0D62  (party1, party2, party3)
            internal byte[] Direction; //0x0D68  (party1, party2, party3)
            internal byte Padding; //0x0D6B
            internal uint Unknown9; //0x0D6C
            internal FieldVars Fieldvars; //0x0D70 http://wiki.ffrtt.ru/index.php/FF8/Variables
            internal Worldmap Worldmap; //0x1270
            internal TripleTriad TripleTriad; //0x12F0
            internal ChocoboWorld ChocoboWorld; //0x1370
            
            internal struct Item { internal byte ID; internal byte QTY; };

            internal void Read(BinaryReader br)
            {
                timeplayed = new TimeSpan();
                GFs = new GFData[16];
                Characters = new CharacterData[8];
                LocationID = br.ReadUInt16();//0x0004
                firstcharacterscurrentHP = br.ReadUInt16();//0x0006
                firstcharactersmaxHP = br.ReadUInt16();//0x0008
                savecount = br.ReadUInt16();//0x000A
                AmountofGil = br.ReadUInt32();//0x000C
                timeplayed = new TimeSpan(0, 0, (int)br.ReadUInt32());//0x0020
                firstcharacterslevel = br.ReadByte();//0x0024
                charactersportraits = Array.ConvertAll(br.ReadBytes(3), Item => (Faces.ID)Item);//0x0025//0x0026//0x0027 0xFF = blank.
                Squallsname = br.ReadBytes(12);//0x0028
                Rinoasname = br.ReadBytes(12);//0x0034
                Angelosname = br.ReadBytes(12);//0x0040
                Bokosname = br.ReadBytes(12);//0x004C
                CurrentDisk = br.ReadUInt32();//0x0058
                Currentsave = br.ReadUInt32();//0x005C
                for (int i = 0; i < GFs.Length; i++)
                {
                    GFs[i].Read(br);
                }
                for (int i = 0; i <= (int)Faces.ID.Edea_Kramer; i++)
                {
                    Characters[i].Read(br,(Characters)i); // 0x04A0 -> 0x08C8 //152 bytes per 8 total
                    Characters[i].Name = Memory.Strings.GetName((Faces.ID)i,this);
                }
                int ShopCount = 400 / (16 + 1 + 3);
                Shops = new Shop[ShopCount]; //0x0960 //400 bytes
                for (int i = 0; i < ShopCount; i++)
                    Shops[i].Read(br);
                Configuration = br.ReadBytes(20); //0x0AF0 //20 bytes
                Party = Array.ConvertAll(br.ReadBytes(4), Item => (Characters)Item); //0x0B04 // 4 bytes 0xFF terminated.
                KnownWeapons = br.ReadBytes(4); //0x0B08 // 4 bytes
                Grieversname = br.ReadBytes(12); //0x0B0C // 12 bytes

                Unknown1 = br.ReadUInt16();//0x0B18  (always 7966?)
                Unknown2 = br.ReadUInt16();//0x0B1A 
                AmountofGil2 = br.ReadUInt32();//0x0B1C //dupilicate
                AmountofGil_Laguna = br.ReadUInt32();//0x0B20 
                LimitBreakQuistis = br.ReadUInt16();//0x0B24 
                LimitBreakZell = br.ReadUInt16();//0x0B26 
                LimitBreakIrvine = br.ReadByte();//0x0B28 
                LimitBreakSelphie = br.ReadByte();//0x0B29 
                LimitBreakAngelocompleted = br.ReadByte();//0x0B2A 
                LimitBreakAngeloknown = br.ReadByte();//0x0B2B 
                LimitBreakAngelopoints = br.ReadBytes(8);//0x0B2C 
                Itemsbattleorder = br.ReadBytes(32);//0x0B34
                Items = new Item[198];
                for (int i = 0; i < 198; i++)
                    Items[0] = new Item { ID = br.ReadByte(), QTY = br.ReadByte() }; //0x0B54 198 items (Item ID and Quantity)
                Gametime = new TimeSpan(0, 0, (int)br.ReadUInt32());//0x0CE0 
                Countdown = br.ReadUInt32();//0x0CE4 
                Unknown3 = br.ReadUInt32();//0x0CE8 
                Battlevictorycount = br.ReadUInt32();//0x0CEC 
                Unknown4 = br.ReadUInt16();//0x0CF0 
                Battlebattleescaped = br.ReadUInt16();//0x0CF2 
                Unknown5 = br.ReadUInt32();//0x0CF4 
                BattleTonberrykilledcount = br.ReadUInt32();//0x0CF8 
                BattleTonberrySrkilled = br.ReadUInt32()>0;//0x0CFC (yeah, this is a boolean)
                Unknown6 = br.ReadUInt32();//0x0D00 
                BattleR1 = br.ReadUInt32();//0x0D04 First "Bug" battle (R1 tip)
                BattleELEMENTAL = br.ReadUInt32();//0x0D08 First "Bomb" battle (Elemental tip)
                BattleMENTAL = br.ReadUInt32();//0x0D0C  First "T-Rex" battle (Mental tip)
                BattleIRVINE = br.ReadUInt32();//0x0D10 First "Irvine" battle (Irvine's limit break tip)
                BattleMAGIC = br.ReadBytes(8);//0x0D14 Magic drawn once
                BattleSCAN = br.ReadBytes(20);//0x0D1C Ennemy scanned once
                BattleRAUTO = br.ReadByte();//0x0D30 Renzokuken auto 
                BattleRINDICATOR = br.ReadByte();//0x0D31 Renzokuken indicator
                BattleUNK = br.ReadByte();//0x0D32 dream/Odin/Phoenix/Gilgamesh/Angelo disabled/Angel Wing enabled/???/???
                Tutorialinfos = br.ReadBytes(16);//0x0D33 
                SeeDtestlevel = br.ReadByte();//0x0D43 
                Unknown7 = br.ReadUInt32();//0x0D44 
                Party2 = Array.ConvertAll(br.ReadBytes(4), Item => (Characters)Item); //0x0D48 (last byte always = 255) //dupicate?
                Unknown8 = br.ReadUInt32();//0x0D4C 
                Module = br.ReadUInt16();//0x0D50 (1= field, 2= worldmap, 3= battle)
                Currentfield = br.ReadUInt16();//0x0D52 
                Previousfield = br.ReadUInt16();//0x0D54 
                CoordX = new short[3];
                for (int i =0; i<3;i++)
                    CoordX[i] = br.ReadInt16();//0x0D56 signed  (party1, party2, party3)
                CoordY = new short [3];
                for (int i = 0; i < 3; i++)
                    CoordY[i] = br.ReadInt16();//0x0D5C signed  (party1, party2, party3)
                Triangle_ID = new ushort[3];
                for (int i = 0; i < 3; i++)
                    Triangle_ID[i] = br.ReadUInt16();//0x0D62  (party1, party2, party3)
                Direction = br.ReadBytes(3 * 1);//0x0D68  (party1, party2, party3)
                Padding = br.ReadByte();//0x0D6B 
                Unknown9 = br.ReadUInt32();//0x0D6C 
                Fieldvars.Read(br); //0x0D70 http://wiki.ffrtt.ru/index.php/FF8/Variables
                Worldmap.Read(br);//br.ReadBytes(128);//0x1270 
                TripleTriad.Read(br); //br.ReadBytes(128);//0x12F0 
                ChocoboWorld.Read(br); //br.ReadBytes(64);//0x1370 

            }
        }
    }
}