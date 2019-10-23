using Microsoft.Xna.Framework;
using OpenVIII.Encoding.Tags;
using System.Collections.Generic;

namespace OpenVIII.IGMData
{
    public class PartyAP : IGMData.Base
    {
        #region Fields

        private readonly FF8String DialogSelectedAbility;
        private readonly FF8String DialogSelectedGF;
        private readonly FF8String DialogSelectedIcon;
        private readonly FF8String DialogSelectedNum;
        private readonly FF8String str_GF_AP;
        private readonly FF8String str_Learn;
        private readonly FF8String str_Levelup;
        private Queue<KeyValuePair<GFs, Kernel_bin.Abilities>> _abilities;
        private uint _ap;

        #endregion Fields

        #region Constructors

        public PartyAP(Menu_Base container) : base()
        {
            DialogSelectedGF = new byte[] { (byte)FF8TextTagCode.Dialog, (byte)FF8TextTagDialog.SelectedGF };
            DialogSelectedNum = new byte[] { (byte)FF8TextTagCode.Dialog, (byte)FF8TextTagDialog.Number };
            DialogSelectedAbility = new byte[] { (byte)FF8TextTagCode.Dialog, (byte)FF8TextTagDialog.SelectedGFAbility };
            DialogSelectedIcon = new byte[] { (byte)FF8TextTagCode.Dialog, (byte)FF8TextTagDialog.CustomICON };
            str_Levelup =
                Memory.Strings.Read(Strings.FileID.KERNEL, 30, 121) +
                DialogSelectedGF + " " +
                Memory.Strings.Read(Strings.FileID.KERNEL, 30, 32);
            str_Learn =
                Memory.Strings.Read(Strings.FileID.KERNEL, 30, 121) +
                DialogSelectedGF + "\n  " +
                Memory.Strings.Read(Strings.FileID.KERNEL, 30, 120) + "\n     " +
                DialogSelectedIcon + " " +
                DialogSelectedAbility +
                Memory.Strings.Read(Strings.FileID.KERNEL, 30, 118);
            str_GF_AP = Memory.Strings.Read(Strings.FileID.KERNEL, 30, 109);
            Leveled = new Queue<GFs>();
            Init(1, 7, container, 1, 1);
        }

        #endregion Constructors

        #region Properties

        public Queue<KeyValuePair<GFs, Kernel_bin.Abilities>> Abilities { get => _abilities; set => _abilities = value; }

        public uint AP
        {
            get => _ap; set
            {
                _ap = value;
                ((IGMData.Dialog.Small)ITEM[0, 1]).Data = str_GF_AP.Clone().Replace(DialogSelectedNum, _ap.ToString());
                Leveled = Memory.State.EarnAP(_ap, out _abilities);
            }
        }

        public GFs GF { get; private set; }
        public Queue<GFs> Leveled { get; set; }

        #endregion Properties

        #region Methods

        public void Earn()
        {
            skipsnd = true;
            init_debugger_Audio.PlaySound(17);
        }

        public override bool Inputs_CANCEL() => false;

        public override bool Inputs_OKAY()
        {
            base.Inputs_OKAY();
            if (ITEM[0, 1].Enabled)
            {
                ITEM[0, 1].Hide();
                ITEM[0, 2].Show();
            }
            if (Leveled != null && Leveled.Count > 0)
            {
                Refresh();
                return true;
            }
            else
            {
                if (Abilities != null && Abilities.Count > 0)
                {
                    if (!ITEM[0, 3].Enabled)
                    {
                        ITEM[0, 2].Hide();
                        ITEM[0, 3].Show();
                    }
                    Refresh();
                    return true;
                }
            }
            return false;
        }

        public void Learn()
        {
            KeyValuePair<GFs, Kernel_bin.Abilities> Ability = Abilities.Dequeue();
            GF = Ability.Key;
            ((IGMData.Dialog.Small)ITEM[0, 3]).Data =
                str_Learn.Clone()
                .Replace(DialogSelectedGF, Memory.Strings.GetName(GF))
                .Replace(DialogSelectedIcon, DialogSelectedIcon.Clone() + new byte[] {
                            (byte)((short)Kernel_bin.AllAbilities[Ability.Value].Icon & 0xFF),
                            (byte)(((short)Kernel_bin.AllAbilities[Ability.Value].Icon & 0xFF00)>>8),
                            (Kernel_bin.AllAbilities[Ability.Value].Palette)
                })
                .Replace(DialogSelectedAbility, Kernel_bin.AllAbilities[Ability.Value].Name);
            skipsnd = true;
            init_debugger_Audio.PlaySound(0x28);
        }

        public void Level()
        {
            GF = Leveled.Dequeue();
            ((IGMData.Dialog.Small)ITEM[0, 2]).Data = str_Levelup.Clone().Replace(DialogSelectedGF, Memory.Strings.GetName(GF));
            skipsnd = true;
            init_debugger_Audio.PlaySound(0x28);
        }

        public override void Refresh()
        {
            base.Refresh();
            if (Enabled)
            {
                if (ITEM[0, 1].Enabled)
                    Earn();
                else if (ITEM[0, 2].Enabled && Leveled != null && Leveled.Count > 0)
                    Level();
                else if (ITEM[0, 3].Enabled && Abilities != null && Abilities.Count > 0)
                    Learn();
            }
        }

        protected override void Init()
        {
            base.Init();
            Hide();
            ITEM[0, 0] = new IGMDataItem.Box { Data = Memory.Strings.Read(Strings.FileID.KERNEL, 30, 111), Pos = new Rectangle(SIZE[0].X, SIZE[0].Y, SIZE[0].Width, 78), Title = Icons.ID.INFO, Options = Box_Options.Middle };
            ITEM[0, 1] = IGMData.Dialog.Small.Create(str_GF_AP, SIZE[0].X + 232, SIZE[0].Y + 315, Icons.ID.NOTICE, Box_Options.Center | Box_Options.Middle, SIZE[0]); // GF recieved ### AP!
            ITEM[0, 2] = IGMData.Dialog.Small.Create(null, SIZE[0].X + 232, SIZE[0].Y + 315, Icons.ID.NOTICE, Box_Options.Center | Box_Options.Middle, SIZE[0]); // GF Leveled up!
            ITEM[0, 3] = IGMData.Dialog.Small.Create(null, SIZE[0].X + 232, SIZE[0].Y + 315, Icons.ID.NOTICE, Box_Options.Center | Box_Options.Middle, SIZE[0]); // GF Leveled up!
            ITEM[0, 1].Show();
            ITEM[0, 2].Hide();
            ITEM[0, 3].Hide();
            Cursor_Status |= (Cursor_Status.Hidden | (Cursor_Status.Enabled | Cursor_Status.Static));
        }

        #endregion Methods
    }
}