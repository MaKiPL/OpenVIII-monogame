using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#pragma warning disable CS0649

namespace FF8
{
    internal static class Memory
    {
        //monogame
        public static GraphicsDeviceManager graphics;

        public static SpriteBatch spriteBatch;
        public static ContentManager content;

        public static bool IsActive = true;
        public static Font font;
        //public static Texture2D[] iconsTex;

        public static Cards Cards;
        public static Faces Faces;
        public static Icons Icons;
        public static Strings Strings;
        public static Kernel_bin Kernel_Bin;

        public static Texture2D shadowTexture;
        public static VertexPositionTexture[] shadowGeometry;
        public enum ScaleMode
        {
            /// <summary>
            /// scale object to have the same height as viewport
            /// </summary>
            FitVertical,
            /// <summary>
            /// scale object to have the same width as viewport
            /// </summary>
            FitHorizontal,
            /// <summary>
            /// Same as FitVertical unless width is too large, then it becomes FitHorizontal
            /// </summary>
            FitBoth,
            /// <summary>
            /// fill the entire viewport
            /// </summary>
            Stretch
        }

        public static Point Center => new Point(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);
        public const ScaleMode _scaleMode = ScaleMode.Stretch;

        public static Vector2 Scale(float Width = PreferredViewportWidth, float Height = PreferredViewportHeight, ScaleMode scaleMode = _scaleMode, int targetX = 0, int targetY = 0)
        {
            if (targetX == 0)
                targetX = graphics.GraphicsDevice.Viewport.Width;
            if (targetY == 0)
                targetY = graphics.GraphicsDevice.Viewport.Height;
            float h = targetX / Width;
            float v = targetY / Height;
            switch (scaleMode)
            {
#pragma warning disable CS0162 // Unreachable code detected
                case ScaleMode.FitHorizontal:
                    return new Vector2(h, h);

                case ScaleMode.FitVertical:
                    return new Vector2(v, v);
                case ScaleMode.FitBoth:
                    return (v * Width > targetX)? new Vector2(h, h): new Vector2(v, v);
                case ScaleMode.Stretch:
                default:
                    return new Vector2(h, v);
#pragma warning restore CS0162 // Unreachable code detected
            }
        }

        //original resolution I am working on, therefore if user scales it we need to propertially scale everything
        public const int PreferredViewportWidth = 1280;

        public const int PreferredViewportHeight = 720;

        public static GameTime gameTime;

        private static ushort prevmusic = 0;
        private static ushort currmusic = 0;
        /// <summary>
        /// Stores current savestate. When you save this is wrote. When you load this is replaced.
        /// </summary>
        public static Saves.Data State = new Saves.Data();
        internal static ushort MusicIndex
        {
            get
            {
                if (dicMusic.Count > 0)
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
                    while (dicMusic.Count > 0 && prevmusic < currmusic && !dicMusic.ContainsKey(currmusic))
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
                else return 0;
            }
            set
            {
                prevmusic = currmusic;
                currmusic = value;
            }
        }

        public static string[] musices;
        public static readonly Dictionary<ushort, List<string>> dicMusic = new Dictionary<ushort, List<string>>(); //ogg and sgt files have same 3 digit prefix.

        public static void SpriteBatchStartStencil(SamplerState ss = null) => spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, ss, graphics.GraphicsDevice.DepthStencilState);

        public static void SpriteBatchStartAlpha(SamplerState ss = null, Matrix? tm = null) => spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: ss ?? SamplerState.PointClamp, transformMatrix: tm);

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
        public static string FF8DIRdata { get; private set; }
        public static string FF8DIRdata_lang { get; private set; }

        public static void Init(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, ContentManager content)
        {
            FF8DIRdata = MakiExtended.GetUnixFullPath(Path.Combine(FF8DIR, "Data"));
            string testdir = MakiExtended.GetUnixFullPath(Path.Combine(FF8DIRdata, "lang-en"));
            FF8DIRdata_lang = Directory.Exists(testdir) ? testdir : FF8DIRdata;

            Memory.graphics = graphics;
            Memory.spriteBatch = spriteBatch;
            Memory.content = content;
            Memory.DirtyEncoding = new DirtyEncoding();
            Memory.FieldHolder.FieldMemory = new int[1024];

            Memory.font = new Font(); //this initializes the fonts and drawing system- holds fonts in-memory
            Memory.Strings = new Strings();

            Kernel_Bin = new Kernel_bin();
#if DEBUG
            //export the string data so you can find where the string you want is.
            // then you can Memory.Strings.Read() it :)
            try
            {
                string dumpfolder = Path.GetTempPath();
                if (Directory.Exists(dumpfolder))
                {
                    Memory.Strings.Dump(Strings.FileID.MNGRP, Path.Combine(dumpfolder, "MNGRPdump.txt"));
                    Memory.Strings.Dump(Strings.FileID.AREAMES, Path.Combine(dumpfolder, "AREAMESdump.txt"));
                    Memory.Strings.Dump(Strings.FileID.NAMEDIC, Path.Combine(dumpfolder, "NAMEDICdump.txt"));
                    Memory.Strings.Dump(Strings.FileID.KERNEL, Path.Combine(dumpfolder, "KERNELdump.txt"));
                }
            }
            catch (IOException)
            {

            }

            Memory.Cards = new Cards();
            Memory.Faces = new Faces();
            Memory.Icons = new Icons();

#endif
        }
        /// <summary>
        /// If true by the end of Update() will skip the next Draw()
        /// </summary>
        public static bool SuppressDraw { get; internal set; }

        public static bool IsMouseVisible { get; internal set; } = false;
        public static DirtyEncoding DirtyEncoding;

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
        public const int MODULE_CARD_TEST = -22;

        #endregion modules

        #region battleProvider

        /// <summary>
        /// Active battle encounter. Set by field or battle module. You shouldn't change it in-battle. 
        /// </summary>
        public static int battle_encounter = 0;
        /// <summary>
        /// Battle music pointer. Set by SETBATTLEMUSIC in field module or by world module. Default=6
        /// </summary>
        public static int SetBattleMusic = 6;
        public static Init_debugger_battle.Encounter[] encounters;

        #endregion battleProvider

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

        #endregion MusicDataMidi

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

        #endregion MusicDataOGG

        #region DrawPointMagic

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
        internal static Random random;

        #endregion DrawPointMagic


        public static class Archives
        {
            public static string A_BATTLE = MakiExtended.GetUnixFullPath(Path.Combine(FF8DIRdata_lang, "battle"));
            public static string A_FIELD = MakiExtended.GetUnixFullPath(Path.Combine(FF8DIRdata_lang, "field"));
            public static string A_MAGIC = MakiExtended.GetUnixFullPath(Path.Combine(FF8DIRdata_lang, "magic"));
            public static string A_MAIN = MakiExtended.GetUnixFullPath(Path.Combine(FF8DIRdata_lang, "main"));
            public static string A_MENU = MakiExtended.GetUnixFullPath(Path.Combine(FF8DIRdata_lang, "menu"));
            public static string A_WORLD = MakiExtended.GetUnixFullPath(Path.Combine(FF8DIRdata_lang, "world"));

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