using Microsoft.Xna.Framework;
using OpenVIII.Encoding.Tags;
using System.Collections.Concurrent;
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
        private ConcurrentQueue<KeyValuePair<GFs, Kernel.Abilities>> _abilities;
        private uint _ap;
        private GFs _gf;

        #endregion Fields

        #region Constructors

        private PartyAP()
        {
            DialogSelectedGF = new byte[] { (byte)FF8TextTagCode.Dialog, (byte)FF8TextTagDialog.SelectedGF };
            DialogSelectedNum = new byte[] { (byte)FF8TextTagCode.Dialog, (byte)FF8TextTagDialog.Number };
            DialogSelectedAbility = new byte[] { (byte)FF8TextTagCode.Dialog, (byte)FF8TextTagDialog.SelectedGFAbility };
            DialogSelectedIcon = new byte[] { (byte)FF8TextTagCode.Dialog, (byte)FF8TextTagDialog.CustomICON };
            str_Levelup =
                Strings.Name.GF2 +
                DialogSelectedGF + " " +
                Strings.Name.LevelUP_;
            str_Learn =
                Strings.Name.GF2 +
                DialogSelectedGF + "\n  " +
                Strings.Name.Learned + "\n     " +
                DialogSelectedIcon + " " +
                DialogSelectedAbility +
                Strings.Name.ExclamationPoint;
            str_GF_AP = Strings.Name.GF_Received_X_AP_;
            Leveled = new ConcurrentQueue<GFs>();
        }

        #endregion Constructors

        #region Properties

        public ConcurrentQueue<KeyValuePair<GFs, Kernel.Abilities>> Abilities { get => _abilities; set => _abilities = value; }

        public uint AP
        {
            get => _ap; set
            {
                _ap = value;
                ((IGMData.Dialog.Small)ITEM[0, 1]).Data = str_GF_AP.Clone().Replace(DialogSelectedNum, _ap.ToString());
                Leveled = Memory.State.EarnAP(_ap, out _abilities);
            }
        }

        public GFs GF { get => _gf; private set => _gf = value; }

        public ConcurrentQueue<GFs> Leveled { get; set; }

        #endregion Properties

        #region Methods

        public static PartyAP Create(Rectangle pos)
        {
            PartyAP r = new PartyAP();
            r.Init(1, 7, new IGMDataItem.Empty { Pos = pos }, 1, 1);
            return r;
        }

        public void Earn()
        {
            skipsnd = true;
            AV.Sound.Play(17);
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
                ITEM[0, 1].Show();
                ITEM[0, 2].Hide();
                ITEM[0, 3].Hide();
            }
            return false;
        }

        public void Learn()
        {
            if (Abilities.TryDequeue(out KeyValuePair<GFs, Kernel.Abilities> Ability))
            {
                GF = Ability.Key;
                ((IGMData.Dialog.Small)ITEM[0, 3]).Data =
                    str_Learn.Clone()
                    .Replace(DialogSelectedGF, Memory.Strings.GetName(GF))
                    .Replace(DialogSelectedIcon, DialogSelectedIcon.Clone() + new byte[] {
                            (byte)((short)Memory.Kernel_Bin.AllAbilities[Ability.Value].Icon & 0xFF),
                            (byte)(((short)Memory.Kernel_Bin.AllAbilities[Ability.Value].Icon & 0xFF00)>>8),
                            (Memory.Kernel_Bin.AllAbilities[Ability.Value].Palette)
                    })
                    .Replace(DialogSelectedAbility, Memory.Kernel_Bin.AllAbilities[Ability.Value].Name);
                skipsnd = true;
                AV.Sound.Play(0x28);
            }
        }

        public void Level()
        {
            if (Leveled.TryDequeue(out _gf))
            {
                ((IGMData.Dialog.Small)ITEM[0, 2]).Data = str_Levelup.Clone().Replace(DialogSelectedGF, Memory.Strings.GetName(GF));
                skipsnd = true;
                AV.Sound.Play(0x28);
            }
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
            ITEM[0, 0] = new IGMDataItem.Box { Data = Strings.Name.Raising_GF, Pos = new Rectangle(SIZE[0].X, SIZE[0].Y, SIZE[0].Width, 78), Title = Icons.ID.INFO, Options = Box_Options.Middle };
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