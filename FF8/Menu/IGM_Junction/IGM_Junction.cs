using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction : Menu
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
                Mag_Group,

                /// <summary>
                /// 4 Commands you can use in battle
                /// </summary>
                Commands,

                /// <summary>
                /// Portrait Name HP EXP Rank?
                /// </summary>
                CharacterInfo,

                TopMenu_Junction,
                TopMenu_Off,
                TopMenu_Auto,
                TopMenu_Abilities,
                RemMag,
                RemAll,
                TopMenu_GF_Group,
            }

            public static Dictionary<Items, FF8String> Titles { get; private set; }
            public static Dictionary<Items, FF8String> Misc { get; private set; }
            public static Dictionary<Items, FF8String> Descriptions { get; private set; }

            /// <summary>
            /// Character who has the junctions and inventory. Same as VisableCharacter unless TeamLaguna.
            /// </summary>
            public static Characters Character { get; private set; }

            /// <summary>
            /// Required to support Laguna's Party. They have unique stats but share junctions and inventory.
            /// </summary>
            public static Characters VisableCharacter { get; private set; }

            public override bool Update()
            {
                if (Enabled)
                {
                    bool ret = base.Update();
                    return Inputs() || ret;
                }
                return false;
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
                    {Items.Junction, Memory.Strings.Read(Strings.FileID.MNGRP,2,218) }
                };

                Data.Add(SectionName.CharacterInfo, new IGMData_CharacterInfo());
                Data.Add(SectionName.Commands, new IGMData_Commands());
                Data.Add(SectionName.Help, new IGMData_Help());
                Data.Add(SectionName.TopMenu, new IGMData_TopMenu());
                Data.Add(SectionName.Title, new IGMData_Container(
                    new IGMDataItem_Box(Titles[Items.Junction], pos: new Rectangle(615, 0, 225, 66))));
                Data.Add(SectionName.Mag_Group, new IGMData_Mag_Group(
                    new IGMData_Mag_Stat_Slots(),
                    new IGMData_Mag_PageTitle(),
                    new IGMData_Mag_Pool(),
                    new IGMData_Mag_EL_A_D_Slots(),
                    new IGMData_Mag_EL_A_Values(),
                    new IGMData_Mag_EL_D_Values(),
                    new IGMData_Mag_ST_A_D_Slots(),
                    new IGMData_Mag_ST_A_Values(),
                    new IGMData_Mag_ST_D_Values()
                    ));
                Data.Add(SectionName.TopMenu_Junction, new IGMData_TopMenu_Junction());
                Data.Add(SectionName.TopMenu_Off, new IGMData_TopMenu_Off_Group(
                    new IGMData_Container(
                        new IGMDataItem_Box(Titles[Items.Off], pos: new Rectangle(0, 12, 169, 54), options: Box_Options.Center | Box_Options.Middle)),
                    new IGMData_TopMenu_Off()
                    ));
                Data.Add(SectionName.TopMenu_Auto, new IGMData_TopMenu_Auto_Group(
                    new IGMData_Container(
                        new IGMDataItem_Box(Titles[Items.Auto], pos: new Rectangle(0, 12, 169, 54), options: Box_Options.Center | Box_Options.Middle)),
                    new IGMData_TopMenu_Auto()));
                Data.Add(SectionName.TopMenu_Abilities, new IGMData_Abilities_Group(
                    new IGMData_Abilities_Command(),
                    new IGMData_Abilities_AbilitySlots(),
                    new IGMData_Abilities_CommandPool(),
                    new IGMData_Abilities_AbilityPool()
                    ));
                FF8String Yes = Memory.Strings.Read(Strings.FileID.MNGRP, 0, 57);
                FF8String No = Memory.Strings.Read(Strings.FileID.MNGRP, 0, 58);
                Data.Add(SectionName.RemMag, new IGMData_ConfirmRemMag(data: Memory.Strings.Read(Strings.FileID.MNGRP, 2, 280), title: Icons.ID.NOTICE, opt1: Yes, opt2: No, pos: new Rectangle(180, 174, 477, 216)));
                Data.Add(SectionName.RemAll, new IGMData_ConfirmRemAll(data: Memory.Strings.Read(Strings.FileID.MNGRP, 2, 279), title: Icons.ID.NOTICE, opt1: Yes, opt2: No, pos: new Rectangle(170, 174, 583, 216)));
                Data.Add(SectionName.TopMenu_GF_Group, new IGMData_GF_Group(
                    new IGMData_GF_Junctioned(),
                    new IGMData_GF_Pool(),
                    new IGMData_Container(new IGMDataItem_Box(pos: new Rectangle(440, 345, 385, 66)))
                    ));

                base.Init();
            }

            public void ReInit(Characters c, Characters vc)
            {
                Character = c;
                VisableCharacter = vc;
                ReInit();
            }

            public new enum Mode
            {
                TopMenu,
                TopMenu_Junction,
                TopMenu_Off,
                TopMenu_Auto,
                Abilities,
                Abilities_Commands,
                Abilities_Abilities,
                RemMag,
                RemAll,
                TopMenu_GF_Group,
                Mag_Pool_Stat,
                Mag_Pool_EL_A,
                Mag_Pool_EL_D,
                Mag_Pool_ST_A,
                Mag_Pool_ST_D,
                Mag_Stat,
                Mag_EL_A_D,
                Mag_ST_A_D
            }

            public new Mode mode;

            protected override bool Inputs()
            {
                bool ret = false;
                if (Enabled)
                {
                    switch (mode)
                    {
                        case Mode.TopMenu:
                            ret = ((IGMData_TopMenu)Data[SectionName.TopMenu]).Inputs();
                            break;

                        case Mode.TopMenu_Junction:
                            ret = ((IGMData_TopMenu_Junction)Data[SectionName.TopMenu_Junction]).Inputs();
                            break;

                        case Mode.TopMenu_Off:
                            ret = ((IGMData_TopMenu_Off_Group)Data[SectionName.TopMenu_Off]).Inputs();
                            break;

                        case Mode.TopMenu_Auto:
                            ret = ((IGMData_TopMenu_Auto_Group)Data[SectionName.TopMenu_Auto]).Inputs();
                            break;

                        case Mode.Abilities:
                            ret = ((IGMData_Abilities_Group)Data[SectionName.TopMenu_Abilities]).Inputs();
                            break;

                        case Mode.Abilities_Commands:
                            ret = ((IGMData_Abilities_Group)Data[SectionName.TopMenu_Abilities]).ITEM[2, 0].Inputs();
                            break;

                        case Mode.Abilities_Abilities:
                            ret = ((IGMData_Abilities_Group)Data[SectionName.TopMenu_Abilities]).ITEM[3, 0].Inputs();
                            break;

                        case Mode.RemMag:
                            ret = ((IGMData_ConfirmDialog)Data[SectionName.RemMag]).Inputs();
                            break;

                        case Mode.RemAll:
                            ret = ((IGMData_ConfirmDialog)Data[SectionName.RemAll]).Inputs();
                            break;

                        case Mode.TopMenu_GF_Group:
                            ret = ((IGMData_GF_Group)Data[SectionName.TopMenu_GF_Group]).ITEM[1, 0].Inputs();
                            break;

                        case Mode.Mag_Pool_Stat:
                        case Mode.Mag_Pool_EL_A:
                        case Mode.Mag_Pool_EL_D:
                        case Mode.Mag_Pool_ST_A:
                        case Mode.Mag_Pool_ST_D:
                            ret = ((IGMData_Mag_Group)Data[SectionName.Mag_Group]).ITEM[2, 0].Inputs();
                            break;

                        case Mode.Mag_Stat:
                            ret = ((IGMData_Mag_Group)Data[SectionName.Mag_Group]).ITEM[0, 0].Inputs();
                            break;

                        case Mode.Mag_EL_A_D:
                            ret = ((IGMData_Mag_Group)Data[SectionName.Mag_Group]).ITEM[3, 0].Inputs();
                            break;

                        case Mode.Mag_ST_A_D:
                            ret = ((IGMData_Mag_Group)Data[SectionName.Mag_Group]).ITEM[6, 0].Inputs();
                            break;

                        default:
                            break;
                    }
                }
                return ret;
            }

            public override void Draw()
            {
                if (Enabled)
                {
                    StartDraw();
                    base.Draw();
                    EndDraw();
                }
            }
        }
    }
}