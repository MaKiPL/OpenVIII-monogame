using Microsoft.Xna.Framework;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        #region Classes

        public class IGMDataItem_Icon : IGMDataItem//<Icons.ID>
        {
            private byte _pallet;
            private byte _faded_pallet;

            public Icons.ID Data { get; set; }

            public byte Pallet
            {
                get => _pallet; set
                {
                    if (value >= Memory.Icons.PalletCount) value = 2;
                    _pallet = value;
                }
            }

            public byte Faded_Pallet
            {
                get => _faded_pallet; set
                {
                    if (value >= Memory.Icons.PalletCount) value = 2;
                    _faded_pallet = value;
                }
            }

            public bool Blink => Faded_Pallet != Pallet;
            public float Blink_Adjustment { get; set; }

            public IGMDataItem_Icon(Icons.ID data, Rectangle? pos = null, byte? pallet = null, byte? faded_pallet = null, float blink_adjustment = 1f,Vector2? scale = null) : base(pos,scale)
            {
                Data = data;
                Pallet = pallet ?? 2;
                Faded_Pallet = faded_pallet ?? Pallet;
                Blink_Adjustment = blink_adjustment;
            }

            public override void Draw()
            {
                if (Enabled)
                {
                    Memory.Icons.Draw(Data, Pallet, Pos, Scale, fade);
                    if (Blink)
                        Memory.Icons.Draw(Data, Faded_Pallet, Pos, Scale, fade * blink_Amount * Blink_Adjustment);
                }
            }
        }
        #endregion Classes
    }
}