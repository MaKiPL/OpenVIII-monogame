using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#pragma warning disable CS0649
namespace FF8
{
    internal class Memory
    {
        //monogame
        public static GraphicsDeviceManager graphics;
        public static SpriteBatch spriteBatch;
        public static ContentManager content;


        public static bool IsActive = true;
        public static Font font;
        public static Texture2D[] iconsTex;

        public enum ScaleMode
        {
            Vertical,Horizontal,Stretch
        }
        public const ScaleMode _scaleMode = ScaleMode.Vertical;
        public static Vector2 Scale
        {
            get
            {
                float h = (float)Memory.graphics.GraphicsDevice.Viewport.Width / Memory.PreferredViewportWidth;
                float v = (float)Memory.graphics.GraphicsDevice.Viewport.Height / Memory.PreferredViewportHeight;
                switch (_scaleMode)
                {
#pragma warning disable CS0162 // Unreachable code detected
                    case ScaleMode.Horizontal:
                        return new Vector2(h, h);
                    case ScaleMode.Vertical:
                        return new Vector2(v, v);
                    case ScaleMode.Stretch:
                    default:
                        return new Vector2(h, v);
#pragma warning restore CS0162 // Unreachable code detected
                }
            }
        }


        //original resolution I am working on, therefore if user scales it we need to propertially scale everything
        public static int PreferredViewportWidth = 1280;
        public static int PreferredViewportHeight = 720;

        public static GameTime gameTime;

        private static ushort prevmusic = 0;
        private static ushort currmusic = 0;
        internal static ushort MusicIndex
        {
            get
            {
                while ((prevmusic > currmusic || prevmusic == ushort.MinValue && currmusic == ushort.MaxValue) &&
                    !dicMusic.ContainsKey(currmusic))
                {

                    if (dicMusic.Keys.Max() < currmusic)
                    {
                        currmusic = dicMusic.Keys.Max();
                    }
                    else
                    {
                        currmusic--;
                    }
                }
                while (prevmusic < currmusic && !dicMusic.ContainsKey(currmusic))
                {

                    if (dicMusic.Keys.Max() < currmusic)
                    {
                        currmusic = dicMusic.Keys.Min();
                    }
                    else
                    {
                        currmusic++;
                    }
                }

                return currmusic;
            }
            set
            {
                prevmusic = currmusic;
                currmusic = value;
            }
        }
        public static string[] musices;
        public static readonly Dictionary<ushort, List<string>> dicMusic = new Dictionary<ushort, List<string>>(); //ogg and sgt files have same 3 digit prefix.
        public static void SpriteBatchStartStencil(SamplerState ss = null) => spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque,ss, graphics.GraphicsDevice.DepthStencilState);

        public static void SpriteBatchStartAlpha(SamplerState ss = null) => spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,ss);

        public static void SpriteBatchEnd() => spriteBatch.End();

        public static readonly BlendState blendState_BasicAdd = new BlendState()
        {
            ColorSourceBlend = Blend.SourceColor,
            ColorDestinationBlend = Blend.DestinationColor,
            ColorBlendFunction = BlendFunction.Add,

            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.DestinationAlpha,
            AlphaBlendFunction = BlendFunction.Add
        };

        public static readonly BlendState blendState_forceDraw = new BlendState()
        {
            ColorSourceBlend = Blend.SourceColor,
            ColorDestinationBlend = Blend.SourceColor,
            ColorBlendFunction = BlendFunction.Add,

            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.DestinationAlpha,
            AlphaBlendFunction = BlendFunction.Add,
        };




        public static int module = MODULE_OVERTURE_DEBUG;
        
        public static string FF8DIR => GameLocation.Current.DataPath;

        /// <summary>
        /// If true by the end of Update() will skip the next Draw()
        /// </summary>
        public static bool SuppressDraw { get; internal set; }
        public static bool IsMouseVisible { get; internal set; } = false;
        #region modules
        public const int MODULE_BATTLE = 3;
        public const int MODULE_FIELD = 5;
        public const int MODULE_FIELD_DEBUG = -5;
        public const int MODULE_BATTLE_DEBUG = -3;
        public const int MODULE_MOVIETEST = -9;
        public const int MODULE_OVERTURE_DEBUG = -12;
        public const int MODULE_MAINMENU_DEBUG = -13;
        public const int MODULE_WORLD_DEBUG = -17;
        public const int MODULE_FACE_TEST = -20;
        public const int MODULE_ICON_TEST = -21;
        #endregion

        #region battleProvider
        public static int battle_encounter = 000;
        public static int SetBattleMusic = 6;
        public static Init_debugger_battle.Encounter[] encounters;

        public struct VIII_cameraMemoryStruct
        {
            public byte camAnimId; //.data:01D977A8 beginning of struct
            public byte UNKNOWNpadding; //?
            public ushort mainController; //so far unknown, probably a controller for animation? .data:01D977AA
            public ushort secondWordController;
            public ushort thirdWordController;
            public byte cameraVar1; //this is later set up after controllers bit-parsing. May be actually camera
            public byte cameraVar2;
            public byte cameraVar3;
            public byte cameraVar4;
            public byte cameraVar5;
            public byte cameraVar6;
            public ushort unknownWord; //.data:01D977B6 unknown, padding? is this struct packed?
            public uint animationPointer; //.data:01D977B8 always used with animation data + this struct
        }

        public static VIII_cameraMemoryStruct BS_CameraStruct;
        #endregion

        #region MusicDataMidi
        public static Dictionary<ushort, string> Songssgt = new Dictionary<ushort, string>()
        {
            {0, "Lose" },
            {1, "Win" },
            {2, "Open" },
            {3, "Combat" },
            {4, "Run" },
            {5, "Battle" },
            {6, "Funsui" },
            {7, "End" },
            {8, "Antenna" },
            {9, "Waiting" },
            {10, "Ante " },
            {11, "Wind" },
            {12, "Crab" },
            {13, "Battle2" },
            {14, "Friend2" },
            {15, "Fuan2" },
            {16, "March2" },
            {17, "Land" },
            {18, "Julia" },
            {19, "Waltz" },
            {20, "Friend " },
            {21, "Dungeon" },
            {22, "Pianosolo" },
            {23, "Parade" },
            {24, "March1" },
            {25, "Secret" },
            {26, "Garden" },
            {27, "Fuan " },
            {28, "Polka2" },
            {29, "Anthem" },
            {30, "FlangChorus" },
            {31, "DubChorus" },
            {32, "SoloChorus" },
            {33, "FemaleChorus" },
            {34, "Chorus" },
            {35, "M7F5" },
            {36, "Sorceress" },
            {37, "Reet" },
            {38, "Soyo" },
            {39, "Rouka" },
            {40, "Night" },
            {41, "Field" },
            {42, "Guitar" },
            {43, "Concert" },
            {44, "Sea" },
            {45, "Silent" },
            {46, "Resistance" },
            {47, "Kaiso" },
            {48, "Horizon" },
            {49, "Master" },
            {50, "Battle2" },
            {51, "Rinoa" },
            {52, "Trabia" },
            {53, "Horizon2" },
            {54, "Truth" },
            {55, "Prison" },
            {56, "GalbadiaGarden" },
            {57, "Timber" },
            {58, "Galbadia " },
            {59, "Pinchi" },
            {60, "Scene1" },
            {61, "Pub" },
            {62, "Bat3" },
            {63, "Stage" },
            {64, "Choco" },
            {65, "White" },
            {66, "Majomv" },
            {67, "Musho" },
            {68, "Missile" },
            {69, "Speech" },
            {70, "Card" },
            {71, "Gomon" },
            {72, "Soto" },
            {73, "Majobat" },
            {74, "Train" },
            {75, "Garden2" },
            {76, "Bossbat2" },
            {77, "LastDungeon" },
            {78, "Gafly" },
            {79, "Demo" },
            {80, "Spy" },
            {81, "VoiceDeChocobo" },
            {82, "Salt" },
            {83, "Alien" },
            {84, "Sekichu" },
            {85, "Esta" },
            {86, "Moonmv" },
            {87, "Mdmotor" },
            {88, "Moonmv2" },
            {89, "Fly" },
            {90, "BossBat1" },
            {91, "Rag1" },
            {92, "Rag2" },
            {93, "LastBoss" },
            {94, "Lastwhite" },
            {95, "Lasbl" },
            {96, "Keisho" },
            {97, "Compression" },
        };
        #endregion
        #region MusicDataOGG
        public static Dictionary<ushort, string> Songsogg = new Dictionary<ushort, string>()
        {
            {0,"Lose" },
{1,"The Winner" },
{4,"Never Look Back" },
{5,"Don't Be Afraid" },
{7,"Dead End" },
{8,"Starting Up" },
{9,"Intruders" },
{12,"Don't Be Afraid (X-ATM092)" },
{13,"Force Your Way" },
{14,"FITHOS LUSEC WECOS VINOSEC (No Intro)" },
{15,"Unrest" },
{16,"The Stage is Set" },
{17,"The Landing" },
{18,"Love Grows" },
{19,"Waltz for the Moon" },
{20,"Ami" },
{21,"Find Your Way" },
{22,"Julia" },
{23,"FITHOS LUSEC WECOS VINOSEC" },
{24,"SeeD" },
{25,"Tell Me" },
{26,"Balamb GARDEN" },
{27,"Fear" },
{28,"Dance with the Balamb-Fish" },
{29,"Cactus Jack" },
{35,"The Mission" },
{36,"SUCCESSION OF WITCHES" },
{41,"Blue Fields" },
{42,"Breezy" },
{43,"Concert" },
{46,"Timber Owls" },
{47,"Fragments of Memories" },
{48,"Fisherman's Horizon" },
{49,"Heresy" },
{51,"My Mind" },
{52,"Where I Belong" },
{53,"Starting Up (Looped)" },
{54,"Truth" },
{55,"Trust Me" },
{56,"Galbadia GARDEN" },
{57,"Martial Law" },
{58,"Under Her Control" },
{59,"Only a Plank Between One and Perdition" },
{60,"Junction" },
{61,"Roses and Wine" },
{62,"The Man with the Machine Gun" },
{63,"A Sacrifice" },
{64,"ODEKA ke Chocobo" },
{65,"Drifting" },
{66,"Wounded" },
{67,"Jailed" },
{68,"Retaliation" },
{69,"The Oath" },
{70,"Shuffle or Boogie" },
{71,"Rivals" },
{72,"Blue Sky" },
{73,"Premonition" },
{75,"Galbadia GARDEN (No Intro)" },
{76,"Maybe I'm a Lion" },
{77,"The Castle" },
{78,"Movin'" },
{79,"Overture" },
{80,"The Spy" },
{81,"Mods de Chocobo" },
{82,"The Salt Flats" },
{83,"The Residents" },
{84,"Lunatic Pandora" },
{85,"Silence and Motion" },
{86,"Tears of the Moon" },
{88,"Tears of the Moon (Alternate)" },
{89,"Ride On" },
{90,"The Legendary Beast" },
{91,"Slide Show Part 1" },
{92,"Slide Show Part 2" },
{93,"The Extreme" },
{96,"The Successor" },
{97,"Compression of Time" },
{99,"The Landing (No Intro)" },
{512,"The Loser" },
{513,"Eyes on Me" },
{514,"Irish Jig (Concert)" },
{515,"Eyes on Me (Concert)" },
{516,"Movin' (No Intro)" },
{517,"The Landing (Alternate)" },
{518,"The Landing (Alternate - No Intro)" },
{519,"Galbadia GARDEN (Alternate)" },

        };
        #endregion

        #region  DrawPointMagic
        public static Dictionary<byte, string> DrawPointMagic = new Dictionary<byte, string>()
        {
            {0, "Cure - Balamb Garden courtyard"},
            {1, "Blizzard - Balamb Garden training center"},
            {2, "Full-Life - Balamb Garden MD level"},
            {3, "Esuna - Balamb Garden library next to the book shelf"},
            {4, "Demi - Balamb Garden cafeteria (only during Garden Riot)"},
            {5, "Bio - Balamb Garden B2 floor"},
            {6, "Thunder - Balamb outside junk shop"},
            {7, "Cure - Balamb harbor"},
            {8, "Fire - Fire Cavern"},
            {9, "Silence - Dollet town square"},
            {10, "Blind - Dollet Communications Tower"},
            {11, "Scan - Timber Pub Aurora back alley"},
            {12, "Cure - Timber outside the pub"},
            {13, "Blizzaga - Timber Maniacs Building left room"},
            {14, "Haste - Galbadia Garden lobby"},
            {15, "Life - Galbadia Garden changing rooms"},
            {16, "Shell - Galbadia Garden courtyard"},
            {17, "Protect - Galbadia Garden ice rink"},
            {18, "Double - Galbadia Garden auditorium"},
            {19, "Aura - Outside Galbadia Garden during Garden war"},
            {20, "Cure - Timber forests in a Laguna dream"},
            {21, "Water - Timber forests in a Laguna dream"},
            {22, "Thundara - Deling City park"},
            {23, "Zombie - Deling City Sewers"},
            {24, "Esuna - Deling City Sewers"},
            {25, "Bio - Deling City Sewers"},
            {26, "Fira"},
            {27, "Berserk - D-District Prison Floor 9 - right cell"},
            {28, "Thundaga - D-District Prison Floor 11 - right cell"},
            {29, "Aero - Outside D-District Prison"},
            {30, "Blizzara - Missile Base - control room"},
            {31, "Blind - Missile Base room with G-Soldiers who ask to deliver a message"},
            {32, "Full-Life - Missile Base - silo room"},
            {33, "Drain - Winhill road south from town square"},
            {34, "Dispel - Winhill town square"},
            {35, "Curaga - Winhill Laguna's room in the dream"},
            {36, "Reflect - Winhill east road"},
            {37, "Protect - Tomb of the Unknown King - outside"},
            {38, "Float - Tomb of the Unknown King - north room"},
            {39, "Cura - Tomb of the Unknown King - east room"},
            {40, "Haste - Fishermans Horizon abandoned train station"},
            {41, "Shell - Fishermans Horizon junk shop"},
            {42, "Regen - Fishermans Horizon overlooking the sun panel"},
            {43, "Full-Life - Fishermans Horizon Master Fisherman's fishing spot"},
            {44, "Ultima - Fishermans Horizon mayor's house"},
            {45, "Thundaga - Great Salt Lake past the dinosaur skeleton"},
            {46, "Meteor - Great Salt Lake dinosaur skeleton"},
            {47, "Curaga - Esthar city streets near city entrance"},
            {48, "Blizzard - Esthar outside palace"},
            {49, "Quake - Esthar outside Odine's Lab"},
            {50, "Tornado - Esthar shopping mall"},
            {51, "Double - Esthar Odine's Lab in a Laguna dream"},
            {52, "Pain"},
            {53, "Flare - Esthar Odine's Lab in a Laguna dream"},
            {54, "Stop - Sorceress Memorial"},
            {55, "Stop"},
            {56, "Life - Tears' Point entrance"},
            {57, "Reflect - Tears' Point middle"},
            {58, "Death - Lunatic Pandora Laboratory in a Laguna dream"},
            {59, "Holy - Lunatic Pandora near Elevator #1"},
            {60, "Silence - Lunatic Pandora"},
            {61, "Ultima - Lunatic Pandora"},
            {62, "Confuse"},
            {63, "Break - Lunatic Pandora on the way to fight Adel"},
            {64, "Meteor - Lunatic Pandora entrance"},
            {65, "Curaga - Lunatic Pandora elevator room"},
            {66, "Slow"},
            {67, "Curaga - Edea's Orphanage"},
            {68, "Flare"},
            {69, "Holy"},
            {70, "Sleep - Centra Excavation Site"},
            {71, "Confuse - Centra Excavation Site"},
            {72, "Aero - Centra Ruins right ladder after the lift"},
            {73, "Drain - Centra Ruins platform after the first staircase"},
            {74, "Pain - Centra Ruins next to the dome"},
            {75, "Thundaga - Trabia Garden in front of the statue"},
            {76, "Zombie - Trabia Garden cemetery"},
            {77, "Aura - Trabia Garden stage"},
            {78, "Ultima - Shumi Village - above ground"},
            {79, "Blizzaga - Shumi Village - outside elder's house"},
            {80, "Firaga - Shumi Village workshop"},
            {81, "Tornado"},
            {82, "Holy - White SeeD Ship"},
            {83, "Cura - Ragnarok room with a red Propagator"},
            {84, "Life - Ragnarok hangar upstairs"},
            {85, "Full-Life - Ragnarok room with save point"},
            {86, "Dispel - Deep Sea Research Center second level"},
            {87, "Esuna - Deep Sea Research Center secret room"},
            {88, "Triple - Deep Sea Research Center third screen on the way to Ultima Weapon's lair"},
            {89, "Ultima - Deep Sea Research Center fifth screen on the way to Ultima Weapon's lair"},
            {90, "Meltdown - Lunar Base room before the escape pods"},
            {91, "Meteor - Lunar Base Ellone's room"},
            {92, "Haste"},
            {93, "Slow"},
            {94, "Curaga"},
            {95, "Life"},
            {96, "Stop"},
            {97, "Regen"},
            {98, "Double"},
            {99, "Triple"},
            {100, "Flare - Ultimecia Castle outside"},
            {101, "Curaga - Ultimecia Castle storage room"},
            {102, "Cura - Ultimecia Castle passageway"},
            {103, "Scan"},
            {104, "Esuna"},
            {105, "Slow - Ultimecia Castle courtyard"},
            {106, "Dispel - Ultimecia Castle chapel"},
            {107, "Stop - Ultimecia Castle clock tower"},
            {108, "Life"},
            {109, "Flare"},
            {110, "Aura - Ultimecia Castle wine cellar"},
            {111, "Holy - Ultimecia Castle treasure room"},
            {112, "Meteor"},
            {113, "Meltdown - Ultimecia Castle art gallery"},
            {114, "Ultima - Ultimecia Castle armory"},
            {115, "Full-Life - Ultimecia Castle prison"},
            {116, "Triple"},
            {117, "Fire"},
            {118, "Fire"},
            {119, "Fire"},
            {120, "Fire"},
            {121, "Fire"},
            {122, "Fire"},
            {123, "Fire"},
            {124, "Fire"},
            {125, "Fire"},
            {126, "Fire"},
            {127, "Fire"},
            {128, "Cure"},
            {129, "Esuna"},
            {130, "Thunder"},
            {131, "Fira"},
            {132, "Thundara"},
            {133, "Blizzara"},
            {134, "Blizzard"},
            {135, "Fire"},
            {136, "Cure"},
            {137, "Water"},
            {138, "Cura"},
            {139, "Esuna"},
            {140, "Scan"},
            {141, "Shell"},
            {142, "Haste"},
            {143, "Aero"},
            {144, "Bio"},
            {145, "Life"},
            {146, "Demi"},
            {147, "Protect"},
            {148, "Holy"},
            {149, "Thundaga"},
            {150, "Stop"},
            {151, "Firaga"},
            {152, "Regen"},
            {153, "Blizzaga"},
            {154, "Confuse"},
            {155, "Flare"},
            {156, "Dispel"},
            {157, "Slow"},
            {158, "Quake"},
            {159, "Curaga"},
            {160, "Tornado"},
            {161, "Full-Life"},
            {162, "Reflect"},
            {163, "Aura"},
            {164, "Quake"},
            {165, "Double"},
            {166, "Break"},
            {167, "Meteor"},
            {168, "Ultima"},
            {169, "Triple"},
            {170, "Confuse"},
            {171, "Blind"},
            {172, "Quake"},
            {173, "Sleep"},
            {174, "Silence"},
            {175, "Flare"},
            {176, "Death"},
            {177, "Drain"},
            {178, "Pain"},
            {179, "Berserk"},
            {180, "Float"},
            {181, "Zombie"},
            {182, "Meltdown"},
            {183, "Ultima"},
            {184, "Tornado"},
            {185, "Quake"},
            {186, "Meteor"},
            {187, "Holy"},
            {188, "Flare"},
            {189, "Aura"},
            {190, "Ultima"},
            {191, "Triple"},
            {192, "Full-Life"},
            {193, "Tornado"},
            {194, "Quake"},
            {195, "Meteor"},
            {196, "Holy"},
            {197, "Flare"},
            {198, "Aura"},
            {199, "Ultima"},
            {200, "Triple"},
            {201, "Full-Life"},
            {202, "Tornado"},
            {203, "Quake"},
            {204, "Meteor"},
            {205, "Holy"},
            {206, "Flare"},
            {207, "Aura"},
            {208, "Ultima"},
            {209, "Triple"},
            {210, "Full-Life"},
            {211, "Ultima"},
            {212, "Meteor"},
            {213, "Holy"},
            {214, "Flare"},
            {215, "Aura"},
            {216, "Ultima"},
            {217, "Triple"},
            {218, "Full-Life"},
            {219, "Meteor"},
            {220, "Holy"},
            {221, "Triple"},
            {222, "Aura"},
            {223, "Ultima"},
            {224, "Triple"},
            {225, "Full-Life"},
            {226, "Meteor"},
            {227, "Holy"},
            {228, "Flare"},
            {229, "Aura"},
            {230, "Ultima"},
            {231, "Triple"},
            {232, "Full-Life"},
            {233, "Meteor"},
            {234, "Triple"},
            {235, "Flare"},
            {236, "Aura"},
            {237, "Ultima"},
            {238, "Triple"},
            {239, "Full-Life"},
            {240, "Meteor"},
            {241, "Holy"},
            {242, "Flare"},
            {243, "Aura"},
            {244, "Ultima"},
            {245, "Blizzard"},
            {246, "Cure"},
            {247, "Dispel"},
            {248, "Confuse"},
            {249, "Meteor"},
            {250, "Double"},
            {251, "Aura"},
            {252, "Holy"},
            {253, "Flare"},
            {254, "Ultima"},
            {255, "Scan"}
        };
        #endregion
        #region FF8Vaiables
        /*
        // ref http://wiki.ffrtt.ru/index.php/FF8/Variables
        // copied the table into excel and tried to changed the list into c# code.
        // I was thinking atleast some of it would be useful.
        enum Characters
        {
            // I noticed some values were in order of these characters so I made those values into arrays and put the character names into an enum.
            Squall, Zell, Irvine, Quistis, Rinoa, Selphie, Seifer, Edea, Count // Count is the last value and is the number of characters in enum
        }
        struct FF8Variables
        {
            //unsafe fixed byte byte03[4]; //[0-3]unused in fields (always "FF-8")
            ulong Steps; //[4]Steps (used to generate random encounters)
            ulong Payslip; //[8]Payslip
            //unsafe fixed byte byte1215[4]; //[12-15]unused in fields
            short SeedRankPts; //[16]SeeD rank points?
            //unsafe fixed byte byte1819[2]; //[18-19]unused in fields
            ulong BattlesWon; //[20]Battles won. (Fun fact: this affects the basketball shot in Trabia.)
            unsafe fixed byte byte2425[2]; //[24-25]unused in fields
            ushort EscapedBattles; //[26]Battles escaped.
            unsafe fixed ushort EnemiesKilled[(int)Characters.Count];

            //ushort ushort28; //[28]Enemies killed by Squall
            //ushort ushort30; //[30]Enemies killed by Zell
            //ushort ushort32; //[32]Enemies killed by Irvine
            //ushort ushort34; //[34]Enemies killed by Quistis
            //ushort ushort36; //[36]Enemies killed by Rinoa
            //ushort ushort38; //[38]Enemies killed by Selphie
            //ushort ushort40; //[40]Enemies killed by Seifer
            //ushort ushort42; //[42]Enemies killed by Edea

            unsafe fixed ushort DeathCounter[(int)Characters.Count];
            
            //ushort ushort44; //[44]Squall death count
            //ushort ushort46; //[46]Zell death count
            //ushort ushort48; //[48]Irvine death count
            //ushort ushort50; //[50]Quistis death count
            //ushort ushort52; //[52]Rinoa death count
            //ushort ushort54; //[54]Selphie death count
            //ushort ushort56; //[56]Seifer death count
            //ushort ushort58; //[58]Edea death count
            
            //unsafe fixed byte byte6067[8]; //[60-67]unused in fields
            ulong EnemiesKilledTotal; //[68]Enemies killed
            ulong Gill; //[72]Amount of Gil the party currently has
            ulong GillLaguna; //[76]Amount of Gil Laguna's party has
            ulong FMVFrames; //[80]Counts the number of frames since the current movie started playing. (default fps is 15?)
            ushort LastArea; //[84]Last area visited.
            sbyte CurrentCarRent; //[86]Current car rent.
            //sbyte sbyte87; //[87]Built-in engine variable. No idea what it does. Scripts always check if it's equal to 0 or 10. Related to music.
            //sbyte sbyte88; //[88]Built-in engine variable. Used exclusively on save points. Never written to with field scripts. Related to Siren's Move-Find ability.
            //unsafe fixed byte byte89103[15]; //[89-103]unused in fields
            ulong ulong104; //[104]Seems related to SARALYDISPON/SARALYON/MUSICLOAD/PHSPOWER opcodes
            ulong ulong108; //[108]Music related
            //ulong ulong112; //[112]unused in fields
            unsafe fixed byte DrawPtsFeild[32]; //[116-147]Draw points in field
            unsafe fixed byte DrawPtsWorld[31]; //[148-179]Draw points in worldmap
            unsafe fixed byte byte180255[76]; //[180-255]unused in fields
            ushort StoryQuestprogress; //[256]Main Story quest progress.
            //byte byte258; //[258]not investigated
            //unsafe fixed byte byte259260[2]; //[259-260]unused in fields
            //byte byte261; //[261]not investigated
            //unsafe fixed byte byte262263[2]; //[262-263]unused in fields
            //byte byte264; //[264]not investigated
            //byte byte265; //[265]not investigated
            byte WorldMapVersion; //[266]World map version? (3=Esthar locations unlocked)
            //byte byte267; //[267]unused in fields
            //byte byte268; //[268]not investigated
            //byte byte269; //[269]not investigated
            //byte byte270; //[270]not investigated
            //byte byte271; //[271]unused in fields
            //unsafe fixed byte byte272299[28]; //[272-299]SO MANY F***ING CARD GAME VARIABLES
            byte CardQueenrecards; //[300]Card Queen re-cards.
            //unsafe fixed byte byte301303[3]; //[301-303]unused in fields
            unsafe fixed byte TimberManiacsIssues[2]; //[304-305]Timber Maniacs issues found.
            unsafe fixed byte Hacktuar[14]; //[306-319]Reserved for Hacktuar / FF8Voice
            unsafe fixed byte UltimeciaGallery[3]; //[320-332]Ultimecia Gallery related (pictures viewed?)
            byte UltimeciaArmory; //[333]Ultimecia Armory chest flags
            byte UltimeciaCastle; //[334]Ultimecia Castle seals. See SEALEDOFF for details.
            byte Card; //[335]Card related
            byte BusRelated; //[336]Deling City bus related
            unsafe fixed byte GatesOpened[3]; //[338-340]Deling Sewer gates opened
            //byte byte341; //[341]Does lots of things.5
            byte BusSystem; //[342]Deling City bus system
            byte byte343; //[343]G-Garden door/event flags.
            byte byte344; //[344]B-Garden / G-Garden event flags (during GvG)
            byte byte345; //[345]G-Garden door/event flags.
            unsafe fixed byte byte346349[4]; //[346-349]FH Instrument (346 Zell, 347 Irvine, 348 Selphie, 349 Quistis)
            unsafe fixed ushort ushort350356[7]; //[350-356]Health Bars (Garden mech fight)
            byte byte358; //[358]Space station talk flags, Centra ruins related (beat odin?).
            byte byte359; //[359]Centra ruins related (beat odin?).
            ulong ulong360; //[360]Choice of FH music.
            unsafe fixed byte byte364368[5]; //[364-368]Randomly generated code for Centra Ruins.
            unsafe fixed byte byte369370[2]; //[369-370]Ultimecia Castle flags
            byte byte371; //[371]unused in fields
            unsafe fixed byte byte372376[5]; //[372-376]Ultimecia boss/timer/item flags
            byte byte377; //[377]Ultimecia organ note controller
            byte byte378; //[378]Centra Ruins timer (controls blackout messages from Odin)
            byte byte379; //[379]unused in fields
            ushort ushort380; //[380]Squall health during mech fight.
            unsafe fixed byte byte382383[2]; //[382-383]unused in fields
            byte byte384; //[384]Something about Laguna's time periods and GFs.
            byte byte385; //[385]Laguna dialogue in pub. Only the +2 bit is ever set. Don't change the +1 bit.
            byte byte387; //[387]Winhill progress?
            byte byte388; //[388]Timber Maniacs HQ talk flags (main lobby)
            byte byte389; //[389]Timber Maniacs HQ talk flags (office room)
            byte byte390; //[390]Edea talk flags at her house
            byte byte391; //[391]Laguna talk flags (in his office, disc 3)
            //byte byte392; //[392]unknown (used in Edea's house and in the Balamb Garden computer system)
            //unsafe fixed byte byte393399[7]; //[393-399]unused in fields
            //ulong ulong400and404; //[400 and 404]Related to monsters killed in Winhill, but I don't think it actually does anything. Will investigate.
            byte byte408; //[408]unused in fields
            byte byte409; //[409]Balamb Garden computer system
            unsafe fixed byte byte410431[22]; //[410-431]unused in fields
            byte byte432; //[432]BG Main hall flags
            byte byte433; //[433]Flags. Switches are assigned all over BG. No idea what any of them control.
            byte byte434; //[434]Flags. Switches are assigned all over BG. No idea what any of them control.
            byte byte435; //[435]Flags. Switches are assigned all over BG. No idea what any of them control.
            byte byte436; //[436]Moomba friendship level in the prison? Some actions cause these flags to be set.
            byte byte437; //[437]In BG on Disc 2, keeps track of who's in your party. In the prison, it's the current floor you're on.
            byte byte438; //[438]Cid vs Norg event flags
            byte byte439; //[439]Cid vs Norg event flags
            byte byte440; //[440]Event flags. (+1 Quad ambush, +2 quad item giver, +4/+8 Infirmary helped, +16 Nida, +64 Kadowaki Elixir, +128 Training center)
            byte byte441; //[441]Cid vs Norg event flags
            byte byte442; //[442]Rinoa Garden tour flags
            ushort ushort443; //[443]Zell Health in Prison (Hacktuar)
            unsafe fixed byte byte445447[3]; //[445-447]Propagator defeated flags
            ushort ushort448; //[448]Unknown
            unsafe fixed byte byte450451[2]; //[450-451]Various magazine/talk flags
            byte byte452; //[452]Lunatic Pandora areas visited?
            unsafe fixed byte byte453455[3]; //[453-455]Moomba teleport variables
            unsafe fixed byte byte456457[2]; //[456-457]unused in fields
            unsafe fixed byte byte458459[2]; //[458-459]Used with MUSICSKIP in some Balamb Garden areas
            byte byte460; //[460]Random flags (some used for Card Club)
            unsafe fixed byte byte461473[13]; //[461-473]unused in fields
            byte byte474; //[474]Random flags (some used for Card Club)
            unsafe fixed byte byte475478[4]; //[475-478]CC Group variables
            byte byte479; //[479]If set to 0, disables all random battles during area loading.
            byte byte480; //[480]State of students in classroom (what they're doing).
            byte byte481; //[481]Controls a conversation in the cafeteria.
            short short482; //[482]Error ratio of missiles
            byte byte484; //[484]Missile Base progression?
            byte byte485; //[485]ToUK Progression (initially 0b111010101, +2 on finish quest. No other pops)
            byte byte486; //[486]ToUK room? (used to control map jumps in the maze)
            byte byte487; //[487]Missile base progression (also does something in BG2F classroom)
            byte byte488; //[488]Alternate Party Flags. Irvine +1/+16, Quistis +2/+32, Rinoa +4/+64, Zell +8/+128.1
            byte byte489; //[489]Random talk flags?
            byte byte490; //[490]Cafeteria cutscene
            byte byte491; //[491]ToUK stuff
            byte byte492; //[492]I think this is a door opener for the missile base if you choose a short time limit.
            byte byte493; //[493]Missile base timer related?
            unsafe fixed byte byte494527[34]; //[494-527]unused in fields
            short short528; //[528]Sub-story progression (it's a progression variable for individual segments of the game)
            byte byte530; //[530]X-ATM related (defeated it in battle?)
            byte byte531; //[531]Functionally unused. Read from at dollet, only manipulated in debug rooms.
            byte byte532; //[532]Controls footstep sounds at dollet (sand to concrete)
            byte byte533; //[533]not investigated
            byte byte534; //[534]not investigated
            byte byte535; //[535]not investigated
            byte byte536; //[536]not investigated
            byte byte537; //[537]not investigated
            byte byte538; //[538]not investigated
            byte byte539; //[539]not investigated
            unsafe fixed byte byte540591[52]; //[540-591]unused in fields
            unsafe fixed byte byte592593[2]; //[592-593]Seems to control angles and character facing.
            byte byte594; //[594]unused in fields
            byte byte595; //[595]not investigated
            byte byte596; //[596]not investigated
            byte byte597; //[597]not investigated
            byte byte598; //[598]not investigated
            byte byte599; //[599]not investigated
            byte byte600; //[600]not investigated
            byte byte601; //[601]not investigated
            byte byte602; //[602]not investigated
            byte byte603; //[603]not investigated
            byte byte604; //[604]not investigated
            byte byte605; //[605]not investigated
            byte byte606; //[606]not investigated
            byte byte607; //[607]not investigated
            byte byte608; //[608]not investigated
            byte byte609; //[609]not investigated
            byte byte610; //[610]not investigated
            byte byte611; //[611]not investigated
            byte byte612; //[612]not investigated
            byte byte613; //[613]not investigated
            byte byte614; //[614]not investigated
            byte byte615; //[615]not investigated
            byte byte616; //[616]not investigated
            byte byte617; //[617]not investigated
            byte byte618; //[618]not investigated
            byte byte619; //[619]not investigated
            byte byte620; //[620]not investigated
            byte byte621; //[621]not investigated
            byte byte622; //[622]not investigated
            byte byte623; //[623]not investigated
            byte byte624; //[624]not investigated
            byte byte625; //[625]Balamb visited flags (+8 Zell's room)
            byte byte626; //[626]not investigated
            byte byte627; //[627]not investigated
            byte byte628; //[628]unused in fields
            byte byte629; //[629]not investigated
            byte byte630; //[630]not investigated
            byte byte631; //[631]not investigated
            byte byte632; //[632]not investigated
            byte byte633; //[633]not investigated
            ushort ushort634; //[634]not investigated
            byte byte636; //[636]not investigated
            byte byte637; //[637]unused in fields
            byte byte638; //[638]not investigated
            byte byte639; //[639]unused in fields
            byte byte640; //[640]not investigated
            byte byte641; //[641]not investigated
            byte byte642; //[642]not investigated
            byte byte643; //[643]not investigated
            byte byte644; //[644]not investigated
            byte byte645; //[645]not investigated
            byte byte646; //[646]not investigated
            byte byte647; //[647]not investigated
            byte byte648; //[648]not investigated
            byte byte649; //[649]not investigated
            unsafe fixed byte byte650655[6]; //[650-655]unused in fields
            ushort ushort656; //[656]not investigated
            byte byte658; //[658]not investigated
            byte byte659; //[659]not investigated
            byte byte660; //[660]not investigated
            byte byte661; //[661]not investigated
            byte byte662; //[662]not investigated
            byte byte663; //[663]not investigated
            byte byte664; //[664]not investigated
            byte byte665; //[665]not investigated
            ushort ushort666; //[666]not investigated
            byte byte668; //[668]not investigated
            unsafe fixed byte byte669671[3]; //[669-671]unused in fields
            ushort ushort672; //[672]not investigated
            byte byte674; //[674]unused in fields
            byte byte675; //[675]not investigated
            byte byte676; //[676]unused in fields
            byte byte677; //[677]not investigated
            byte byte678; //[678]not investigated
            byte byte679; //[679]unused in fields
            byte byte680; //[680]not investigated
            byte byte681; //[681]not investigated
            byte byte682; //[682]not investigated
            byte byte683; //[683]not investigated
            byte byte684; //[684]not investigated
            byte byte685; //[685]not investigated
            byte byte686; //[686]not investigated
            byte byte687; //[687]not investigated
            byte byte688; //[688]not investigated
            byte byte689; //[689]not investigated
            byte byte690; //[690]not investigated
            byte byte691; //[691]not investigated
            unsafe fixed byte byte692719[28]; //[692-719]unused in fields
            byte byte720; //[720]Squall's costume (0=normal, 1=student, 2=SeeD, 3=Bandage on forehead)
            byte byte721; //[721]Zell's Costume (0=normal, 1=student, 2=SeeD)
            byte byte722; //[722]Selphie's costume (0=normal, 1=student, 2=SeeD)
            byte byte723; //[723]Quistis' Costume (0=normal, 1=SeeD)
            ushort ushort724; //[724]Dollet mission time
            ushort ushort726; //[726]not investigated
            byte byte728; //[728]Does lots of things.3
            byte byte729; //[729]not investigated
            byte byte730; //[730]Flags (+1 Joined Garden Festival Committee, +4 Gave Selphie tour of BG, +16 Kadowaki asks for Cid, +32 and +64 Tomb of Unknown Kind hints?, +128 Beat all card people?)
            byte byte731; //[731]unused in fields
            ushort ushort732; //[732]not investigated
            byte byte734; //[734]Split Party Flags (+1 Zell, +2 Irvine, +4 Rinoa, +8 Quistis, +16 Selphie).2
            byte byte735; //[735]not investigated
            unsafe fixed byte byte736751[16]; //[736-751]unused in fields
            byte byte752; //[752]not investigated
            unsafe fixed byte byte7531023[271]; //[753-1023]unused in fields
            byte byteAbove1023; //[Above 1023]Temporary variables used pretty much everywhere.
        }
*/
        #endregion

        public static class Archives
        {
            public static string A_BATTLE = MakiExtended.GetUnixFullPath(Path.Combine(FF8DIR, "battle"));
            public static string A_FIELD = MakiExtended.GetUnixFullPath(Path.Combine(FF8DIR, "field"));
            public static string A_MAGIC = MakiExtended.GetUnixFullPath(Path.Combine(FF8DIR, "magic"));
            public static string A_MAIN = MakiExtended.GetUnixFullPath(Path.Combine(FF8DIR, "main"));
            public static string A_MENU = MakiExtended.GetUnixFullPath(Path.Combine(FF8DIR, "menu"));
            public static string A_WORLD = MakiExtended.GetUnixFullPath(Path.Combine(FF8DIR, "world"));

            public const string B_FileList = ".fl";
            public const string B_FileIndex = ".fi";
            public const string B_FileArchive = ".fs";
        }

        public static class FieldHolder
        {
            //public static string[] MapList;
            public static ushort FieldID = 161;
            public static string[] fields;
            public static int[] FieldMemory;
        }
    }
}