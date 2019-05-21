using Microsoft.Xna.Framework;
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
                NextLEVEL
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
            }

            public static Dictionary<Items, FF8String> Titles { get; private set; }
            public static Dictionary<Items, FF8String> Descriptions { get; private set; }
            public static Saves.Characters Character { get; private set; }

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
                        { Items.CurrentEXP, Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,23)  },
                        { Items.NextLEVEL, Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,24)  },
                };

                //{Items.ST_D,Memory.Strings.Read(Strings.FileID.MNGRP,2,259)},
                //{Items.EL_A,Memory.Strings.Read(Strings.FileID.MNGRP,2,260)},
                //{Items.EL_D,Memory.Strings.Read(Strings.FileID.MNGRP,2,261)},
                //{Items._,Memory.Strings.Read(Strings.FileID.MNGRP,2,266)},
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
                base.Init();

            }

            public void ReInit(Saves.Characters c)
            {
                Character = c;
                ReInit();
            }

            protected override bool Inputs() => false;


            public override void Draw()
            {
                StartDraw();
                //switch (mode)
                //{
                //    case Mode.ChooseChar:
                ////    case Mode.ChooseItem:
                //    default:
                        base.Draw();
                        //break;
                //}
                //switch (mode)
                //{
                //    case Mode.ChooseChar:
                //        DrawPointer(Data[SectionName.SideMenu].CURSOR[(int)choSideBar], blink: true);

                //        if (choChar < Data[SectionName.Party].Count && choChar >= 0)
                //            DrawPointer(Data[SectionName.Party].CURSOR[choChar]);
                //        else if (choChar < Data[SectionName.Non_Party].Count + Data[SectionName.Party].Count && choChar >= Data[SectionName.Party].Count)
                //            DrawPointer(Data[SectionName.Non_Party].CURSOR[choChar - Data[SectionName.Party].Count]);
                //        break;

                //    default:
                //        DrawPointer(Data[SectionName.SideMenu].CURSOR[(int)choSideBar]);
                //        break;
                //}
                EndDraw();
            }

            private class IGMData_CharacterInfo : IGMData
            {
                public IGMData_CharacterInfo(): base(1,15,new IGMDataItem_Empty(new Rectangle(20,153,395,255)))
                {
                }
                public override bool Update()
                {
                    base.Update();
                    ITEM[0, 1] = new IGMDataItem_Face((Faces.ID)Character, new Rectangle(X + 20, Y, 80, 144));
                    ITEM[0, 2] = new IGMDataItem_String(Memory.Strings.GetName(Character), new Rectangle(X + 117, Y + 0, 0, 0));
                    ITEM[0, 3] = new IGMDataItem_Icon(Icons.ID.Lv, new Rectangle(X + 117, Y + 54, 0, 0), 13);

                    if (Memory.State.Characters != null)
                        ITEM[0, 4] = new IGMDataItem_Int(Memory.State.Characters[(int)Character].Level, new Rectangle(X + 117 + 35, Y + 54, 0, 0), 13,numtype:Icons.NumType.Num_8x8_2, padding: 1, spaces: 6);
                    if (Memory.State.Party != null && Memory.State.Party.Contains(Character))
                        ITEM[0, 5] = new IGMDataItem_Icon(Icons.ID.InParty, new Rectangle(X + 278, Y + 48, 0, 0), 6);
                    else
                        ITEM[0, 5] = null;
                    ITEM[0, 6] = new IGMDataItem_Icon(Icons.ID.HP2, new Rectangle(X + 117, Y + 108, 0, 0), 13);
                    if (Memory.State.Characters != null)
                        ITEM[0, 7] = new IGMDataItem_Int(Memory.State.Characters[(int)Character].CurrentHP, new Rectangle(X + 117, Y + 108, 0, 0), 13, numtype: Icons.NumType.Num_8x8_2, padding: 1, spaces: 6);
                    ITEM[0, 8] = new IGMDataItem_Icon(Icons.ID.Size_08x08_Forward_Slash, new Rectangle(X + 153, Y + 108, 0, 0), 13);
                    if (Memory.State.Characters != null)
                        ITEM[0, 9] = new IGMDataItem_Int(Memory.State.Characters[(int)Character].MaxHP(), new Rectangle(X + 153+20, Y + 108, 0, 0), 13, numtype: Icons.NumType.Num_8x8_2, padding: 1, spaces: 5);
                    ITEM[0, 10] = new IGMDataItem_String(Titles[Items.CurrentEXP] + new FF8String("\n") + Titles[Items.NextLEVEL], new Rectangle(X, Y + 153, 0, 0));
                    if (Memory.State.Characters != null)
                        ITEM[0, 11] = new IGMDataItem_Int((int)Memory.State.Characters[(int)Character].Experience, new Rectangle(X + 192, Y + 198, 0, 0), 13, numtype: Icons.NumType.Num_8x8_2, padding: 1, spaces: 9);
                    ITEM[0, 12] = new IGMDataItem_Icon(Icons.ID.P, new Rectangle(X + 372, Y + 198, 0, 0), 13);
                    if (Memory.State.Characters != null)
                        ITEM[0, 13] = new IGMDataItem_Int(Memory.State.Characters[(int)Character].ExperienceToNextLevel, new Rectangle(X + 192, Y + 231, 0, 0), 13, numtype: Icons.NumType.Num_8x8_2, padding: 1, spaces: 9);
                    ITEM[0, 14] = new IGMDataItem_Icon(Icons.ID.P, new Rectangle(X + 372, Y + 231, 0, 0), 13);
                    return false;
                }

                public override void ReInit()
                {
                    base.ReInit();

                    
                }
            }

            private class IGMData_Stats : IGMData
            {
                public IGMData_Stats() : base (10,4,new IGMDataItem_Empty(new Rectangle(0,414,840,216)))
                {
                }

                public override void ReInit()
                {
                    base.ReInit();
                    //ITEM[0, 1] = new IGMDataItem_Face((Faces.ID)Character, new Rectangle(X + 20, Y, 80, 144));
                    //ITEM[0, 2] = new IGMDataItem_String(Memory.Strings.GetName(Character), new Rectangle(X + 117, Y + 0, 0, 0));
                    //ITEM[0, 3] = new IGMDataItem_Icon(Icons.ID.Lv, new Rectangle(X + 117, Y + 54, 0, 0), 13); if (Memory.State.Characters != null)
                    //    ITEM[0, 4] = new IGMDataItem_Int(Memory.State.Characters[(int)Character].Level, new Rectangle(X + 117, Y + 54, 0, 0), 13, padding: 1, spaces: 6);
                    //if (Memory.State.Party.Contains(Character))
                    //    ITEM[0, 5] = new IGMDataItem_Icon(Icons.ID.InParty, new Rectangle(X + 278, Y + 48, 0, 0), 6);
                    //else
                    //    ITEM[0, 5] = null;
                    //ITEM[0, 6] = new IGMDataItem_Icon(Icons.ID.HP2, new Rectangle(X + 117, Y + 108, 0, 0), 13);
                    //ITEM[0, 7] = new IGMDataItem_Int(Memory.State.Characters[(int)Character].CurrentHP, new Rectangle(X + 117, Y + 108, 0, 0), 13, padding: 1, spaces: 6);
                    //ITEM[0, 8] = new IGMDataItem_Icon(Icons.ID.Size_08x08_Forward_Slash, new Rectangle(X + 153, Y + 108, 0, 0), 13);
                    //ITEM[0, 9] = new IGMDataItem_Int(Memory.State.Characters[(int)Character].MaxHP(), new Rectangle(X + 117, Y + 108, 0, 0), 13, padding: 1, spaces: 5);
                    //ITEM[0, 10] = new IGMDataItem_String(Titles[Items.CurrentEXP] + new FF8String("\n") + Titles[Items.NextLEVEL], new Rectangle(X, Y + 153, 0, 0));
                    //ITEM[0, 11] = new IGMDataItem_Int((int)Memory.State.Characters[(int)Character].Experience, new Rectangle(X + 192, Y + 198, 0, 0), 13, padding: 1, spaces: 9);
                    //ITEM[0, 12] = new IGMDataItem_Icon(Icons.ID.P, new Rectangle(X + 372, Y + 198, 0, 0), 13);
                    //ITEM[0, 13] = new IGMDataItem_Int(Memory.State.Characters[(int)Character].ExperienceToNextLevel, new Rectangle(X + 192, Y + 231, 0, 0), 13, padding: 1, spaces: 9);
                    //ITEM[0, 14] = new IGMDataItem_Icon(Icons.ID.P, new Rectangle(X + 372, Y + 231, 0, 0), 13);
                }
            }
        }
    }
}