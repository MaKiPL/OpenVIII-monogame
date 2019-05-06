namespace FF8
{






    internal static partial class Saves
    {
        internal struct FF8Variables
        {
            
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
    }
}