using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private class IGM_Junction : Menu
        {
            public enum Items
            {
                Junction,
                Off,
                Auto,
                Ability,
                HP,
                Str,
                Vit,
                Mag,
                Spr,
                Spd,
                Luck,
                Hit,
                ST_A,
                ST_D,
                EL_A,
                EL_D,
                ST_A_D,
                EL_A_D,
                Stats,
                ST_A2,
                GF,
                Magic,
                AutoAtk,
                AutoMag,
                AutoDef,
                RemAll,
                RemMag,
                ChooseGFtojunction,
                Chooseslottojunction,
                Choosemagictojunction,
                RemovealljunctionedGFandmagic,
                Removealljunctionedmagic,
                CurrentEXP,
                NextLEVEL,
                _,
                LV,
                ForwardSlash,
                Percent
            }

            public enum SectionName
            {
                /// <summary>
                /// Junction OFF Auto Ability
                /// </summary>
                TopMenu,

                /// <summary>
                /// Top Right
                /// </summary>
                Title,

                /// <summary>
                /// Description Help
                /// </summary>
                Help,

                /// <summary>
                /// Character Stats
                /// </summary>
                Stats,

                /// <summary>
                /// 4 Commands you can use in battle
                /// </summary>
                Commands,

                /// <summary>
                /// Portrait Name HP EXP Rank?
                /// </summary>
                CharacterInfo,
                TopMenu_Junction,
            }

            public static Dictionary<Items, FF8String> Titles { get; private set; }
            public static Dictionary<Items, FF8String> Misc { get; private set; }
            public static Dictionary<Items, FF8String> Descriptions { get; private set; }
            /// <summary>
            /// Character who has the junctions and inventory. Same as VisableCharacter unless TeamLaguna.
            /// </summary>
            public static Saves.Characters Character { get; private set; }
            /// <summary>
            /// Required to support Laguna's Party. They have unique stats but share junctions and inventory.
            /// </summary>
            public static Saves.Characters VisableCharacter { get; private set; }

            public override bool Update()
            {
                base.Update();
                return Inputs();
            }

            protected override void Init()
            {
                Size = new Vector2 { X = 840, Y = 630 };
                TextScale = new Vector2(2.545455f, 3.0375f);

                Titles = new Dictionary<Items, FF8String> {
                    {Items.Junction, Memory.Strings.Read(Strings.FileID.MNGRP,2,217) },
                    {Items.Off, Memory.Strings.Read(Strings.FileID.MNGRP,2,219) },
                    {Items.Auto, Memory.Strings.Read(Strings.FileID.MNGRP,2,221) },
                    {Items.Ability, Memory.Strings.Read(Strings.FileID.MNGRP,2,223) },
                    {Items.HP, Memory.Strings.Read(Strings.FileID.MNGRP,2,225) },
                    {Items.Str, Memory.Strings.Read(Strings.FileID.MNGRP,2,227) },
                    {Items.Vit, Memory.Strings.Read(Strings.FileID.MNGRP,2,229) },
                    {Items.Mag, Memory.Strings.Read(Strings.FileID.MNGRP,2,231) },
                    {Items.Spr, Memory.Strings.Read(Strings.FileID.MNGRP,2,233) },
                    {Items.Spd, Memory.Strings.Read(Strings.FileID.MNGRP,2,235) },
                    {Items.Luck, Memory.Strings.Read(Strings.FileID.MNGRP,2,237) },
                    {Items.Hit, Memory.Strings.Read(Strings.FileID.MNGRP,2,239) },
                    {Items.ST_A,Memory.Strings.Read(Strings.FileID.MNGRP,2,243)},
                    {Items.ST_D,Memory.Strings.Read(Strings.FileID.MNGRP,2,245)},
                    {Items.EL_A,Memory.Strings.Read(Strings.FileID.MNGRP,2,247)},
                    {Items.EL_D,Memory.Strings.Read(Strings.FileID.MNGRP,2,249)},
                    {Items.ST_A_D,Memory.Strings.Read(Strings.FileID.MNGRP,2,251)},
                    {Items.EL_A_D,Memory.Strings.Read(Strings.FileID.MNGRP,2,253)},
                    {Items.Stats,Memory.Strings.Read(Strings.FileID.MNGRP,2,255)},
                    { Items.ST_A2,Memory.Strings.Read(Strings.FileID.MNGRP, 2, 257)},
                    {Items.GF,Memory.Strings.Read(Strings.FileID.MNGRP,2,262)},
                    { Items.Magic,Memory.Strings.Read(Strings.FileID.MNGRP, 2, 264)},
                    {Items.AutoAtk,Memory.Strings.Read(Strings.FileID.MNGRP,2,269)},
                    {Items.AutoMag,Memory.Strings.Read(Strings.FileID.MNGRP,2,271)},
                    {Items.AutoDef,Memory.Strings.Read(Strings.FileID.MNGRP,2,273)},
                    {Items.RemAll,Memory.Strings.Read(Strings.FileID.MNGRP,2,275)},
                    {Items.RemMag,Memory.Strings.Read(Strings.FileID.MNGRP,2,277)},
                };

                Misc = new Dictionary<Items, FF8String> {
                { Items.CurrentEXP, Memory.Strings.Read(Strings.FileID.MNGRP, 0, 23)  },
                { Items.NextLEVEL, Memory.Strings.Read(Strings.FileID.MNGRP, 0, 24)  },
                { Items._,Memory.Strings.Read(Strings.FileID.MNGRP,2,266)},
                { Items.HP,Memory.Strings.Read(Strings.FileID.MNGRP,0,26)},
                { Items.LV,Memory.Strings.Read(Strings.FileID.MNGRP,0,27)},
                { Items.ForwardSlash,Memory.Strings.Read(Strings.FileID.MNGRP,0,25)},
                { Items.Percent,Memory.Strings.Read(Strings.FileID.MNGRP,0,29)},
                };
                //{Items.ST_D,Memory.Strings.Read(Strings.FileID.MNGRP,2,259)},
                //{Items.EL_A,Memory.Strings.Read(Strings.FileID.MNGRP,2,260)},
                //{Items.EL_D,Memory.Strings.Read(Strings.FileID.MNGRP,2,261)},
                //{Items.Areyousure?,Memory.Strings.Read(Strings.FileID.MNGRP,2,267)},
                //{Items.Keepprevioussetting,Memory.Strings.Read(Strings.FileID.MNGRP,2,268)},
                //{Items.Junctionedto<Special:0xA27>,Memory.Strings.Read(Strings.FileID.MNGRP,2,284)},
                //{Items.Empty,Memory.Strings.Read(Strings.FileID.MNGRP,2,285)},
                //{Items.BasicOperation,Memory.Strings.Read(Strings.FileID.MNGRP,2,286)},
                //{Items.BasicControlExplanationinFFVIII,Memory.Strings.Read(Strings.FileID.MNGRP,2,287)},
                //{Items.BattleOperation,Memory.Strings.Read(Strings.FileID.MNGRP,2,288)},
                //{Items.BattleExplanation,Memory.Strings.Read(Strings.FileID.MNGRP,2,289)},
                //{Items.CardGameRules,Memory.Strings.Read(Strings.FileID.MNGRP,2,290)},
                //{Items.CardGameExplanation,Memory.Strings.Read(Strings.FileID.MNGRP,2,291)},
                //{Items.OnlineHelp,Memory.Strings.Read(Strings.FileID.MNGRP,2,292)},
                //{Items.ExplanationofVariousFeatures,Memory.Strings.Read(Strings.FileID.MNGRP,2,293)},
                //{Items.GFJunction,Memory.Strings.Read(Strings.FileID.MNGRP,2,294)},
                //{Items.JunctioningaGFandsettingcommands,Memory.Strings.Read(Strings.FileID.MNGRP,2,295)},
                //{Items.MagicJunction,Memory.Strings.Read(Strings.FileID.MNGRP,2,296)},
                //{Items.Explanationonjunctioningmagic,Memory.Strings.Read(Strings.FileID.MNGRP,2,297)},
                //{Items.JunctiontoElements,Memory.Strings.Read(Strings.FileID.MNGRP,2,298)},
                //{Items.Explanationofelementaljunction,Memory.Strings.Read(Strings.FileID.MNGRP,2,299)},
                //{Items.JunctionofStatus,Memory.Strings.Read(Strings.FileID.MNGRP,2,300)},
                //{Items.Explanationofstatusjunction,Memory.Strings.Read(Strings.FileID.MNGRP,2,301)},

                Descriptions = new Dictionary<Items, FF8String> {
                    {Items.Junction, Memory.Strings.Read(Strings.FileID.MNGRP,2,218) },
                    {Items.Off, Memory.Strings.Read(Strings.FileID.MNGRP,2,220) },
                    {Items.Auto, Memory.Strings.Read(Strings.FileID.MNGRP,2,222) },
                    {Items.Ability, Memory.Strings.Read(Strings.FileID.MNGRP,2,224) },
                    {Items.HP, Memory.Strings.Read(Strings.FileID.MNGRP,2,226) },
                    {Items.Str, Memory.Strings.Read(Strings.FileID.MNGRP,2,228) },
                    {Items.Vit, Memory.Strings.Read(Strings.FileID.MNGRP,2,230) },
                    {Items.Mag, Memory.Strings.Read(Strings.FileID.MNGRP,2,232) },
                    {Items.Spr, Memory.Strings.Read(Strings.FileID.MNGRP,2,234) },
                    {Items.Spd, Memory.Strings.Read(Strings.FileID.MNGRP,2,236) },
                    {Items.Luck, Memory.Strings.Read(Strings.FileID.MNGRP,2,238) },
                    {Items.Hit, Memory.Strings.Read(Strings.FileID.MNGRP,2,240) },
                    {Items.ST_A,Memory.Strings.Read(Strings.FileID.MNGRP,2,244)},
                    {Items.ST_D,Memory.Strings.Read(Strings.FileID.MNGRP,2,246)},
                    {Items.EL_A,Memory.Strings.Read(Strings.FileID.MNGRP,2,248)},
                    {Items.EL_D,Memory.Strings.Read(Strings.FileID.MNGRP,2,250)},
                    {Items.ST_A_D,Memory.Strings.Read(Strings.FileID.MNGRP,2,252)},
                    {Items.EL_A_D,Memory.Strings.Read(Strings.FileID.MNGRP,2,254)},
                    { Items.Stats,Memory.Strings.Read(Strings.FileID.MNGRP,2,256)},
                    {Items.ST_A2,Memory.Strings.Read(Strings.FileID.MNGRP,2,258)},
                    { Items.GF,Memory.Strings.Read(Strings.FileID.MNGRP,2,263)},
                    {Items.Magic,Memory.Strings.Read(Strings.FileID.MNGRP,2,265)},
                    {Items.AutoAtk,Memory.Strings.Read(Strings.FileID.MNGRP,2,270)},
                    {Items.AutoMag,Memory.Strings.Read(Strings.FileID.MNGRP,2,272)},
                    {Items.AutoDef,Memory.Strings.Read(Strings.FileID.MNGRP,2,274)},
                    {Items.RemAll,Memory.Strings.Read(Strings.FileID.MNGRP,2,276)},
                    {Items.RemMag,Memory.Strings.Read(Strings.FileID.MNGRP,2,278)},
                    {Items.RemovealljunctionedGFandmagic,Memory.Strings.Read(Strings.FileID.MNGRP,2,279)},
                    {Items.Removealljunctionedmagic,Memory.Strings.Read(Strings.FileID.MNGRP,2,280)},
                    {Items.ChooseGFtojunction,Memory.Strings.Read(Strings.FileID.MNGRP,2,281)},
                    {Items.Chooseslottojunction,Memory.Strings.Read(Strings.FileID.MNGRP,2,282)},
                    {Items.Choosemagictojunction,Memory.Strings.Read(Strings.FileID.MNGRP,2,283)},
                };

                Data.Add(SectionName.CharacterInfo, new IGMData_CharacterInfo());
                Data.Add(SectionName.Stats, new IGMData_Stats());
                Data.Add(SectionName.Commands, new IGMData_Commands());
                Data.Add(SectionName.Help, new IGMData_Help());
                Data.Add(SectionName.TopMenu, new IGMData_TopMenu());
                Data.Add(SectionName.Title, new IGMData_Title());
                Data.Add(SectionName.TopMenu_Junction, new IGMData_TopMenu_Junction());
                base.Init();
            }

            public void ReInit(Saves.Characters c, Saves.Characters vc)
            {
                Character = c;
                VisableCharacter = vc;
                ReInit();
            }
            public new enum Mode
            {
                TopMenu,
                TopMenu_Junction
            }
            public new Mode mode;
            protected override bool Inputs()
            {
                bool ret = false;
                switch(mode)
                {
                    case Mode.TopMenu:
                        ret = ((IGMData_TopMenu)Data[SectionName.TopMenu]).Inputs();
                        break;
                    case Mode.TopMenu_Junction:
                        ret = ((IGMData_TopMenu_Junction)Data[SectionName.TopMenu_Junction]).Inputs();
                        break;
                    default:
                        break;
                        
                }
                return ret;
            }

            public override void Draw()
            {
                StartDraw();
                //switch (mode)
                //{
                //    case Mode.ChooseChar:
                ////    case Mode.ChooseItem:
                // default:
                base.Draw();
                //break;
                //}
                //switch (mode)
                //{
                //    case Mode.ChooseChar:
                //        DrawPointer(Data[SectionName.SideMenu].CURSOR[(int)choSideBar], blink: true);

                // if (choChar < Data[SectionName.Party].Count && choChar >= 0)
                // DrawPointer(Data[SectionName.Party].CURSOR[choChar]); else if (choChar <
                // Data[SectionName.Non_Party].Count + Data[SectionName.Party].Count && choChar >=
                // Data[SectionName.Party].Count)
                // DrawPointer(Data[SectionName.Non_Party].CURSOR[choChar -
                // Data[SectionName.Party].Count]); break;

                //    default:
                //        DrawPointer(Data[SectionName.SideMenu].CURSOR[(int)choSideBar]);
                //        break;
                //}
                EndDraw();
            }

            private class IGMData_CharacterInfo : IGMData
            {
                public IGMData_CharacterInfo() : base(1, 15, new IGMDataItem_Empty(new Rectangle(20, 153, 395, 255)))
                {
                }

                /// <summary>
                /// Things that may of changed before screen loads or junction is changed.
                /// </summary>
                public override void ReInit()
                {
                    base.ReInit();
                    ITEM[0, 0] = new IGMDataItem_Face((Faces.ID)VisableCharacter, new Rectangle(X + 12, Y, 96, 144));
                    ITEM[0, 2] = new IGMDataItem_String(Memory.Strings.GetName(VisableCharacter), new Rectangle(X + 117, Y + 0, 0, 0));


                    if (Memory.State.Characters != null)
                    {

                        ITEM[0, 4] = new IGMDataItem_Int(Memory.State.Characters[(int)Character].Level, new Rectangle(X + 117 + 35, Y + 54, 0, 0), 13, numtype: Icons.NumType.sysFntBig, padding: 1, spaces: 6);
                        ITEM[0, 5] = Memory.State.Party != null && Memory.State.Party.Contains(Character)
                            ? new IGMDataItem_Icon(Icons.ID.InParty, new Rectangle(X + 278, Y + 48, 0, 0), 6)
                            : null;
                        ITEM[0, 7] = new IGMDataItem_Int(Memory.State.Characters[(int)Character].CurrentHP, new Rectangle(X + 152, Y + 108, 0, 0), 13, numtype: Icons.NumType.sysFntBig, padding: 1, spaces: 6);
                        ITEM[0, 9] = new IGMDataItem_Int(Memory.State.Characters[(int)Character].MaxHP(VisableCharacter), new Rectangle(X + 292, Y + 108, 0, 0), 13, numtype: Icons.NumType.sysFntBig, padding: 1, spaces: 5);
                        ITEM[0, 11] = new IGMDataItem_Int((int)Memory.State.Characters[(int)Character].Experience, new Rectangle(X + 192, Y + 198, 0, 0), 13, numtype: Icons.NumType.Num_8x8_2, padding: 1, spaces: 9);
                        ITEM[0, 13] = new IGMDataItem_Int(Memory.State.Characters[(int)Character].ExperienceToNextLevel, new Rectangle(X + 192, Y + 231, 0, 0), 13, numtype: Icons.NumType.Num_8x8_2, padding: 1, spaces: 9);
                    }
                }

                /// <summary>
                /// Things fixed at startup.
                /// </summary>
                protected override void Init()
                {
                    ITEM[0, 1] = new IGMDataItem_Icon(Icons.ID.MenuBorder, new Rectangle(X + 10, Y - 2, 100, 148), scale: new Vector2(1f));
                    ITEM[0, 3] = new IGMDataItem_String(Misc[Items.LV], new Rectangle(X + 117, Y + 54, 0, 0));
                    ITEM[0, 6] = new IGMDataItem_String(Misc[Items.HP], new Rectangle(X + 117, Y + 108, 0, 0));
                    ITEM[0, 8] = new IGMDataItem_String(Misc[Items.ForwardSlash], new Rectangle(X + 272, Y + 108, 0, 0));
                    ITEM[0, 10] = new IGMDataItem_String(Misc[Items.CurrentEXP] + new FF8String("\n") + Misc[Items.NextLEVEL], new Rectangle(X, Y + 192, 0, 0));
                    ITEM[0, 12] = new IGMDataItem_Icon(Icons.ID.P, new Rectangle(X + 372, Y + 198, 0, 0), 2);
                    ITEM[0, 14] = new IGMDataItem_Icon(Icons.ID.P, new Rectangle(X + 372, Y + 231, 0, 0), 2);
                    base.Init();
                }
            }

            private class IGMData_Stats : IGMData
            {
                /// <summary>
                /// Convert stat to stat junction
                /// </summary>
                private static Dictionary<Kernel_bin.Stat, Kernel_bin.Abilities> Stat2Ability = new Dictionary<Kernel_bin.Stat, Kernel_bin.Abilities>
                {
                    { Kernel_bin.Stat.HP, Kernel_bin.Abilities.HP_J },
                    { Kernel_bin.Stat.STR, Kernel_bin.Abilities.Str_J },
                    { Kernel_bin.Stat.VIT, Kernel_bin.Abilities.Vit_J},
                    { Kernel_bin.Stat.MAG, Kernel_bin.Abilities.Mag_J},
                    { Kernel_bin.Stat.SPR, Kernel_bin.Abilities.Spr_J },
                    { Kernel_bin.Stat.SPD, Kernel_bin.Abilities.Spd_J },
                    { Kernel_bin.Stat.EVA, Kernel_bin.Abilities.Eva_J },
                    { Kernel_bin.Stat.LUCK, Kernel_bin.Abilities.Luck_J },
                    { Kernel_bin.Stat.HIT, Kernel_bin.Abilities.Hit_J },
                };

                public IGMData_Stats() : base(10, 4, new IGMDataItem_Box(pos: new Rectangle(0, 414, 840, 216)))
                {
                }
                /// <summary>
                /// Convert stat to correct icon id.
                /// </summary>
                private static Dictionary<Kernel_bin.Stat, Icons.ID> Stat2Icon = new Dictionary<Kernel_bin.Stat, Icons.ID>
                {
                    { Kernel_bin.Stat.HP, Icons.ID.Stats_Hit_Points },
                    { Kernel_bin.Stat.STR, Icons.ID.Stats_Strength },
                    { Kernel_bin.Stat.VIT, Icons.ID.Stats_Vitality },
                    { Kernel_bin.Stat.MAG, Icons.ID.Stats_Magic },
                    { Kernel_bin.Stat.SPR, Icons.ID.Stats_Spirit },
                    { Kernel_bin.Stat.SPD, Icons.ID.Stats_Speed },
                    { Kernel_bin.Stat.EVA, Icons.ID.Stats_Evade },
                    { Kernel_bin.Stat.LUCK, Icons.ID.Stats_Luck },
                    { Kernel_bin.Stat.HIT, Icons.ID.Stats_Hit_Percent },
                };

                /// <summary>
                /// Things that may of changed before screen loads or junction is changed.
                /// </summary>
                public override void ReInit()
                {
                    base.ReInit();

                    if (Memory.State.Characters != null)
                    {
                        Kernel_bin.Abilities[] unlocked = Memory.State.Characters[(int)Character].UnlockedGFAbilities;
                        ITEM[5, 0] = new IGMDataItem_Icon(Icons.ID.Icon_Status_Attack, new Rectangle(SIZE[5].X + 200, SIZE[5].Y, 0, 0),
                            (byte)(unlocked.Contains(Kernel_bin.Abilities.ST_Atk_J) ? 2 : 7));
                        ITEM[5, 1] = new IGMDataItem_Icon(Icons.ID.Icon_Status_Defense, new Rectangle(SIZE[5].X + 240, SIZE[5].Y, 0, 0),
                            (byte)(unlocked.Contains(Kernel_bin.Abilities.ST_Def_J) ||
                            unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx2) ||
                            unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx4) ? 2 : 7));
                        ITEM[5, 2] = new IGMDataItem_Icon(Icons.ID.Icon_Elemental_Attack, new Rectangle(SIZE[5].X + 280, SIZE[5].Y, 0, 0),
                            (byte)(unlocked.Contains(Kernel_bin.Abilities.Elem_Atk_J) ? 2 : 7));
                        ITEM[5, 3] = new IGMDataItem_Icon(Icons.ID.Icon_Elemental_Defense, new Rectangle(SIZE[5].X + 320, SIZE[5].Y, 0, 0),
                            (byte)(unlocked.Contains(Kernel_bin.Abilities.Elem_Def_J) ||
                            unlocked.Contains(Kernel_bin.Abilities.Elem_Defx2) ||
                            unlocked.Contains(Kernel_bin.Abilities.Elem_Defx4) ? 2 : 7));
                        foreach (Kernel_bin.Stat stat in (Kernel_bin.Stat[])Enum.GetValues(typeof(Kernel_bin.Stat)))
                        {
                            int pos = (int)stat;
                            if (pos >= 5) pos++;
                            FF8String name = Kernel_bin.MagicData[Memory.State.Characters[(int)Character].JunctionStat[stat]].Name;
                            if (name.Length == 0) name = Misc[Items._];

                            ITEM[pos, 0] = new IGMDataItem_Icon(Stat2Icon[stat], new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0), 2);
                            ITEM[pos, 1] = new IGMDataItem_String(name, new Rectangle(SIZE[pos].X + 80, SIZE[pos].Y, 0, 0));
                            if (!unlocked.Contains(Stat2Ability[stat]))
                            {
                                ((IGMDataItem_Icon)ITEM[pos, 0]).Pallet = ((IGMDataItem_Icon)ITEM[pos, 0]).Faded_Pallet = 7;
                                ((IGMDataItem_String)ITEM[pos, 1]).Colorid = Font.ColorID.Grey;
                            }
                            ITEM[pos, 2] = new IGMDataItem_Int(Memory.State.Characters[(int)Character].TotalStat(stat, VisableCharacter), new Rectangle(SIZE[pos].X + 152, SIZE[pos].Y, 0, 0), 2, Icons.NumType.sysFntBig, spaces: 10);
                            ITEM[pos, 3] = stat == Kernel_bin.Stat.HIT || stat == Kernel_bin.Stat.EVA
                                ? new IGMDataItem_String(Misc[Items.Percent], new Rectangle(SIZE[pos].X + 350, SIZE[pos].Y, 0, 0))
                                : null;
                        }
                    }
                }

                /// <summary>
                /// Things fixed at startup.
                /// </summary>
                protected override void Init()
                {
                    int rows = 5;
                    int cols = 2;
                    for (int i = 0; i < SIZE.Length; i++)
                    {
                        int col = i / rows;
                        int row = i % rows;
                        SIZE[i] = new Rectangle
                        {
                            X = X + (Width * col) / cols,
                            Y = Y + (Height * row) / rows,
                            Width = Width / cols,
                            Height = Height / rows,
                        };

                        SIZE[i].Inflate(-22, -8);
                        SIZE[i].Offset(0, 4 + (-2 * row));
                    }
                    base.Init();
                }
            }

            private class IGMData_Commands : IGMData
            {
                public IGMData_Commands(): base (4,1, new IGMDataItem_Box(pos:new Rectangle(615,150,210,192),title: Icons.ID.COMMAND))
                {
                }

                /// <summary>
                /// Things that may of changed before screen loads or junction is changed.
                /// </summary>
                public override void ReInit()
                {
                    base.ReInit();

                    if (Memory.State.Characters != null)
                    {
                        ITEM[0,0]= new IGMDataItem_String(
                                Kernel_bin.BattleCommands[
                                    Memory.State.Characters[(int)Character].Abilities.Contains(Kernel_bin.Abilities.Mug)?
                                    13:
                                    1].Name,
                                SIZE[0]);
                        
                        for (int pos=1; pos < SIZE.Length; pos++)
                        {

                            ITEM[pos, 0] = Memory.State.Characters[(int)Character].Commands[pos - 1] != Kernel_bin.Abilities.None? new IGMDataItem_String(
                                Kernel_bin.Commandabilities[-0x14+(int)(Memory.State.Characters[(int)Character].Commands[pos - 1])].Name,
                                SIZE[pos]):null;
                        }
                    }
                }

                /// <summary>
                /// Things fixed at startup.
                /// </summary>
                protected override void Init()
                {
                    int rows = 4;
                    int cols = 1;
                    for (int i = 0; i < SIZE.Length; i++)
                    {
                        int col = i / rows;
                        int row = i % rows;
                        SIZE[i] = new Rectangle
                        {
                            X = X + (Width * col) / cols,
                            Y = Y + (Height * row) / rows,
                            Width = Width / cols,
                            Height = Height / rows,
                        };
                        SIZE[i].Inflate(-22, -8);
                        SIZE[i].Offset(0, 12 + (-8 * row));
                    }
                    base.Init();
                }
            }

            private class IGMData_Help : IGMData
            {
                public FF8String Data { get => ((IGMDataItem_Box)CONTAINER).Data; set => ((IGMDataItem_Box)CONTAINER).Data = value; }
                public IGMData_Help():base(0,0,new IGMDataItem_Box(Descriptions[Items.Junction], pos: new Rectangle(15,69,810,78),title:Icons.ID.HELP))
                {
                }
            }

            private class IGMData_TopMenu : IGMData
            {
                public IGMData_TopMenu():base(4,1,new IGMDataItem_Box(pos: new Rectangle(0,12,610,54)))
                {
                }


                protected override void Init()
                {
                    int rows = 1;
                    int cols = 4;
                    for (int i = 0; i < SIZE.Length; i++)
                    {
                        int col = i / rows;
                        int row = i % rows;
                        SIZE[i] = new Rectangle
                        {
                            X = X + (Width * col) / cols,
                            Y = Y + (Height * row) / rows,
                            Width = Width / cols,
                            Height = Height / rows,
                        };
                        SIZE[i].Inflate(-40, -12);
                        SIZE[i].Offset(20+ (-20 * (col>1?col:0)), 0);
                    }
                    ITEM[0, 0] = new IGMDataItem_String(Titles[Items.Junction], SIZE[0]);
                    ITEM[1, 0] = new IGMDataItem_String(Titles[Items.Off], SIZE[1]);
                    ITEM[2, 0] = new IGMDataItem_String(Titles[Items.Auto], SIZE[2]);
                    ITEM[3, 0] = new IGMDataItem_String(Titles[Items.Ability], SIZE[3]);
                    Cursor_Status |= Cursor_Status.Enabled;
                    Cursor_Status |= Cursor_Status.Horizontal;
                    Cursor_Status |= Cursor_Status.Vertical;
                    base.Init();
                }
                public override void Inputs_CANCEL()
                {
                    base.Inputs_CANCEL();
                    if (State == MainMenuStates.IGM_Junction)
                    {
                        State = MainMenuStates.InGameMenu;
                        Fade = 0.0f;
                    }
                }

                public override void Inputs_OKAY()
                {
                    switch(CURSOR_SELECT)
                    {
                        case 0:
                            InGameMenu_Junction.Data[SectionName.TopMenu_Junction].Enabled = true;
                            Cursor_Status |= Cursor_Status.Blinking;
                            InGameMenu_Junction.mode = Mode.TopMenu_Junction;
                            break;
                    }
                    base.Inputs_OKAY();
                }

                public override void Draw() => base.Draw();
            }

            private class IGMData_Title : IGMData
            {
                public IGMData_Title():base(0,0,new IGMDataItem_Box(Titles[Items.Junction], pos: new Rectangle(615,0,225,66)))
                {
                }

            }

            private class IGMData_TopMenu_Junction : IGMData
            {
                public override void Inputs_CANCEL()
                {
                    base.Inputs_CANCEL();
                    InGameMenu_Junction.mode = Mode.TopMenu;
                    Enabled = false;
                }

                public IGMData_TopMenu_Junction() : base(2, 1, new IGMDataItem_Box(pos: new Rectangle(210, 12, 400, 54)))
                {
                }
                public override void ReInit()
                {
                    Enabled = false;
                    base.ReInit();
                }

                public override void Draw() => base.Draw();

                protected override void Init()
                {
                    int rows = 1;
                    int cols = 2;
                    for (int i = 0; i < SIZE.Length; i++)
                    {
                        int col = i / rows;
                        int row = i % rows;
                        SIZE[i] = new Rectangle
                        {
                            X = X + (Width * col) / cols,
                            Y = Y + (Height * row) / rows,
                            Width = Width / cols,
                            Height = Height / rows,
                        };
                        SIZE[i].Inflate(-40, -12);
                        SIZE[i].Offset(20 + (-20 * (col > 1 ? col : 0)), 0);
                    }
                    ITEM[0, 0] = new IGMDataItem_String(Titles[Items.GF], SIZE[0]);
                    ITEM[1, 0] = new IGMDataItem_String(Titles[Items.Magic], SIZE[1]);
                    Cursor_Status |= Cursor_Status.Enabled;
                    Cursor_Status |= Cursor_Status.Horizontal;
                    Cursor_Status |= Cursor_Status.Vertical;
                    base.Init();
                }
            }
        }
    }
}