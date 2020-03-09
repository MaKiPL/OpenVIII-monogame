using Microsoft.Xna.Framework;
using System;

namespace OpenVIII
{
    public partial class IGM_Junction : Menu
    {
        #region Enums

        public enum Mode
        {
            None,
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
            Mag_EL_A,
            Mag_ST_A,
            Mag_EL_D,
            Mag_ST_D,
            ConfirmChanges
        }

        public enum SectionName : byte
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
            /// Portrait Name HP EXP Rank?
            /// </summary>
            CharacterInfo,

            /// <summary>
            /// Description Help
            /// </summary>
            Help,

            /// <summary>
            /// 4 Commands you can use in battle
            /// </summary>
            Commands,

            /// <summary>
            /// Character Stats Magic Junctions
            /// </summary>
            Mag_Group,

            /// <summary>
            /// Top menu where you select junction GF or Magic
            /// </summary>
            TopMenu_Junction,

            /// <summary>
            /// Top Menu where you select unjunction all or magic
            /// </summary>
            TopMenu_Off,

            /// <summary>
            /// Top Menu where you select automaticly sort by ATK DEF or MAG
            /// </summary>
            TopMenu_Auto,

            /// <summary>
            /// Junction commands/abilities
            /// </summary>
            TopMenu_Abilities,

            /// <summary>
            /// Remove all Magic?
            /// </summary>
            RemMag,

            /// <summary>
            /// Remove all Junctions?
            /// </summary>
            RemAll,

            /// <summary>
            /// GF junction screen
            /// </summary>
            TopMenu_GF_Group,

            /// <summary>
            /// Confirm changes screen
            /// </summary>
            ConfirmChanges,
        }

        #endregion Enums

        #region Methods

        public static IGM_Junction Create() => Create<IGM_Junction>();

        public void ChangeHelp(FF8String str) => ((IGMDataItem.HelpBox)Data[SectionName.Help]).Data = str;

        //public static Dictionary<Items, FF8String> Descriptions { get; private set; }
        //public static Dictionary<Items, FF8String> Misc { get; private set; }
        //public static Dictionary<Items, FF8String> Titles { get; private set; }
        public override bool Inputs()
        {
            if (GetMode().Equals(Mode.None)) SetMode(Mode.TopMenu);
            bool ret = false;
            if (Enabled)
            {
                switch (GetMode())
                {
                    case Mode.TopMenu:
                        ret = ((IGMData_TopMenu)Data[SectionName.TopMenu]).Inputs();
                        break;

                    case Mode.TopMenu_Junction:
                        ret = ((IGMData_TopMenu_Junction)Data[SectionName.TopMenu_Junction]).Inputs();
                        break;

                    case Mode.TopMenu_Off:
                        ret = ((IGMData.Group.TopMenu)Data[SectionName.TopMenu_Off]).Inputs();
                        break;

                    case Mode.TopMenu_Auto:
                        ret = ((IGMData.Group.TopMenu)Data[SectionName.TopMenu_Auto]).Inputs();
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
                        ret = ((IGMData.Dialog.Confirm)Data[SectionName.RemMag]).Inputs();
                        break;

                    case Mode.RemAll:
                        ret = ((IGMData.Dialog.Confirm)Data[SectionName.RemAll]).Inputs();
                        break;

                    case Mode.ConfirmChanges:
                        ret = ((IGMData.Dialog.Confirm)Data[SectionName.ConfirmChanges]).Inputs();
                        break;

                    case Mode.TopMenu_GF_Group:
                        ret = ((IGMData_GF_Group)Data[SectionName.TopMenu_GF_Group]).ITEM[1, 0].Inputs();
                        break;

                    case Mode.Mag_Pool_Stat:
                    case Mode.Mag_Pool_EL_A:
                    case Mode.Mag_Pool_EL_D:
                    case Mode.Mag_Pool_ST_A:
                    case Mode.Mag_Pool_ST_D:
                    case Mode.Mag_Stat:
                    case Mode.Mag_EL_A:
                    case Mode.Mag_EL_D:
                    case Mode.Mag_ST_A:
                    case Mode.Mag_ST_D:
                        ret = ((IGMData_Mag_Group)Data[SectionName.Mag_Group]).Inputs();
                        break;

                    default:
                        break;
                }
            }
            return ret;
        }

        protected override void Init()
        {
            SetMode((Mode)0);
            Size = new Vector2 { X = 840, Y = 630 };

            Menu_Base[] tmp = new Menu_Base[9];
            Action[] actions = new Action[]
            {
                () =>tmp[0] = IGMData_Mag_Stat_Slots.Create(),
                () =>tmp[1] = IGMData_Mag_PageTitle.Create(),
                () =>tmp[3] = IGMData_Mag_EL_A_D_Slots.Create(),
                () =>tmp[2] = IGMData.Pool.Magic.Create(),
                () =>tmp[4] = IGMData_Mag_EL_A_Values.Create(),
                () =>tmp[5] = IGMData_Mag_EL_D_Values.Create(),
                () =>tmp[6] = IGMData_Mag_ST_A_D_Slots.Create(),
                () =>tmp[7] = IGMData_Mag_ST_A_Values.Create(),
                () =>tmp[8] = IGMData_Mag_ST_D_Values.Create(),
                () => Data.TryAdd(SectionName.CharacterInfo, IGMData_CharacterInfo.Create()),
                () => Data.TryAdd(SectionName.Commands, IGMData.Commands.Create(new Rectangle(615, 150, 210, 192))),
                () => Data.TryAdd(SectionName.Help, new IGMDataItem.HelpBox { Data = Strings.Description.Junction, Pos = new Rectangle(15, 69, 810, 78), Title = Icons.ID.HELP }),
                () => Data.TryAdd(SectionName.TopMenu, IGMData_TopMenu.Create()),
                () => Data.TryAdd(SectionName.Title, new IGMDataItem.Box { Data = Strings.Name.Junction, Pos = new Rectangle(615, 0, 225, 66) }),
                () => Data.TryAdd(SectionName.TopMenu_Junction, IGMData_TopMenu_Junction.Create()),
                () => Data.TryAdd(SectionName.TopMenu_Off, IGMData.Group.TopMenu.Create(
                    new IGMDataItem.Box { Data = Strings.Name.Off, Pos = new Rectangle(0, 12, 169, 54), Options = Box_Options.Center | Box_Options.Middle },
                    IGMData_TopMenu_Off.Create()
                    )),
                () => Data.TryAdd(SectionName.TopMenu_Auto, IGMData.Group.TopMenu.Create(
                    new IGMDataItem.Box { Data = Strings.Name.Auto, Pos = new Rectangle(0, 12, 169, 54), Options = Box_Options.Center | Box_Options.Middle },
                    IGMData_TopMenu_Auto.Create())),
                () => Data.TryAdd(SectionName.TopMenu_Abilities, IGMData_Abilities_Group.Create(
                IGMData.Slots.Command.Create(),
                IGMData.Slots.Abilities.Create(),
                IGMData_Abilities_CommandPool.Create(),
                IGMData_Abilities_AbilityPool.Create())),
                () => Data.TryAdd(SectionName.TopMenu_GF_Group, IGMData_GF_Group.Create(
                    IGMData_GF_Junctioned.Create(),
                    IGMData.Pool.GF.Create(),
                    new IGMDataItem.Box { Pos = new Rectangle(440, 345, 385, 66) }
                    )),
                () => Data.TryAdd(SectionName.RemMag, IGMData_ConfirmRemMag.Create(data: Memory.Strings.Read(Strings.FileID.MenuGroup, 2, 280), title: Icons.ID.NOTICE, opt1: Strings.Name.Yes, opt2: Strings.Name.No, pos: new Rectangle(180, 174, 477, 216))),
                () => Data.TryAdd(SectionName.RemAll, IGMData_ConfirmRemAll.Create(data: Memory.Strings.Read(Strings.FileID.MenuGroup, 2, 279), title: Icons.ID.NOTICE, opt1: Strings.Name.Yes, opt2: Strings.Name.No, pos: new Rectangle(170, 174, 583, 216))),
                () => Data.TryAdd(SectionName.ConfirmChanges, IGMData_ConfirmChanges.Create(data: Memory.Strings.Read(Strings.FileID.MenuGroup, 0, 73), title: Icons.ID.NOTICE, opt1: Strings.Name.Yes, opt2: Memory.Strings.Read(Strings.FileID.MenuGroup, 2, 268), pos: new Rectangle(280, 174, 367, 216))),
            };
            Memory.ProcessActions(actions);

            Data.TryAdd(SectionName.Mag_Group, IGMData_Mag_Group.Create(tmp));
            base.Init();
        }

        #endregion Methods
    }
}