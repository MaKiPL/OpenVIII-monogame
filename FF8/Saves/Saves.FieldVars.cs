using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF8
{
    public static partial class Saves
    {
        public class FieldVars
        {
            public string byte03; //[0-3]unused in fields (always "FF-8")
            public ulong Steps; //[4]Steps (used to generate random encounters)

            public ulong Payslip; //[8]Payslip
            public byte[] byte1215; //[12-15]unused in fields
            public short SeedRankPts; //[16]SeeD rank points?
            public byte[] byte1819; //[18-19]unused in fields
            public ulong BattlesWon; //[20]Battles won. (Fun fact: this affects the basketball shot in Trabia.)
            public byte[] byte2425; //[24-25]unused in fields
            public ushort EscapedBattles; //[26]Battles escaped.
            public Dictionary<Characters, ushort> EnemiesKilled;

            //ushort ushort28; //[28]Enemies killed by Squall
            //ushort ushort30; //[30]Enemies killed by Zell
            //ushort ushort32; //[32]Enemies killed by Irvine
            //ushort ushort34; //[34]Enemies killed by Quistis
            //ushort ushort36; //[36]Enemies killed by Rinoa
            //ushort ushort38; //[38]Enemies killed by Selphie
            //ushort ushort40; //[40]Enemies killed by Seifer
            //ushort ushort42; //[42]Enemies killed by Edea

            public Dictionary<Characters, ushort> DeathCounter;

            //ushort ushort44; //[44]Squall death count
            //ushort ushort46; //[46]Zell death count
            //ushort ushort48; //[48]Irvine death count
            //ushort ushort50; //[50]Quistis death count
            //ushort ushort52; //[52]Rinoa death count
            //ushort ushort54; //[54]Selphie death count
            //ushort ushort56; //[56]Seifer death count
            //ushort ushort58; //[58]Edea death count

            public byte[] byte6067; //[60-67]unused in fields
            public ulong EnemiesKilledTotal; //[68]Enemies killed
            public ulong Gill; //[72]Amount of Gil the party currently has
            public ulong GillLaguna; //[76]Amount of Gil Laguna's party has
            public ulong FMVFrames; //[80]Counts the number of frames since the current movie started playing. (default fps is 15?)
            public ushort LastArea; //[84]Last area visited.
            public sbyte CurrentCarRent; //[86]Current car rent.
            public sbyte sbyte87; //[87]Built-in engine variable. No idea what it does. Scripts always check if it's equal to 0 or 10. Related to music.
            public sbyte sbyte88; //[88]Built-in engine variable. Used exclusively on save points. Never written to with field scripts. Related to Siren's Move-Find ability.
            public byte[] byte89103; //[89-103]unused in fields
            public ulong ulong104; //[104]Seems related to SARALYDISPON/SARALYON/MUSICLOAD/PHSPOWER opcodes
            public ulong music; //[108]Music related
            public ulong ulong112; //[112]unused in fields
            public byte[] DrawPtsFeild; //[116-147]Draw points in field
            public byte[] DrawPtsWorld; //[148-179]Draw points in worldmap
            public byte[] byte180255; //[180-255]unused in fields
            public ushort StoryQuestprogress; //[256]Main Story quest progress.
            public byte byte258; //[258]not investigated
            public byte[] byte259260; //[259-260]unused in fields
            public byte byte261; //[261]not investigated
            public byte[] byte262263; //[262-263]unused in fields
            public byte byte264; //[264]not investigated
            public byte byte265; //[265]not investigated
            public byte WorldMapVersion; //[266]World map version? (3=Esthar locations unlocked)
            public byte byte267; //[267]unused in fields
            public byte byte268; //[268]not investigated
            public byte byte269; //[269]not investigated
            public byte byte270; //[270]not investigated
            public byte byte271; //[271]unused in fields
            public byte[] byte272299; //[272-299]SO MANY F***ING CARD GAME VARIABLES
            public byte CardQueenrecards; //[300]Card Queen re-cards.
            public byte[] byte301303; //[301-303]unused in fields
            public byte[] TimberManiacsIssues; //[304-305]Timber Maniacs issues found.
            public byte[] Hacktuar; //[306-319]Reserved for Hacktuar / FF8Voice
            public byte[] UltimeciaGallery; //[320-332]Ultimecia Gallery related (pictures viewed?)
            public byte UltimeciaArmory; //[333]Ultimecia Armory chest flags
            public byte UltimeciaCastle; //[334]Ultimecia Castle seals. See SEALEDOFF for details.
            public byte Card; //[335]Card related
            public byte BusRelated; //[336]Deling City bus related
            public byte[] GatesOpened; //[338-340]Deling Sewer gates opened
            public byte byte341; //[341]Does lots of things.5
            public byte BusSystem; //[342]Deling City bus system
            public byte byte343; //[343]G-Garden door/event flags.
            public byte byte344; //[344]B-Garden / G-Garden event flags (during GvG)
            public byte byte345; //[345]G-Garden door/event flags.
            public byte[] byte346349; //[346-349]FH Instrument (346 Zell, 347 Irvine, 348 Selphie, 349 Quistis)
            public ushort[] ushort350356; //[350-356]Health Bars (Garden mech fight)
            public byte byte358; //[358]Space station talk flags, Centra ruins related (beat odin?).
            public byte byte359; //[359]Centra ruins related (beat odin?).
            public ulong ulong360; //[360]Choice of FH music.
            public byte[] byte364368; //[364-368]Randomly generated code for Centra Ruins.
            public byte[] byte369370; //[369-370]Ultimecia Castle flags
            public byte byte371; //[371]unused in fields
            public byte[] byte372376; //[372-376]Ultimecia boss/timer/item flags
            public byte byte377; //[377]Ultimecia organ note controller
            public byte byte378; //[378]Centra Ruins timer (controls blackout messages from Odin)
            public byte byte379; //[379]unused in fields
            public ushort ushort380; //[380]Squall health during mech fight.
            public byte[] byte382383; //[382-383]unused in fields
            public byte byte384; //[384]Something about Laguna's time periods and GFs.
            public byte byte385; //[385]Laguna dialogue in pub. Only the +2 bit is ever set. Don't change the +1 bit.
            public byte byte387; //[387]Winhill progress?
            public byte byte388; //[388]Timber Maniacs HQ talk flags (main lobby)
            public byte byte389; //[389]Timber Maniacs HQ talk flags (office room)
            public byte byte390; //[390]Edea talk flags at her house
            public byte byte391; //[391]Laguna talk flags (in his office, disc 3)
            public byte byte392; //[392]unknown (used in Edea's house and in the Balamb Garden computer system)
            public byte[] byte393399; //[393-399]unused in fields
            public ulong[] ulong400and404; //[400 and 404]Related to monsters killed in Winhill, but I don't think it actually does anything. Will investigate.
            public byte byte408; //[408]unused in fields
            public byte byte409; //[409]Balamb Garden computer system
            public byte[] byte410431; //[410-431]unused in fields
            public byte byte432; //[432]BG Main hall flags
            public byte byte433; //[433]Flags. Switches are assigned all over BG. No idea what any of them control.
            public byte byte434; //[434]Flags. Switches are assigned all over BG. No idea what any of them control.
            public byte byte435; //[435]Flags. Switches are assigned all over BG. No idea what any of them control.
            public byte byte436; //[436]Moomba friendship level in the prison? Some actions cause these flags to be set.
            public byte byte437; //[437]In BG on Disc 2, keeps track of who's in your party. In the prison, it's the current floor you're on.
            public byte byte438; //[438]Cid vs Norg event flags
            public byte byte439; //[439]Cid vs Norg event flags
            public byte byte440; //[440]Event flags. (+1 Quad ambush, +2 quad item giver, +4/+8 Infirmary helped, +16 Nida, +64 Kadowaki Elixir, +128 Training center)
            public byte byte441; //[441]Cid vs Norg event flags
            public byte byte442; //[442]Rinoa Garden tour flags
            public ushort ushort443; //[443]Zell Health in Prison (Hacktuar)
            public byte[] byte445447; //[445-447]Propagator defeated flags
            public ushort ushort448; //[448]Unknown
            public byte[] byte450451; //[450-451]Various magazine/talk flags
            public byte byte452; //[452]Lunatic Pandora areas visited?
            public byte[] byte453455; //[453-455]Moomba teleport variables
            public byte[] byte456457; //[456-457]unused in fields
            public byte[] byte458459; //[458-459]Used with MUSICSKIP in some Balamb Garden areas
            public byte byte460; //[460]Random flags (some used for Card Club)
            public byte[] byte461473; //[461-473]unused in fields
            public byte byte474; //[474]Random flags (some used for Card Club)
            public byte[] byte475478; //[475-478]CC Group variables
            public byte byte479; //[479]If set to 0, disables all random battles during area loading.
            public byte byte480; //[480]State of students in classroom (what they're doing).
            public byte byte481; //[481]Controls a conversation in the cafeteria.
            public short short482; //[482]Error ratio of missiles
            public byte byte484; //[484]Missile Base progression?
            public byte byte485; //[485]ToUK Progression (initially 0b111010101, +2 on finish quest. No other pops)
            public byte byte486; //[486]ToUK room? (used to control map jumps in the maze)
            public byte byte487; //[487]Missile base progression (also does something in BG2F classroom)
            public byte byte488; //[488]Alternate Party Flags. Irvine +1/+16, Quistis +2/+32, Rinoa +4/+64, Zell +8/+128.1
            public byte byte489; //[489]Random talk flags?
            public byte byte490; //[490]Cafeteria cutscene
            public byte byte491; //[491]ToUK stuff
            public byte byte492; //[492]I think this is a door opener for the missile base if you choose a short time limit.
            public byte byte493; //[493]Missile base timer related?
            public byte[] byte494527; //[494-527]unused in fields
            public short short528; //[528]Sub-story progression (it's a progression variable for individual segments of the game)
            public byte byte530; //[530]X-ATM related (defeated it in battle?)
            public byte byte531; //[531]Functionally unused. Read from at dollet, only manipulated in debug rooms.
            public byte byte532; //[532]Controls footstep sounds at dollet (sand to concrete)
            public byte byte533; //[533]not investigated
            public byte byte534; //[534]not investigated
            public byte byte535; //[535]not investigated
            public byte byte536; //[536]not investigated
            public byte byte537; //[537]not investigated
            public byte byte538; //[538]not investigated
            public byte byte539; //[539]not investigated
            public byte[] byte540591; //[540-591]unused in fields
            public byte[] byte592593; //[592-593]Seems to control angles and character facing.
            public byte byte594; //[594]unused in fields
            public byte byte595; //[595]not investigated
            public byte byte596; //[596]not investigated
            public byte byte597; //[597]not investigated
            public byte byte598; //[598]not investigated
            public byte byte599; //[599]not investigated
            public byte byte600; //[600]not investigated
            public byte byte601; //[601]not investigated
            public byte byte602; //[602]not investigated
            public byte byte603; //[603]not investigated
            public byte byte604; //[604]not investigated
            public byte byte605; //[605]not investigated
            public byte byte606; //[606]not investigated
            public byte byte607; //[607]not investigated
            public byte byte608; //[608]not investigated
            public byte byte609; //[609]not investigated
            public byte byte610; //[610]not investigated
            public byte byte611; //[611]not investigated
            public byte byte612; //[612]not investigated
            public byte byte613; //[613]not investigated
            public byte byte614; //[614]not investigated
            public byte byte615; //[615]not investigated
            public byte byte616; //[616]not investigated
            public byte byte617; //[617]not investigated
            public byte byte618; //[618]not investigated
            public byte byte619; //[619]not investigated
            public byte byte620; //[620]not investigated
            public byte byte621; //[621]not investigated
            public byte byte622; //[622]not investigated
            public byte byte623; //[623]not investigated
            public byte byte624; //[624]not investigated
            public byte byte625; //[625]Balamb visited flags (+8 Zell's room)
            public byte byte626; //[626]not investigated
            public byte byte627; //[627]not investigated
            public byte byte628; //[628]unused in fields
            public byte byte629; //[629]not investigated
            public byte byte630; //[630]not investigated
            public byte byte631; //[631]not investigated
            public byte byte632; //[632]not investigated
            public byte byte633; //[633]not investigated
            public ushort ushort634; //[634]not investigated
            public byte byte636; //[636]not investigated
            public byte byte637; //[637]unused in fields
            public byte byte638; //[638]not investigated
            public byte byte639; //[639]unused in fields
            public byte byte640; //[640]not investigated
            public byte byte641; //[641]not investigated
            public byte byte642; //[642]not investigated
            public byte byte643; //[643]not investigated
            public byte byte644; //[644]not investigated
            public byte byte645; //[645]not investigated
            public byte byte646; //[646]not investigated
            public byte byte647; //[647]not investigated
            public byte byte648; //[648]not investigated
            public byte byte649; //[649]not investigated
            public byte[] byte650655; //[650-655]unused in fields
            public ushort ushort656; //[656]not investigated
            public byte byte658; //[658]not investigated
            public byte byte659; //[659]not investigated
            public byte byte660; //[660]not investigated
            public byte byte661; //[661]not investigated
            public byte byte662; //[662]not investigated
            public byte byte663; //[663]not investigated
            public byte byte664; //[664]not investigated
            public byte byte665; //[665]not investigated
            public ushort ushort666; //[666]not investigated
            public byte byte668; //[668]not investigated
            public byte[] byte669671; //[669-671]unused in fields
            public ushort ushort672; //[672]not investigated
            public byte byte674; //[674]unused in fields
            public byte byte675; //[675]not investigated
            public byte byte676; //[676]unused in fields
            public byte byte677; //[677]not investigated
            public byte byte678; //[678]not investigated
            public byte byte679; //[679]unused in fields
            public byte byte680; //[680]not investigated
            public byte byte681; //[681]not investigated
            public byte byte682; //[682]not investigated
            public byte byte683; //[683]not investigated
            public byte byte684; //[684]not investigated
            public byte byte685; //[685]not investigated
            public byte byte686; //[686]not investigated
            public byte byte687; //[687]not investigated
            public byte byte688; //[688]not investigated
            public byte byte689; //[689]not investigated
            public byte byte690; //[690]not investigated
            public byte byte691; //[691]not investigated
            public byte[] byte692719; //[692-719]unused in fields
            public Dictionary<Characters, Costume> Costumes;

            //public byte byte720; //[720]Squall's costume (0=normal, 1=student, 2=SeeD, 3=Bandage on forehead)
            //public byte byte721; //[721]Zell's Costume (0=normal, 1=student, 2=SeeD)
            //public byte byte722; //[722]Selphie's costume (0=normal, 1=student, 2=SeeD)
            //public byte byte723; //[723]Quistis' Costume (0=normal, 1=SeeD)
            public ushort ushort724; //[724]Dollet mission time

            public ushort ushort726; //[726]not investigated
            public byte byte728; //[728]Does lots of things.3
            public byte byte729; //[729]not investigated
            public byte byte730; //[730]Flags (+1 Joined Garden Festival Committee, +4 Gave Selphie tour of BG, +16 Kadowaki asks for Cid, +32 and +64 Tomb of Unknown Kind hints?, +128 Beat all card people?)
            public byte byte731; //[731]unused in fields
            public ushort ushort732; //[732]not investigated
            public byte byte734; //[734]Split Party Flags (+1 Zell, +2 Irvine, +4 Rinoa, +8 Quistis, +16 Selphie).2
            public byte byte735; //[735]not investigated
            public byte[] byte736751; //[736-751]unused in fields
            public byte byte752; //[752]not investigated
            public byte[] byte7531023; //[753-1023]unused in fields
            public byte[] byteAbove1023; //[Above 1023]Temporary variables used pretty much everywhere.

            public FieldVars()
            { }

            public FieldVars(BinaryReader br) => Read(br);

            public FieldVars Clone()
            {
                //shadowcopy
                FieldVars f = (FieldVars)MemberwiseClone();
                //deepcopy
                f.Costumes = Costumes.ToDictionary(e => e.Key, e => e.Value);
                f.EnemiesKilled = EnemiesKilled.ToDictionary(e => e.Key, e => e.Value);
                f.DeathCounter = DeathCounter.ToDictionary(e => e.Key, e => e.Value);
                return f;
            }

            public void Read(BinaryReader br)
            {
                byte03 = System.Text.Encoding.UTF8.GetString(br.ReadBytes(4)); //[0-3]unused in fields (always "FF-8")
                Steps = br.ReadUInt32(); //[4]Steps (used to generate random encounters)

                Payslip = br.ReadUInt32(); //[8]Payslip
                byte1215 = br.ReadBytes(4); //[12-15]unused in fields
                SeedRankPts = br.ReadInt16(); //[16]SeeD rank points? // i guess seed rank can fall.
                byte1819 = br.ReadBytes(2); //[18-19]unused in fields
                BattlesWon = br.ReadUInt32(); //[20]Battles won. (Fun fact: this affects the basketball shot in Trabia.)
                byte2425 = br.ReadBytes(2); //[24-25]unused in fields
                EscapedBattles = br.ReadUInt16(); //[26]Battles escaped.
                EnemiesKilled = new Dictionary<Characters, ushort>((int)Characters.Edea_Kramer + 1);
                for (int i = 0; i < (int)Characters.Edea_Kramer + 1; i++)
                    EnemiesKilled.Add((Characters)i, br.ReadUInt16());
                //ushort28 = br.ReadUInt16(); //[28]Enemies killed by Squall
                //ushort30 = br.ReadUInt16(); //[30]Enemies killed by Zell
                //ushort32 = br.ReadUInt16(); //[32]Enemies killed by Irvine
                //ushort34 = br.ReadUInt16(); //[34]Enemies killed by Quistis
                //ushort36 = br.ReadUInt16(); //[36]Enemies killed by Rinoa
                //ushort38 = br.ReadUInt16(); //[38]Enemies killed by Selphie
                //ushort40 = br.ReadUInt16(); //[40]Enemies killed by Seifer
                //ushort42 = br.ReadUInt16(); //[42]Enemies killed by Edea

                DeathCounter = new Dictionary<Characters, ushort>((int)Characters.Edea_Kramer + 1);
                for (int i = 0; i < (int)Characters.Edea_Kramer + 1; i++)
                    DeathCounter.Add((Characters)i, br.ReadUInt16());

                //ushort44 = br.ReadUInt16(); //[44]Squall death count
                //ushort46 = br.ReadUInt16(); //[46]Zell death count
                //ushort48 = br.ReadUInt16(); //[48]Irvine death count
                //ushort50 = br.ReadUInt16(); //[50]Quistis death count
                //ushort52 = br.ReadUInt16(); //[52]Rinoa death count
                //ushort54 = br.ReadUInt16(); //[54]Selphie death count
                //ushort56 = br.ReadUInt16(); //[56]Seifer death count
                //ushort58 = br.ReadUInt16(); //[58]Edea death count

                byte6067 = br.ReadBytes(8); //[60-67]unused in fields
                EnemiesKilledTotal = br.ReadUInt32(); //[68]Enemies killed
                Gill = br.ReadUInt32(); //[72]Amount of Gil the party currently has
                GillLaguna = br.ReadUInt32(); //[76]Amount of Gil Laguna's party has
                FMVFrames = br.ReadUInt32(); //[80]Counts the number of frames since the current movie started playing. (default fps is 15?)
                LastArea = br.ReadUInt16(); //[84]Last area visited.
                CurrentCarRent = br.ReadSByte(); //[86]Current car rent.
                sbyte87 = br.ReadSByte(); //[87]Built-in engine variable. No idea what it does. Scripts always check if it's equal to 0 or 10. Related to music.
                sbyte88 = br.ReadSByte(); //[88]Built-in engine variable. Used exclusively on save points. Never written to with field scripts. Related to Siren's Move-Find ability.
                byte89103 = br.ReadBytes(15); //[89-103]unused in fields
                ulong104 = br.ReadUInt32(); //[104]Seems related to SARALYDISPON/SARALYON/MUSICLOAD/PHSPOWER opcodes
                music = br.ReadUInt32(); //[108]Music related
                ulong112 = br.ReadUInt32(); //[112]unused in fields
                DrawPtsFeild = br.ReadBytes(32); //[116-147]Draw points in field
                DrawPtsWorld = br.ReadBytes(31); //[148-179]Draw points in worldmap
                byte180255 = br.ReadBytes(76); //[180-255]unused in fields
                StoryQuestprogress = br.ReadUInt16(); //[256]Main Story quest progress.
                byte258 = br.ReadByte(); //[258]not investigated
                byte259260 = br.ReadBytes(2); //[259-260]unused in fields
                byte261 = br.ReadByte(); //[261]not investigated
                byte262263 = br.ReadBytes(2); //[262-263]unused in fields
                byte264 = br.ReadByte(); //[264]not investigated
                byte265 = br.ReadByte(); //[265]not investigated
                WorldMapVersion = br.ReadByte(); //[266]World map version? (3=Esthar locations unlocked)
                byte267 = br.ReadByte(); //[267]unused in fields
                byte268 = br.ReadByte(); //[268]not investigated
                byte269 = br.ReadByte(); //[269]not investigated
                byte270 = br.ReadByte(); //[270]not investigated
                byte271 = br.ReadByte(); //[271]unused in fields
                byte272299 = br.ReadBytes(28); //[272-299]SO MANY F***ING CARD GAME VARIABLES
                CardQueenrecards = br.ReadByte(); //[300]Card Queen re-cards.
                byte301303 = br.ReadBytes(3); //[301-303]unused in fields
                TimberManiacsIssues = br.ReadBytes(2); //[304-305]Timber Maniacs issues found.
                Hacktuar = br.ReadBytes(14); //[306-319]Reserved for Hacktuar / FF8Voice
                UltimeciaGallery = br.ReadBytes(3); //[320-332]Ultimecia Gallery related (pictures viewed?)
                UltimeciaArmory = br.ReadByte(); //[333]Ultimecia Armory chest flags
                UltimeciaCastle = br.ReadByte(); //[334]Ultimecia Castle seals. See SEALEDOFF for details.
                Card = br.ReadByte(); //[335]Card related
                BusRelated = br.ReadByte(); //[336]Deling City bus related
                GatesOpened = br.ReadBytes(3); //[338-340]Deling Sewer gates opened
                byte341 = br.ReadByte(); //[341]Does lots of things.5
                BusSystem = br.ReadByte(); //[342]Deling City bus system
                byte343 = br.ReadByte(); //[343]G-Garden door/event flags.
                byte344 = br.ReadByte(); //[344]B-Garden / G-Garden event flags (during GvG)
                byte345 = br.ReadByte(); //[345]G-Garden door/event flags.
                byte346349 = br.ReadBytes(4); //[346-349]FH Instrument (346 Zell, 347 Irvine, 348 Selphie, 349 Quistis)
                ushort350356 = new ushort[7]; //[350-356]Health Bars (Garden mech fight)
                for (int i = 0; i < ushort350356.Length; i++)
                    ushort350356[i] = br.ReadUInt16();
                byte358 = br.ReadByte(); //[358]Space station talk flags, Centra ruins related (beat odin?).
                byte359 = br.ReadByte(); //[359]Centra ruins related (beat odin?).
                ulong360 = br.ReadUInt32(); //[360]Choice of FH music.
                byte364368 = br.ReadBytes(5); //[364-368]Randomly generated code for Centra Ruins.
                byte369370 = br.ReadBytes(2); //[369-370]Ultimecia Castle flags
                byte371 = br.ReadByte(); //[371]unused in fields
                byte372376 = br.ReadBytes(5); //[372-376]Ultimecia boss/timer/item flags
                byte377 = br.ReadByte(); //[377]Ultimecia organ note controller
                byte378 = br.ReadByte(); //[378]Centra Ruins timer (controls blackout messages from Odin)
                byte379 = br.ReadByte(); //[379]unused in fields
                ushort380 = br.ReadUInt16(); //[380]Squall health during mech fight.
                byte382383 = br.ReadBytes(2); //[382-383]unused in fields
                byte384 = br.ReadByte(); //[384]Something about Laguna's time periods and GFs.
                byte385 = br.ReadByte(); //[385]Laguna dialogue in pub. Only the +2 bit is ever set. Don't change the +1 bit.
                byte387 = br.ReadByte(); //[387]Winhill progress?
                byte388 = br.ReadByte(); //[388]Timber Maniacs HQ talk flags (main lobby)
                byte389 = br.ReadByte(); //[389]Timber Maniacs HQ talk flags (office room)
                byte390 = br.ReadByte(); //[390]Edea talk flags at her house
                byte391 = br.ReadByte(); //[391]Laguna talk flags (in his office, disc 3)
                byte392 = br.ReadByte(); //[392]unknown (used in Edea's house and in the Balamb Garden computer system)
                byte393399 = br.ReadBytes(7); //[393-399]unused in fields
                ulong400and404 = new ulong[2]; //[400 and 404]Related to monsters killed in Winhill, but I don't think it actually does anything. Will investigate.
                for (int i = 0; i < ulong400and404.Length; i++)
                    ulong400and404[i] = br.ReadUInt32();
                byte408 = br.ReadByte(); //[408]unused in fields
                byte409 = br.ReadByte(); //[409]Balamb Garden computer system
                byte410431 = br.ReadBytes(22); //[410-431]unused in fields
                byte432 = br.ReadByte(); //[432]BG Main hall flags
                byte433 = br.ReadByte(); //[433]Flags. Switches are assigned all over BG. No idea what any of them control.
                byte434 = br.ReadByte(); //[434]Flags. Switches are assigned all over BG. No idea what any of them control.
                byte435 = br.ReadByte(); //[435]Flags. Switches are assigned all over BG. No idea what any of them control.
                byte436 = br.ReadByte(); //[436]Moomba friendship level in the prison? Some actions cause these flags to be set.
                byte437 = br.ReadByte(); //[437]In BG on Disc 2, keeps track of who's in your party. In the prison, it's the current floor you're on.
                byte438 = br.ReadByte(); //[438]Cid vs Norg event flags
                byte439 = br.ReadByte(); //[439]Cid vs Norg event flags
                byte440 = br.ReadByte(); //[440]Event flags. (+1 Quad ambush, +2 quad item giver, +4/+8 Infirmary helped, +16 Nida, +64 Kadowaki Elixir, +128 Training center)
                byte441 = br.ReadByte(); //[441]Cid vs Norg event flags
                byte442 = br.ReadByte(); //[442]Rinoa Garden tour flags
                ushort443 = br.ReadUInt16(); //[443]Zell Health in Prison (Hacktuar)
                byte445447 = br.ReadBytes(3); //[445-447]Propagator defeated flags
                ushort448 = br.ReadUInt16(); //[448]Unknown
                byte450451 = br.ReadBytes(2); //[450-451]Various magazine/talk flags
                byte452 = br.ReadByte(); //[452]Lunatic Pandora areas visited?
                byte453455 = br.ReadBytes(3); //[453-455]Moomba teleport variables
                byte456457 = br.ReadBytes(2); //[456-457]unused in fields
                byte458459 = br.ReadBytes(2); //[458-459]Used with MUSICSKIP in some Balamb Garden areas
                byte460 = br.ReadByte(); //[460]Random flags (some used for Card Club)
                byte461473 = br.ReadBytes(13); //[461-473]unused in fields
                byte474 = br.ReadByte(); //[474]Random flags (some used for Card Club)
                byte475478 = br.ReadBytes(4); //[475-478]CC Group variables
                byte479 = br.ReadByte(); //[479]If set to 0, disables all random battles during area loading.
                byte480 = br.ReadByte(); //[480]State of students in classroom (what they're doing).
                byte481 = br.ReadByte(); //[481]Controls a conversation in the cafeteria.
                short482 = br.ReadInt16(); //[482]Error ratio of missiles
                byte484 = br.ReadByte(); //[484]Missile Base progression?
                byte485 = br.ReadByte(); //[485]ToUK Progression (initially 0b111010101, +2 on finish quest. No other pops)
                byte486 = br.ReadByte(); //[486]ToUK room? (used to control map jumps in the maze)
                byte487 = br.ReadByte(); //[487]Missile base progression (also does something in BG2F classroom)
                byte488 = br.ReadByte(); //[488]Alternate Party Flags. Irvine +1/+16, Quistis +2/+32, Rinoa +4/+64, Zell +8/+128.1
                byte489 = br.ReadByte(); //[489]Random talk flags?
                byte490 = br.ReadByte(); //[490]Cafeteria cutscene
                byte491 = br.ReadByte(); //[491]ToUK stuff
                byte492 = br.ReadByte(); //[492]I think this is a door opener for the missile base if you choose a time limit.
                byte493 = br.ReadByte(); //[493]Missile base timer related?
                byte494527 = br.ReadBytes(34); //[494-527]unused in fields
                short528 = br.ReadInt16(); //[528]Sub-story progression (it's a progression variable for individual segments of the game)
                byte530 = br.ReadByte(); //[530]X-ATM related (defeated it in battle?)
                byte531 = br.ReadByte(); //[531]Functionally unused. Read from at dollet, only manipulated in debug rooms.
                byte532 = br.ReadByte(); //[532]Controls footstep sounds at dollet (sand to concrete)
                byte533 = br.ReadByte(); //[533]not investigated
                byte534 = br.ReadByte(); //[534]not investigated
                byte535 = br.ReadByte(); //[535]not investigated
                byte536 = br.ReadByte(); //[536]not investigated
                byte537 = br.ReadByte(); //[537]not investigated
                byte538 = br.ReadByte(); //[538]not investigated
                byte539 = br.ReadByte(); //[539]not investigated
                byte540591 = br.ReadBytes(52); //[540-591]unused in fields
                byte592593 = br.ReadBytes(2); //[592-593]Seems to control angles and character facing.
                byte594 = br.ReadByte(); //[594]unused in fields
                byte595 = br.ReadByte(); //[595]not investigated
                byte596 = br.ReadByte(); //[596]not investigated
                byte597 = br.ReadByte(); //[597]not investigated
                byte598 = br.ReadByte(); //[598]not investigated
                byte599 = br.ReadByte(); //[599]not investigated
                byte600 = br.ReadByte(); //[600]not investigated
                byte601 = br.ReadByte(); //[601]not investigated
                byte602 = br.ReadByte(); //[602]not investigated
                byte603 = br.ReadByte(); //[603]not investigated
                byte604 = br.ReadByte(); //[604]not investigated
                byte605 = br.ReadByte(); //[605]not investigated
                byte606 = br.ReadByte(); //[606]not investigated
                byte607 = br.ReadByte(); //[607]not investigated
                byte608 = br.ReadByte(); //[608]not investigated
                byte609 = br.ReadByte(); //[609]not investigated
                byte610 = br.ReadByte(); //[610]not investigated
                byte611 = br.ReadByte(); //[611]not investigated
                byte612 = br.ReadByte(); //[612]not investigated
                byte613 = br.ReadByte(); //[613]not investigated
                byte614 = br.ReadByte(); //[614]not investigated
                byte615 = br.ReadByte(); //[615]not investigated
                byte616 = br.ReadByte(); //[616]not investigated
                byte617 = br.ReadByte(); //[617]not investigated
                byte618 = br.ReadByte(); //[618]not investigated
                byte619 = br.ReadByte(); //[619]not investigated
                byte620 = br.ReadByte(); //[620]not investigated
                byte621 = br.ReadByte(); //[621]not investigated
                byte622 = br.ReadByte(); //[622]not investigated
                byte623 = br.ReadByte(); //[623]not investigated
                byte624 = br.ReadByte(); //[624]not investigated
                byte625 = br.ReadByte(); //[625]Balamb visited flags (+8 Zell's room)
                byte626 = br.ReadByte(); //[626]not investigated
                byte627 = br.ReadByte(); //[627]not investigated
                byte628 = br.ReadByte(); //[628]unused in fields
                byte629 = br.ReadByte(); //[629]not investigated
                byte630 = br.ReadByte(); //[630]not investigated
                byte631 = br.ReadByte(); //[631]not investigated
                byte632 = br.ReadByte(); //[632]not investigated
                byte633 = br.ReadByte(); //[633]not investigated
                ushort634 = br.ReadUInt16(); //[634]not investigated
                byte636 = br.ReadByte(); //[636]not investigated
                byte637 = br.ReadByte(); //[637]unused in fields
                byte638 = br.ReadByte(); //[638]not investigated
                byte639 = br.ReadByte(); //[639]unused in fields
                byte640 = br.ReadByte(); //[640]not investigated
                byte641 = br.ReadByte(); //[641]not investigated
                byte642 = br.ReadByte(); //[642]not investigated
                byte643 = br.ReadByte(); //[643]not investigated
                byte644 = br.ReadByte(); //[644]not investigated
                byte645 = br.ReadByte(); //[645]not investigated
                byte646 = br.ReadByte(); //[646]not investigated
                byte647 = br.ReadByte(); //[647]not investigated
                byte648 = br.ReadByte(); //[648]not investigated
                byte649 = br.ReadByte(); //[649]not investigated
                byte650655 = br.ReadBytes(6); //[650-655]unused in fields
                ushort656 = br.ReadUInt16(); //[656]not investigated
                byte658 = br.ReadByte(); //[658]not investigated
                byte659 = br.ReadByte(); //[659]not investigated
                byte660 = br.ReadByte(); //[660]not investigated
                byte661 = br.ReadByte(); //[661]not investigated
                byte662 = br.ReadByte(); //[662]not investigated
                byte663 = br.ReadByte(); //[663]not investigated
                byte664 = br.ReadByte(); //[664]not investigated
                byte665 = br.ReadByte(); //[665]not investigated
                ushort666 = br.ReadUInt16(); //[666]not investigated
                byte668 = br.ReadByte(); //[668]not investigated
                byte669671 = br.ReadBytes(3); //[669-671]unused in fields
                ushort672 = br.ReadUInt16(); //[672]not investigated
                byte674 = br.ReadByte(); //[674]unused in fields
                byte675 = br.ReadByte(); //[675]not investigated
                byte676 = br.ReadByte(); //[676]unused in fields
                byte677 = br.ReadByte(); //[677]not investigated
                byte678 = br.ReadByte(); //[678]not investigated
                byte679 = br.ReadByte(); //[679]unused in fields
                byte680 = br.ReadByte(); //[680]not investigated
                byte681 = br.ReadByte(); //[681]not investigated
                byte682 = br.ReadByte(); //[682]not investigated
                byte683 = br.ReadByte(); //[683]not investigated
                byte684 = br.ReadByte(); //[684]not investigated
                byte685 = br.ReadByte(); //[685]not investigated
                byte686 = br.ReadByte(); //[686]not investigated
                byte687 = br.ReadByte(); //[687]not investigated
                byte688 = br.ReadByte(); //[688]not investigated
                byte689 = br.ReadByte(); //[689]not investigated
                byte690 = br.ReadByte(); //[690]not investigated
                byte691 = br.ReadByte(); //[691]not investigated
                byte692719 = br.ReadBytes(28); //[692-719]unused in fields
                Costumes = new Dictionary<Characters, Costume> {
                    { Characters.Squall_Leonhart, (Costume) br.ReadByte() },
                    { Characters.Zell_Dincht, (Costume) br.ReadByte() },
                    { Characters.Selphie_Tilmitt, (Costume) br.ReadByte() },
                    { Characters.Quistis_Trepe, (Costume) br.ReadByte() },
                };
                //byte720 = br.ReadByte(); //[720]Squall's costume (0=normal, 1=student, 2=SeeD, 3=Bandage on forehead)
                //byte721 = br.ReadByte(); //[721]Zell's Costume (0=normal, 1=student, 2=SeeD)
                //byte722 = br.ReadByte(); //[722]Selphie's costume (0=normal, 1=student, 2=SeeD)
                //byte723 = br.ReadByte(); //[723]Quistis' Costume (0=normal, 1=SeeD)
                ushort724 = br.ReadUInt16(); //[724]Dollet mission time
                ushort726 = br.ReadUInt16(); //[726]not investigated
                byte728 = br.ReadByte(); //[728]Does lots of things.3
                byte729 = br.ReadByte(); //[729]not investigated
                byte730 = br.ReadByte(); //[730]Flags (+1 Joined Garden Festival Committee, +4 Gave Selphie tour of BG, +16 Kadowaki asks for Cid, +32 and +64 Tomb of Unknown Kind hints?, +128 Beat all card people?)
                byte731 = br.ReadByte(); //[731]unused in fields
                ushort732 = br.ReadUInt16(); //[732]not investigated
                byte734 = br.ReadByte(); //[734]Split Party Flags (+1 Zell, +2 Irvine, +4 Rinoa, +8 Quistis, +16 Selphie).2
                byte735 = br.ReadByte(); //[735]not investigated
                byte736751 = br.ReadBytes(16); //[736-751]unused in fields
                byte752 = br.ReadByte(); //[752]not investigated
                byte7531023 = br.ReadBytes(271); //[753-1023]unused in fields
                byteAbove1023 = br.ReadBytes(256); //[Above 1023]Temporary variables used pretty much everywhere.
            }
        }
    }
}