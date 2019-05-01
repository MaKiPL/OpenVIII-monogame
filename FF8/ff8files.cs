using System;
using System.IO;
using System.Text.RegularExpressions;

namespace FF8
{
    // ref http://wiki.ffrtt.ru/index.php/FF8/Variables copied the table into excel and tried to
    // changed the list into c# code. I was thinking atleast some of it would be useful.


    internal struct FF8Variables
    {
        internal enum Characters
        {
            // I noticed some values were in order of these characters so I made those values into arrays
            // and put the character names into an enum.
            Squall, Zell, Irvine, Quistis, Rinoa, Selphie, Seifer, Edea, Count // Count is the last value and is the number of characters in enum
        }
        unsafe fixed byte byte03[4]; //[0-3]unused in fields (always "FF-8")
        private ulong Steps; //[4]Steps (used to generate random encounters)

        private ulong Payslip; //[8]Payslip
        private unsafe fixed byte byte1215[4]; //[12-15]unused in fields
        private short SeedRankPts; //[16]SeeD rank points?
        private unsafe fixed byte byte1819[2]; //[18-19]unused in fields
        private ulong BattlesWon; //[20]Battles won. (Fun fact: this affects the basketball shot in Trabia.)
        private unsafe fixed byte byte2425[2]; //[24-25]unused in fields
        private ushort EscapedBattles; //[26]Battles escaped.
        private unsafe fixed ushort EnemiesKilled[(int)Characters.Count];

        //ushort ushort28; //[28]Enemies killed by Squall
        //ushort ushort30; //[30]Enemies killed by Zell
        //ushort ushort32; //[32]Enemies killed by Irvine
        //ushort ushort34; //[34]Enemies killed by Quistis
        //ushort ushort36; //[36]Enemies killed by Rinoa
        //ushort ushort38; //[38]Enemies killed by Selphie
        //ushort ushort40; //[40]Enemies killed by Seifer
        //ushort ushort42; //[42]Enemies killed by Edea

        private unsafe fixed ushort DeathCounter[(int)Characters.Count];

        //ushort ushort44; //[44]Squall death count
        //ushort ushort46; //[46]Zell death count
        //ushort ushort48; //[48]Irvine death count
        //ushort ushort50; //[50]Quistis death count
        //ushort ushort52; //[52]Rinoa death count
        //ushort ushort54; //[54]Selphie death count
        //ushort ushort56; //[56]Seifer death count
        //ushort ushort58; //[58]Edea death count

        private unsafe fixed byte byte6067[8]; //[60-67]unused in fields
        private ulong EnemiesKilledTotal; //[68]Enemies killed
        private ulong Gill; //[72]Amount of Gil the party currently has
        private ulong GillLaguna; //[76]Amount of Gil Laguna's party has
        private ulong FMVFrames; //[80]Counts the number of frames since the current movie started playing. (default fps is 15?)
        private ushort LastArea; //[84]Last area visited.
        private sbyte CurrentCarRent; //[86]Current car rent.
        private sbyte sbyte87; //[87]Built-in engine variable. No idea what it does. Scripts always check if it's equal to 0 or 10. Related to music.
        private sbyte sbyte88; //[88]Built-in engine variable. Used exclusively on save points. Never written to with field scripts. Related to Siren's Move-Find ability.
        private unsafe fixed byte byte89103[15]; //[89-103]unused in fields
        private ulong ulong104; //[104]Seems related to SARALYDISPON/SARALYON/MUSICLOAD/PHSPOWER opcodes
        private ulong ulong108; //[108]Music related
        private ulong ulong112; //[112]unused in fields
        private unsafe fixed byte DrawPtsFeild[32]; //[116-147]Draw points in field
        private unsafe fixed byte DrawPtsWorld[31]; //[148-179]Draw points in worldmap
        private unsafe fixed byte byte180255[76]; //[180-255]unused in fields
        private ushort StoryQuestprogress; //[256]Main Story quest progress.
        private byte byte258; //[258]not investigated
        private unsafe fixed byte byte259260[2]; //[259-260]unused in fields
        private byte byte261; //[261]not investigated
        private unsafe fixed byte byte262263[2]; //[262-263]unused in fields
        private byte byte264; //[264]not investigated
        private byte byte265; //[265]not investigated
        private byte WorldMapVersion; //[266]World map version? (3=Esthar locations unlocked)
        private byte byte267; //[267]unused in fields
        private byte byte268; //[268]not investigated
        private byte byte269; //[269]not investigated
        private byte byte270; //[270]not investigated
        private byte byte271; //[271]unused in fields
        private unsafe fixed byte byte272299[28]; //[272-299]SO MANY F***ING CARD GAME VARIABLES
        private byte CardQueenrecards; //[300]Card Queen re-cards.
        private unsafe fixed byte byte301303[3]; //[301-303]unused in fields
        private unsafe fixed byte TimberManiacsIssues[2]; //[304-305]Timber Maniacs issues found.
        private unsafe fixed byte Hacktuar[14]; //[306-319]Reserved for Hacktuar / FF8Voice
        private unsafe fixed byte UltimeciaGallery[3]; //[320-332]Ultimecia Gallery related (pictures viewed?)
        private byte UltimeciaArmory; //[333]Ultimecia Armory chest flags
        private byte UltimeciaCastle; //[334]Ultimecia Castle seals. See SEALEDOFF for details.
        private byte Card; //[335]Card related
        private byte BusRelated; //[336]Deling City bus related
        private unsafe fixed byte GatesOpened[3]; //[338-340]Deling Sewer gates opened
        private byte byte341; //[341]Does lots of things.5
        private byte BusSystem; //[342]Deling City bus system
        private byte byte343; //[343]G-Garden door/event flags.
        private byte byte344; //[344]B-Garden / G-Garden event flags (during GvG)
        private byte byte345; //[345]G-Garden door/event flags.
        private unsafe fixed byte byte346349[4]; //[346-349]FH Instrument (346 Zell, 347 Irvine, 348 Selphie, 349 Quistis)
        private unsafe fixed ushort ushort350356[7]; //[350-356]Health Bars (Garden mech fight)
        private byte byte358; //[358]Space station talk flags, Centra ruins related (beat odin?).
        private byte byte359; //[359]Centra ruins related (beat odin?).
        private ulong ulong360; //[360]Choice of FH music.
        private unsafe fixed byte byte364368[5]; //[364-368]Randomly generated code for Centra Ruins.
        private unsafe fixed byte byte369370[2]; //[369-370]Ultimecia Castle flags
        private byte byte371; //[371]unused in fields
        private unsafe fixed byte byte372376[5]; //[372-376]Ultimecia boss/timer/item flags
        private byte byte377; //[377]Ultimecia organ note controller
        private byte byte378; //[378]Centra Ruins timer (controls blackout messages from Odin)
        private byte byte379; //[379]unused in fields
        private ushort ushort380; //[380]Squall health during mech fight.
        private unsafe fixed byte byte382383[2]; //[382-383]unused in fields
        private byte byte384; //[384]Something about Laguna's time periods and GFs.
        private byte byte385; //[385]Laguna dialogue in pub. Only the +2 bit is ever set. Don't change the +1 bit.
        private byte byte387; //[387]Winhill progress?
        private byte byte388; //[388]Timber Maniacs HQ talk flags (main lobby)
        private byte byte389; //[389]Timber Maniacs HQ talk flags (office room)
        private byte byte390; //[390]Edea talk flags at her house
        private byte byte391; //[391]Laguna talk flags (in his office, disc 3)
        private byte byte392; //[392]unknown (used in Edea's house and in the Balamb Garden computer system)
        private unsafe fixed byte byte393399[7]; //[393-399]unused in fields
        private ulong ulong400and404; //[400 and 404]Related to monsters killed in Winhill, but I don't think it actually does anything. Will investigate.
        private byte byte408; //[408]unused in fields
        private byte byte409; //[409]Balamb Garden computer system
        private unsafe fixed byte byte410431[22]; //[410-431]unused in fields
        private byte byte432; //[432]BG Main hall flags
        private byte byte433; //[433]Flags. Switches are assigned all over BG. No idea what any of them control.
        private byte byte434; //[434]Flags. Switches are assigned all over BG. No idea what any of them control.
        private byte byte435; //[435]Flags. Switches are assigned all over BG. No idea what any of them control.
        private byte byte436; //[436]Moomba friendship level in the prison? Some actions cause these flags to be set.
        private byte byte437; //[437]In BG on Disc 2, keeps track of who's in your party. In the prison, it's the current floor you're on.
        private byte byte438; //[438]Cid vs Norg event flags
        private byte byte439; //[439]Cid vs Norg event flags
        private byte byte440; //[440]Event flags. (+1 Quad ambush, +2 quad item giver, +4/+8 Infirmary helped, +16 Nida, +64 Kadowaki Elixir, +128 Training center)
        private byte byte441; //[441]Cid vs Norg event flags
        private byte byte442; //[442]Rinoa Garden tour flags
        private ushort ushort443; //[443]Zell Health in Prison (Hacktuar)
        private unsafe fixed byte byte445447[3]; //[445-447]Propagator defeated flags
        private ushort ushort448; //[448]Unknown
        private unsafe fixed byte byte450451[2]; //[450-451]Various magazine/talk flags
        private byte byte452; //[452]Lunatic Pandora areas visited?
        private unsafe fixed byte byte453455[3]; //[453-455]Moomba teleport variables
        private unsafe fixed byte byte456457[2]; //[456-457]unused in fields
        private unsafe fixed byte byte458459[2]; //[458-459]Used with MUSICSKIP in some Balamb Garden areas
        private byte byte460; //[460]Random flags (some used for Card Club)
        private unsafe fixed byte byte461473[13]; //[461-473]unused in fields
        private byte byte474; //[474]Random flags (some used for Card Club)
        private unsafe fixed byte byte475478[4]; //[475-478]CC Group variables
        private byte byte479; //[479]If set to 0, disables all random battles during area loading.
        private byte byte480; //[480]State of students in classroom (what they're doing).
        private byte byte481; //[481]Controls a conversation in the cafeteria.
        private short short482; //[482]Error ratio of missiles
        private byte byte484; //[484]Missile Base progression?
        private byte byte485; //[485]ToUK Progression (initially 0b111010101, +2 on finish quest. No other pops)
        private byte byte486; //[486]ToUK room? (used to control map jumps in the maze)
        private byte byte487; //[487]Missile base progression (also does something in BG2F classroom)
        private byte byte488; //[488]Alternate Party Flags. Irvine +1/+16, Quistis +2/+32, Rinoa +4/+64, Zell +8/+128.1
        private byte byte489; //[489]Random talk flags?
        private byte byte490; //[490]Cafeteria cutscene
        private byte byte491; //[491]ToUK stuff
        private byte byte492; //[492]I think this is a door opener for the missile base if you choose a short time limit.
        private byte byte493; //[493]Missile base timer related?
        private unsafe fixed byte byte494527[34]; //[494-527]unused in fields
        private short short528; //[528]Sub-story progression (it's a progression variable for individual segments of the game)
        private byte byte530; //[530]X-ATM related (defeated it in battle?)
        private byte byte531; //[531]Functionally unused. Read from at dollet, only manipulated in debug rooms.
        private byte byte532; //[532]Controls footstep sounds at dollet (sand to concrete)
        private byte byte533; //[533]not investigated
        private byte byte534; //[534]not investigated
        private byte byte535; //[535]not investigated
        private byte byte536; //[536]not investigated
        private byte byte537; //[537]not investigated
        private byte byte538; //[538]not investigated
        private byte byte539; //[539]not investigated
        private unsafe fixed byte byte540591[52]; //[540-591]unused in fields
        private unsafe fixed byte byte592593[2]; //[592-593]Seems to control angles and character facing.
        private byte byte594; //[594]unused in fields
        private byte byte595; //[595]not investigated
        private byte byte596; //[596]not investigated
        private byte byte597; //[597]not investigated
        private byte byte598; //[598]not investigated
        private byte byte599; //[599]not investigated
        private byte byte600; //[600]not investigated
        private byte byte601; //[601]not investigated
        private byte byte602; //[602]not investigated
        private byte byte603; //[603]not investigated
        private byte byte604; //[604]not investigated
        private byte byte605; //[605]not investigated
        private byte byte606; //[606]not investigated
        private byte byte607; //[607]not investigated
        private byte byte608; //[608]not investigated
        private byte byte609; //[609]not investigated
        private byte byte610; //[610]not investigated
        private byte byte611; //[611]not investigated
        private byte byte612; //[612]not investigated
        private byte byte613; //[613]not investigated
        private byte byte614; //[614]not investigated
        private byte byte615; //[615]not investigated
        private byte byte616; //[616]not investigated
        private byte byte617; //[617]not investigated
        private byte byte618; //[618]not investigated
        private byte byte619; //[619]not investigated
        private byte byte620; //[620]not investigated
        private byte byte621; //[621]not investigated
        private byte byte622; //[622]not investigated
        private byte byte623; //[623]not investigated
        private byte byte624; //[624]not investigated
        private byte byte625; //[625]Balamb visited flags (+8 Zell's room)
        private byte byte626; //[626]not investigated
        private byte byte627; //[627]not investigated
        private byte byte628; //[628]unused in fields
        private byte byte629; //[629]not investigated
        private byte byte630; //[630]not investigated
        private byte byte631; //[631]not investigated
        private byte byte632; //[632]not investigated
        private byte byte633; //[633]not investigated
        private ushort ushort634; //[634]not investigated
        private byte byte636; //[636]not investigated
        private byte byte637; //[637]unused in fields
        private byte byte638; //[638]not investigated
        private byte byte639; //[639]unused in fields
        private byte byte640; //[640]not investigated
        private byte byte641; //[641]not investigated
        private byte byte642; //[642]not investigated
        private byte byte643; //[643]not investigated
        private byte byte644; //[644]not investigated
        private byte byte645; //[645]not investigated
        private byte byte646; //[646]not investigated
        private byte byte647; //[647]not investigated
        private byte byte648; //[648]not investigated
        private byte byte649; //[649]not investigated
        private unsafe fixed byte byte650655[6]; //[650-655]unused in fields
        private ushort ushort656; //[656]not investigated
        private byte byte658; //[658]not investigated
        private byte byte659; //[659]not investigated
        private byte byte660; //[660]not investigated
        private byte byte661; //[661]not investigated
        private byte byte662; //[662]not investigated
        private byte byte663; //[663]not investigated
        private byte byte664; //[664]not investigated
        private byte byte665; //[665]not investigated
        private ushort ushort666; //[666]not investigated
        private byte byte668; //[668]not investigated
        private unsafe fixed byte byte669671[3]; //[669-671]unused in fields
        private ushort ushort672; //[672]not investigated
        private byte byte674; //[674]unused in fields
        private byte byte675; //[675]not investigated
        private byte byte676; //[676]unused in fields
        private byte byte677; //[677]not investigated
        private byte byte678; //[678]not investigated
        private byte byte679; //[679]unused in fields
        private byte byte680; //[680]not investigated
        private byte byte681; //[681]not investigated
        private byte byte682; //[682]not investigated
        private byte byte683; //[683]not investigated
        private byte byte684; //[684]not investigated
        private byte byte685; //[685]not investigated
        private byte byte686; //[686]not investigated
        private byte byte687; //[687]not investigated
        private byte byte688; //[688]not investigated
        private byte byte689; //[689]not investigated
        private byte byte690; //[690]not investigated
        private byte byte691; //[691]not investigated
        private unsafe fixed byte byte692719[28]; //[692-719]unused in fields
        private byte byte720; //[720]Squall's costume (0=normal, 1=student, 2=SeeD, 3=Bandage on forehead)
        private byte byte721; //[721]Zell's Costume (0=normal, 1=student, 2=SeeD)
        private byte byte722; //[722]Selphie's costume (0=normal, 1=student, 2=SeeD)
        private byte byte723; //[723]Quistis' Costume (0=normal, 1=SeeD)
        private ushort ushort724; //[724]Dollet mission time
        private ushort ushort726; //[726]not investigated
        private byte byte728; //[728]Does lots of things.3
        private byte byte729; //[729]not investigated
        private byte byte730; //[730]Flags (+1 Joined Garden Festival Committee, +4 Gave Selphie tour of BG, +16 Kadowaki asks for Cid, +32 and +64 Tomb of Unknown Kind hints?, +128 Beat all card people?)
        private byte byte731; //[731]unused in fields
        private ushort ushort732; //[732]not investigated
        private byte byte734; //[734]Split Party Flags (+1 Zell, +2 Irvine, +4 Rinoa, +8 Quistis, +16 Selphie).2
        private byte byte735; //[735]not investigated
        private unsafe fixed byte byte736751[16]; //[736-751]unused in fields
        private byte byte752; //[752]not investigated
        private unsafe fixed byte byte7531023[271]; //[753-1023]unused in fields
        private byte byteAbove1023; //[Above 1023]Temporary variables used pretty much everywhere.
    }

    /// <summary>
    /// parse data from save game files
    /// </summary>
    /// <seealso cref="http://wiki.ffrtt.ru/index.php/FF8/GameSaveFormat#The_save_format"/>
    /// <seealso cref="https://github.com/myst6re/hyne"/>
    /// <seealso cref="https://cdn.discordapp.com/attachments/552838120895283210/570733614656913408/ff8_save.zip"/>
    /// <remarks>
    /// antiquechrono was helping. he even wrote a whole class using kaitai. Though I donno if we
    /// wanna use kaitai.
    /// </remarks>
    internal static class Ff8files
    {
        public class Data
        {
            public ushort LocationID;//0x0004
            public ushort firstcharacterscurrentHP;//0x0006
            public ushort firstcharactersmaxHP;//0x0008
            public ushort savecount;//0x000A
            public uint AmountofGil;//0x000C

            /// <summary>
            /// Stored playtime in seconds. Made into timespan for easy parsing.
            /// </summary>
            public TimeSpan timeplayed;//0x0020

            public byte firstcharacterslevel;//0x0024

            /// <summary>
            /// 0xFF = blank; The value should cast to Faces.ID
            /// </summary>
            public byte[] charactersportraits;//0x0025//0x0026//0x0027

            /// <summary>
            /// 12 characters 0x00 terminated
            /// </summary>
            public FF8String Squallsname;//0x0028 //12 characters 0x00 terminated

            public FF8String Rinoasname;//0x0034 //12 characters 0x00 terminated
            public FF8String Angelosname;//0x0040 //12 characters 0x00 terminated
            public FF8String Bokosname;//0x004C //12 characters 0x00 terminated

            // 0 = Disc 1
            public uint CurrentDisk;//0x0058

            public uint Currentsave;//0x005C

            public GFData[] GFs; // 0x0060 -> 0x045C //68 bytes per 16 total
            public CharacterData[] Characters; // 0x04A0 -> 0x08C8 //152 bytes per 8 total
            public byte[] Shops; //0x0960 //400 bytes
            public byte[] Configuration; //0x0AF0 //20 bytes
            public byte[] Party; //0x0B04 // 4 bytes 0xFF terminated.
            public byte[] KnownWeapons; //0x0B08 // 4 bytes
            public FF8String Grieversname; //0x0B0C // 12 bytes

            public ushort Unknown1; //0x0B18  (always 7966?)
            public ushort Unknown2; //0x0B1A
            public uint AmountofGil2; //0x0B1C
            public uint AmountofGil_Laguna; //0x0B20
            public ushort LimitBreakQuistis; //0x0B24
            public ushort LimitBreakZell; //0x0B26
            public byte LimitBreakIrvine; //0x0B28
            public byte LimitBreakSelphie; //0x0B29
            public byte LimitBreakAngelocompleted; //0x0B2A
            public byte LimitBreakAngeloknown; //0x0B2B
            public byte[] LimitBreakAngelopoints; //0x0B2C
            public byte[] Itemsbattleorder; //0x0B34
            public Item[] Items; //0x0B54 198 items (Item ID and Quantity)
            public TimeSpan Gametime; //0x0CE0
            public uint Countdown; //0x0CE4
            public uint Unknown3; //0x0CE8
            public uint Battlevictorycount; //0x0CEC
            public ushort Unknown4; //0x0CF0
            public ushort Battlebattleescaped; //0x0CF2
            public uint Unknown5; //0x0CF4
            public uint BattleTonberrykilledcount; //0x0CF8
            public bool BattleTonberrySrkilled; //0x0CFC (yeah, this is a boolean)
            public uint Unknown6; //0x0D00
            public uint BattleR1; //0x0D04 First "Bug" battle (R1 tip)
            public uint BattleELEMENTAL; //0x0D08 First "Bomb" battle (Elemental tip)
            public uint BattleMENTAL; //0x0D0C  First "T-Rex" battle (Mental tip)
            public uint BattleIRVINE; //0x0D10 First "Irvine" battle (Irvine's limit break tip)
            public byte[] BattleMAGIC; //0x0D14 Magic drawn once
            public byte[] BattleSCAN; //0x0D1C Ennemy scanned once
            public byte BattleRAUTO; //0x0D30 Renzokuken auto
            public byte BattleRINDICATOR; //0x0D31 Renzokuken indicator
            public byte BattleUNK; //0x0D32 dream/Odin/Phoenix/Gilgamesh/Angelo disabled/Angel Wing enabled/???/???
            public byte[] Tutorialinfos; //0x0D33
            public byte SeeDtestlevel; //0x0D43
            public uint Unknown; //0x0D44
            public uint Party2; //0x0D48 (last byte always = 255)
            public uint Unknown7; //0x0D4C
            public ushort Module; //0x0D50 (1= field, 2= worldmap, 3= battle)
            public ushort Currentfield; //0x0D52
            public ushort Previousfield; //0x0D54
            public short[] CoordX; //0x0D56 signed  (party1, party2, party3)
            public short[] CoordY; //0x0D5C signed  (party1, party2, party3)
            public ushort[] Triangle_ID; //0x0D62  (party1, party2, party3)
            public byte[] Direction; //0x0D68  (party1, party2, party3)
            public byte Padding; //0x0D6B
            public uint Unknown8; //0x0D6C
            public byte[] Fieldvars; //0x0D70 http://wiki.ffrtt.ru/index.php/FF8/Variables
            public byte[] Worldmap; //0x1270
            public byte[] TripleTriad; //0x12F0
            public byte[] ChocoboWorld; //0x1370

            public Data()
            {
                LocationID = 0;
                firstcharacterscurrentHP = 0;
                firstcharactersmaxHP = 0;
                savecount = 0;
                AmountofGil = 0;
                timeplayed = new TimeSpan();
                firstcharacterslevel = 0;
                charactersportraits = null;
                Squallsname = null;
                Rinoasname = null;
                Angelosname = null;
                Bokosname = null;
                CurrentDisk = 0;
                Currentsave = 0;
                GFs = new GFData[16];
                Characters = new CharacterData[8];
                Shops = null;
                Configuration = null;
                Party = null;
                KnownWeapons = null;
                Grieversname = null;
            }

            public struct Item { public byte ID; public byte QTY; };
            public void Read(BinaryReader br)
            {
                LocationID = br.ReadUInt16();//0x0004
                firstcharacterscurrentHP = br.ReadUInt16();//0x0006
                firstcharactersmaxHP = br.ReadUInt16();//0x0008
                savecount = br.ReadUInt16();//0x000A
                AmountofGil = br.ReadUInt32();//0x000C
                timeplayed = new TimeSpan(0, 0, (int)br.ReadUInt32());//0x0020
                firstcharacterslevel = br.ReadByte();//0x0024
                charactersportraits = br.ReadBytes(3);//0x0025//0x0026//0x0027 0xFF = blank.
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
                for (int i = 0; i < 8; i++)
                    Characters[i].Read(br); // 0x04A0 -> 0x08C8 //152 bytes per 8 total
                Shops = br.ReadBytes(400); //0x0960 //400 bytes
                Configuration = br.ReadBytes(20); //0x0AF0 //20 bytes
                Party = br.ReadBytes(4); //0x0B04 // 4 bytes 0xFF terminated.
                KnownWeapons = br.ReadBytes(4); //0x0B08 // 4 bytes
                Grieversname = br.ReadBytes(12); //0x0B0C // 12 bytes

                Unknown = br.ReadUInt16();//0x0B18  (always 7966?)
                Unknown = br.ReadUInt16();//0x0B1A 
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
                Unknown = br.ReadUInt32();//0x0CE8 
                Battlevictorycount = br.ReadUInt32();//0x0CEC 
                Unknown = br.ReadUInt16();//0x0CF0 
                Battlebattleescaped = br.ReadUInt16();//0x0CF2 
                Unknown = br.ReadUInt32();//0x0CF4 
                BattleTonberrykilledcount = br.ReadUInt32();//0x0CF8 
                BattleTonberrySrkilled = br.ReadUInt32()>0;//0x0CFC (yeah, this is a boolean)
                Unknown = br.ReadUInt32();//0x0D00 
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
                Unknown = br.ReadUInt32();//0x0D44 
                Party2 = br.ReadUInt32();//0x0D48 (last byte always = 255) //dupicate
                Unknown = br.ReadUInt32();//0x0D4C 
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
                Unknown = br.ReadUInt32();//0x0D6C 
                //Fieldvars = br.ReadBytes(256 + 1024);//0x0D70 http://wiki.ffrtt.ru/index.php/FF8/Variables
                //Worldmap = br.ReadBytes(128);//0x1270 
                //TripleTriad = br.ReadBytes(128);//0x12F0 
                //ChocoboWorld = br.ReadBytes(64);//0x1370 

            }
        }

        /// <summary>
        /// Data for each Character
        /// </summary>
        /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/GameSaveFormat#Characters"/>
        public struct CharacterData
        {
            public ushort MaxHPs; //0x00
            public uint Experience; //0x02
            public byte ModelID; //0x04
            public byte WeaponID; //0x08
            public byte STR; //0x09
            public byte VIT; //0x0A
            public byte MAG; //0x0B
            public byte SPR; //0x0C
            public byte SPD; //0x0D
            public byte LCK; //0x0E
            public ushort[] Magics; //0x0F
            public byte[] Commands; //0x10
            public byte Paddingorunusedcommand; //0x50
            public uint Abilities; //0x53
            public ushort JunctionnedGFs; //0x54
            public byte Unknown1; //0x58
            public byte Alternativemodel; //0x5A (Normal, SeeD, Soldier...)
            public byte JunctionHP; //0x5B
            public byte JunctionSTR; //0x5C
            public byte JunctionVIT; //0x5D
            public byte JunctionMAG; //0x5E
            public byte JunctionSPR; //0x5F
            public byte JunctionSPD; //0x60
            public byte JunctionEVA; //0x61
            public byte JunctionHIT; //0x62
            public byte JunctionLCK; //0x63
            public byte Junctionelementalattack; //0x64
            public byte Junctionmentalattack; //0x65
            public uint Junctionelementaldefense; //0x66
            public uint Junctionmentaldefense; //0x67
            public byte Unknown2; //0x6B (padding?)
            public ushort[] CompatibilitywithGFs; //0x6F
            public ushort Numberofkills; //0x70
            public ushort NumberofKOs; //0x90
            public byte Exists; //0x92
            public byte Unknown3; //0x94
            public byte MentalStatus; //0x95
            public byte Unknown4; //0x96

            public void Read(BinaryReader br)
            {
                MaxHPs = br.ReadUInt16();//0x02
                Experience = br.ReadUInt32();//0x04
                ModelID = br.ReadByte();//0x08
                WeaponID = br.ReadByte();//0x09
                STR = br.ReadByte();//0x0A
                VIT = br.ReadByte();//0x0B
                MAG = br.ReadByte();//0x0C
                SPR = br.ReadByte();//0x0D
                SPD = br.ReadByte();//0x0E
                LCK = br.ReadByte();//0x0F
                Magics = new ushort[32];
                for (int i = 0; i < 32; i++)
                    Magics[i] = br.ReadUInt16();//0x10
                Commands = br.ReadBytes(3);//0x50
                Paddingorunusedcommand = br.ReadByte();//0x53
                Abilities = br.ReadUInt32();//0x54
                JunctionnedGFs = br.ReadUInt16();//0x58
                Unknown1 = br.ReadByte();//0x5A
                Alternativemodel = br.ReadByte();//0x5B (Normal, SeeD, Soldier...)
                JunctionHP = br.ReadByte();//0x5C
                JunctionSTR = br.ReadByte();//0x5D
                JunctionVIT = br.ReadByte();//0x5E
                JunctionMAG = br.ReadByte();//0x5F
                JunctionSPR = br.ReadByte();//0x60
                JunctionSPD = br.ReadByte();//0x61
                JunctionEVA = br.ReadByte();//0x62
                JunctionHIT = br.ReadByte();//0x63
                JunctionLCK = br.ReadByte();//0x64
                Junctionelementalattack = br.ReadByte();//0x65
                Junctionmentalattack = br.ReadByte();//0x66
                Junctionelementaldefense = br.ReadUInt32();//0x67
                Junctionmentaldefense = br.ReadUInt32();//0x6B
                Unknown2 = br.ReadByte();//0x6F (padding?)
                CompatibilitywithGFs = new ushort[16];
                for (int i = 0; i < 16; i++)
                    CompatibilitywithGFs[i] = br.ReadUInt16();//0x70
                Numberofkills = br.ReadUInt16();//0x90
                NumberofKOs = br.ReadUInt16();//0x92
                Exists = br.ReadByte();//0x94
                Unknown3 = br.ReadByte();//0x95
                MentalStatus = br.ReadByte();//0x96
                Unknown4 = br.ReadByte();//0x97
            }
        }

        /// <summary>
        /// Data for each GF
        /// </summary>
        /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/GameSaveFormat#Guardian_Forces"/>
        public struct GFData
        {
            public FF8String Name; //Offset (0x00 terminated)
            public uint Experience; //0x00
            public byte Unknown; //0x0C
            public byte Exists; //0x10
            public ushort HP; //0x11
            public byte[] Complete; //0x12 abilities (1 bit = 1 ability completed, 9 bits unused)
            public byte[] APs; //0x14 (1 byte = 1 ability of the GF, 2 bytes unused)
            public ushort NumberKills; //0x24 of kills
            public ushort NumberKOs; //0x3C of KOs
            public byte Learning; //0x3E ability
            public byte[] Forgotten; //0x41 abilities (1 bit = 1 ability of the GF forgotten, 2 bits unused)

            public void Read(BinaryReader br)
            {
                Name = br.ReadBytes(12);//0x00 (0x00 terminated)
                Experience = br.ReadUInt32();//0x0C
                Unknown = br.ReadByte();//0x10
                Exists = br.ReadByte();//0x11
                HP = br.ReadUInt16();//0x12
                Complete = br.ReadBytes(16);//0x14 abilities (1 bit = 1 ability completed, 9 bits unused)
                APs = br.ReadBytes(24);//0x24 (1 byte = 1 ability of the GF, 2 bytes unused)
                NumberKills = br.ReadUInt16();//0x3C of kills
                NumberKOs = br.ReadUInt16();//0x3E of KOs
                Learning = br.ReadByte();//0x41 ability
                Forgotten = br.ReadBytes(3);//0x42 abilities (1 bit = 1 ability of the GF forgotten, 2 bits unused)
            }
        }

        /// <summary>
        /// Locations used by save files.
        /// </summary>
        /// <seealso cref="https://github.com/myst6re/hyne/blob/master/Data.cpp"/>
        public static string[] Locations = new string[] {
 ("???")
,("Plaines d'Arkland - Balamb")
,("Monts Gaulg - Balamb")
,("Baie de Rinaul - Balamb")
,("Cap Raha - Balamb")
,("Forêt de Rosfall - Timber")
,("Mandy Beach - Timber")
,("Lac Obel - Timber")
,("Vallée de Lanker - Timber")
,("Ile Nantakhet - Timber")
,("Yaulny Canyon - Timber")
,("Val Hasberry - Dollet")
,("Cap Holy Glory - Dollet")
,("Longhorn Island - Dollet")
,("Péninsule Malgo - Dollet")
,("Plateau Monterosa - Galbadia")

,("Lallapalooza Canyon - Galbadia")
,("Shenand Hill - Timber")
,("Péninsule Gotland - Galbadia")
,("Ile de l'Enfer - Galbadia")
,("Plaine Galbadienne")
,("Wilburn Hill - Galbadia")
,("Archipel Rem - Galbadia")
,("Dingo Désert - Galbadia")
,("Cap Winhill")
,("Archipel Humphrey - Winhill")
,("Ile Winter - Trabia")
,("Val de Solvard - Trabia")
,("Crête d'Eldbeak - Trabia")
 ,("")
,("Plaine d'Hawkind - Trabia")
,("Atoll Albatross - Trabia")

,("Vallon de Bika - Trabia")
,("Péninsule Thor - Trabia")
 ,("")
,("Crête d'Heath - Trabia")
,("Trabia Crater - Trabia")
,("Mont Vienne - Trabia")
,("Plaine de Mordor - Esthar")
,("Mont Nortes - Esthar")
,("Atoll Fulcura - Esthar")
,("Forêt Grandidi - Esthar")
,("Iles Millefeuilles - Esthar")
,("Grandes plaines d'Esthar")
,("Esthar City")
,("Salt Lake - Esthar")
,("Côte Ouest - Esthar")
,("Mont Sollet - Esthar")

,("Vallée d'Abadan - Esthar")
,("Ile Minde - Esthar")
,("Désert Kashkabald - Esthar")
,("Ile Paradisiaque - Esthar")
,("Pic de Talle - Esthar")
,("Atoll Shalmal - Esthar")
,("Vallée de Lolestern - Centra")
,("Aiguille d'Almage - Centra")
,("Vallon Lenown - Centra")
,("Cap de l'espoir - Centra")
,("Mont Yorn - Centra")
,("Ile Pampa - Esthar")
,("Val Serengetti - Centra")
,("Péninsule Nectalle - Centra")
,("Centra Crater - Centra")
,("Ile Poccarahi - Centra")

,("Bibliothèque - BGU")
,("Entrée - BGU")
,("Salle de cours - BGU")
,("Cafétéria - BGU")
,("Niveau MD - BGU")
,("Hall 1er étage - BGU")
,("Hall - BGU")
,("Infirmerie - BGU")
,("Dortoirs doubles - BGU")
,("Dortoirs simples - BGU")
,("Bureau proviseur - BGU")
,("Parking - BGU")
,("Salle de bal - BGU")
,("Campus - BGU")
,("Serre de combat - BGU")
,("Zone secrète - BGU")

,("Corridor - BGU")
,("Temple - BGU")
,("Pont - BGU")
,("Villa Dincht - Balamb")
,("Hôtel - Balamb")
,("Place centrale - Balamb")
,("Place de la gare - Balamb")
,("Port - Balamb")
,("Résidence - Balamb")
,("Train")
,("Voiture")
,("Vaisseau")
,("Mine de souffre")
,("Place du village - Dollet")
,("Zuma Beach")
,("Port - Dollet")

,("Pub - Dollet")
,("Hôtel - Dollet")
,("Résidence - Dollet")
,("Tour satellite - Dollet")
,("Refuge montagneux - Dollet")
,("Centre ville - Timber")
,("Chaîne de TV - Timber")
,("Base des Hiboux - Timber")
,("Pub - Timber")
,("Hôtel - Timber")
,("Train - Timber")
,("Résidence - Timber")
,("Ecran géant - Timber")
,("Centre de presse - Timber")
,("Forêt de Timber")
,("Entrée - Fac de Galbadia")

,("Station Fac de Galbadia")
,("Hall - Fac de Galbadia")
,("Corridor - Fac de Galbadia")
,("Salle d'attente - Fac Galbadia")
,("Salle de cours - Fac Galbadia")
,("Salle de réunion - Fac Galbadia")
,("Dortoirs - Fac de Galbadia")
,("Ascenseur - Fac de Galbadia")
,("Salle recteur - Fac Galbadia")
,("Auditorium - Fac de Galbadia")
,("Stade - Fac de Galbadia")
,("Stand - Fac de Galbadia")
,("2nde entrée - Fac Galbadia")
,("Gymnase - Fac de Galbadia")
,("Palais président - Deling City")
,("Manoir Caraway - Deling City")

,("Gare - Deling City")
,("Place centrale - Deling City")
,("Hôtel - Deling City")
,("Bar - Deling City")
,("Sortie - Deling City")
,("Parade - Deling City")
,("Egout - Deling City")
,("Prison du désert - Galbadia")
,("Désert")
,("Base des missiles")
,("Village de Winhill")
,("Pub - Winhill")
,("Maison vide - Winhill")
,("Manoir - Winhill")
,("Résidence - Winhill")
,("Hôtel - Winhill")

,("Voiture - Winhill")
,("Tombe du roi inconnu")
,("Horizon")
,("Habitations - Horizon")
,("Ecrans solaires - Horizon")
,("Villa du maire - Horizon")
,("Usine - Horizon")
,("Salle des fêtes - Horizon")
,("Hôtel - Horizon")
,("Résidence - Horizon")
,("Gare - Horizon")
,("Aqueduc d'Horizon")
,("Station balnéaire")
,("Salt Lake")
,("Bâtiment mystérieux")
,("Esthar City")

,("Laboratoire Geyser - Esthar")
,("Aérodrome - Esthar")
,("Lunatic Pandora approche")
,("Résidence président - Esthar")
,("Hall - Résidence président")
,("Couloir - Résidence président")
,("Bureau - Résidence président")
,("Accueil - Labo Geyser")
,("Laboratoire Geyser")
,("Deleted")
,("Lunar Gate")
,("Parvis - Lunar Gate")
,("Glacière - Lunar gate")
,("Mausolée - Esthar")
,("Entrée - Mausolée")
,("Pod de confinement - Mausolée")

,("Salle de contrôle - Mausolée")
,("Tears Point")
,("Labo Lunatic Pandora")
,("Zone d'atterrissage de secours")
,("Zone d'atterrissage officielle")
,("Lunatic Pandora")
,("Site des fouilles - Centra")
,("Orphelinat")
,("Salle de jeux - Orphelinat")
,("Dortoir - Orphelinat")
,("Jardin - Orphelinat")
,("Front de mer - Orphelinat")
,("Champ - Orphelinat")
,("Ruines de Centra")
,("Entrée - Fac de Trabia")
,("Cimetière - Fac de Trabia")

,("Garage - Fac de Trabia")
,("Campus - Fac Trabia")
,("Amphithéatre - Fac de Trabia")
,("Stade - Fac de Trabia")
,("Dôme mystérieux")
,("Ville du désert - Shumi Village")
,("Ascenseur - Shumi Village")
,("Shumi Village")
,("Habitation - Shumi Village")
,("Résidence - Shumi Village")
,("Habitat - Shumi Village")
,("Hôtel - Shumi Village")
,("Trabia canyon")
,("Vaisseau des Seeds blancs")
,("Navire des Seeds Blancs")
,("Cabine - Navire Seeds blancs")

,("Cockpit - Hydre")
,("Siège passager - Hydre")
,("Couloir - Hydre")
,("Hangar - Hydre")
,("Accès - Hydre")
,("Air Room - Hydre")
,("Salle de pression - Hydre")
,("Centre de recherches Deep Sea")
,("Laboratoire - Deep Sea")
,("Salle de travail - Deep Sea")
,("Fouilles - Deep Sea")
,("Salle de contrôle - Base lunaire")
,("Centre médical - Base lunaire")
,("Pod - Base lunaire")
,("Dock - Base lunaire")
,("Passage - Base lunaire")

,("Vestiaire - Base lunaire")
,("Habitats - Base lunaire")
,("Hyper Espace")
,("Forêt Chocobo")
,("Jungle")
,("Citadelle d'Ultimecia - Vestibule")
,("Citadelle d'Ultimecia - Hall")
,("Citadelle d'Ultimecia - Terrasse")
,("Citadelle d'Ultimecia - Cave")
,("Citadelle d'Ultimecia - Couloir")
,("Elévateur - Citadelle")
,("Escalier - Citadelle d'Ultimecia")
,("Salle du trésor - Citadelle")
,("Salle de rangement - Citadelle")
,("Citadelle d'Ultimecia - Galerie")
,("Citadelle d'Ultimecia - Ecluse")

,("Citadelle - Armurerie")
,("Citadelle d'Ultimecia - Prison")
,("Citadelle d'Ultimecia - Fossé")
,("Citadelle d'Ultimecia - Jardin")
,("Citadelle d'Ultimecia - Chapelle")
,("Clocher - Citadelle d'Ultimecia")
,("Chambre d'Ultimecia - Citadelle")
 ,("???")
,("Citadelle d'Ultimecia")
,("Salle d'initiation")
,("Reine des cartes")
 ,("???")
 ,("???")
 ,("???")
 ,("???")
 ,("???")
    };

        //C:\Users\pcvii\OneDrive\Documents\Square Enix\FINAL FANTASY VIII Steam\user_1727519
        // might crash linux. just trying to get something working.
        public static string SaveFolder { get; private set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Square Enix", "FINAL FANTASY VIII Steam");

        public static Data[,] FileList { get; private set; }

        public static void Init()
        {
            if (!Directory.Exists(SaveFolder)) Directory.CreateDirectory(SaveFolder);
            string[] dirs = Directory.GetDirectories(SaveFolder);
            if (dirs.Length > 0)
            {
                SaveFolder = Directory.GetDirectories(SaveFolder)[0];
                FileList = new Data[2, 30];
                foreach (string file in Directory.EnumerateFiles(SaveFolder))
                {
                    Match n = Regex.Match(file, @"slot(\d+)_save(\d+).ff8");

                    if (n.Success && n.Groups.Count > 0)
                    {
                        FileList[int.Parse(n.Groups[1].Value) - 1, int.Parse(n.Groups[2].Value) - 1] = read(file);
                    }
                }
            }
        }

        private static Data read(string file)
        {
            byte[] decmp;

            using (FileStream fs = File.OpenRead(file))
            using (BinaryReader br = new BinaryReader(fs))
            {
                uint size = br.ReadUInt32();
                //uint fsLen = BitConverter.ToUInt32(FI, loc * 12);
                //uint fSpos = BitConverter.ToUInt32(FI, (loc * 12) + 4);
                //bool compe = BitConverter.ToUInt32(FI, (loc * 12) + 8) != 0;
                //fs.Seek(0, SeekOrigin.Begin);
                byte[] tmp = br.ReadBytes((int)fs.Length - 4);
                decmp = LZSS.DecompressAllNew(tmp);
            }
            //using (FileStream fs = File.Create(Path.Combine(@"d:\", Path.GetFileName(file))))
            //using (BinaryWriter bw = new BinaryWriter(fs))
            //{
            //    bw.Write(decmp);
            //}
            using (MemoryStream ms = new MemoryStream(decmp))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(0x184, SeekOrigin.Begin);
                Data d = new Data();
                d.Read(br);

                return d;
            }
        }
    }
}