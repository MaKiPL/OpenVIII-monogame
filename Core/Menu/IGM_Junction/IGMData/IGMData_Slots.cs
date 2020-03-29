using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        public abstract class IGMData_Slots<C> : IGMData.Base
                where C : class
        {
            #region Fields

            protected bool eventAdded = false;

            protected List<Kernel.Abilities> unlocked;

            private C _prevSetting;

            private Font.ColorID? colorid = null;

            #endregion Fields

            #region Events

            public static event EventHandler<C> PrevSettingUpdateEventListener;

            #endregion Events

            #region Properties

            public Kernel.Stat[] Contents { get; protected set; }

            #endregion Properties

            #region Methods

            public static T Create<T>(int count, int depth, Menu_Base container = null, int? cols = null, int? rows = null) where T : IGMData_Slots<C>, new()
            {
                var r = IGMData.Base.Create<T>(count, depth, container, cols, rows);
                r.Contents = new Kernel.Stat[r.Count];
                return r;
            }

            public abstract void BackupSetting();

            public virtual void CheckMode(bool cursor = true) => CheckMode(0, Mode.None, Mode.None, false, false, cursor);

            public void CheckMode(int pos, Mode one, Mode two, bool slots, bool pools, bool cursor = true)
            {
                if (IGM_Junction != null && slots && Enabled)
                {
                    Cursor_Status &= ~Cursor_Status.Horizontal;
                    Cursor_Status |= Cursor_Status.Vertical;
                    Cursor_Status &= ~Cursor_Status.Blinking;
                    if (CURSOR_SELECT > pos)
                        IGM_Junction.SetMode(two);
                    else
                        IGM_Junction.SetMode(one);
                }
                else if (IGM_Junction != null && pools && Enabled)
                {
                    Cursor_Status |= Cursor_Status.Blinking;
                }
                if (cursor)
                    Cursor_Status |= Cursor_Status.Enabled;
                else
                    Cursor_Status &= ~Cursor_Status.Enabled;
            }

            public virtual void ConfirmChange() => SetPrevSetting(default);

            public C GetPrevSetting() => _prevSetting;

            public override bool Inputs()
            {
                var ret = false;
                if (CONTAINER.Pos.Contains(MouseLocation))
                {
                    if (Input2.DelayedButton(MouseButtons.MouseWheelup))
                    {
                        PageLeft();
                        ret = true;
                    }
                    else if (Input2.DelayedButton(MouseButtons.MouseWheeldown))
                    {
                        PageRight();
                        ret = true;
                    }
                    if (ret)
                    {
                        if (!skipsnd)
                            AV.Sound.Play(0);
                    }
                }
                if (!ret)
                    ret = base.Inputs();
                if (ret) CheckMode();
                return ret;
            }

            /// <summary>
            /// please overload with a if statement to check for mode you want or else this will run
            /// the checkmode method everytime a mode change happens.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public override void ModeChangeEvent(object sender, Enum e) => Refresh();

            public override void Refresh()
            {
                if (Damageable != null && Damageable.GetCharacterData(out var c))
                {
                    unlocked = c.UnlockedGFAbilities;
                    AddEventListener();
                    CheckMode();
                    base.Refresh();
                }
            }

            public abstract void UndoChange();

            /// <summary>
            /// Add event if not added already.
            /// </summary>
            protected virtual void AddEventListener()
            {
                if (!eventAdded)
                {
                    IGM_Junction.ModeChangeHandler += ModeChangeEvent;
                    IGMData_Values.ColorChangeEventListener += ColorChangeEvent;
                    eventAdded = true;
                }
            }

            protected void FillData(Icons.ID starticon, Kernel.Stat statatk, Kernel.Stat statdef)
            {
                if (Damageable.GetCharacterData(out var c))
                    for (byte pos = 0; pos < Count; pos++)
                    {
                        var stat = pos != 0 ? statdef + pos - 1 : statatk;
                        Contents[pos] = stat;
                        getColor(pos, out var palette, out var _colorid, out var unlocked);
                        var name = GetName(stat);
                        UpdateItems();

                        FF8String GetName(Kernel.Stat key)
                        {
                            var _name = Memory.KernelBin.MagicData[c.StatJ[key]].Name;
                            if (_name == null || _name.Length == 0)
                                _name = Strings.Name._;
                            return _name;
                        }
                        void UpdateItems()
                        {
                            ((IGMDataItem.Icon)ITEM[pos, 0]).Data = starticon + 1;
                            ((IGMDataItem.Icon)ITEM[pos, 0]).Palette = palette;
                            ((IGMDataItem.Text)ITEM[pos, 1]).Data = name;
                            ((IGMDataItem.Text)ITEM[pos, 1]).FontColor = _colorid;
                            BLANKS[pos] = !unlocked;
                        }
                    }
            }

            protected override void Init()
            {
                base.Init();
                for (byte pos = 0; pos < Count; pos++)
                {
                    ITEM[pos, 0] = new IGMDataItem.Icon { Pos = new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0) };
                    ITEM[pos, 1] = new IGMDataItem.Text { Pos = new Rectangle(SIZE[pos].X + 60, SIZE[pos].Y, 0, 0) };
                    BLANKS[pos] = true;
                }
            }

            protected abstract void PageLeft();

            protected abstract void PageRight();

            protected void SetPrevSetting(C value)
            {
                if (!EqualityComparer<C>.Default.Equals(_prevSetting, value))
                {
                    _prevSetting = value;
                    PrevSettingUpdateEventListener?.Invoke(this, _prevSetting);
                    if (value == null)
                        colorid = null;
                }
            }

            /// <summary>
            /// overload me and return true if type at pos is unlocked or not.
            /// </summary>
            /// <param name="pos">current position</param>
            /// <returns></returns>
            protected virtual bool Unlocked(byte pos) => true;

            private void ColorChangeEvent(object sender, Font.ColorID e)
            {
                if (colorid != e)
                {
                    colorid = e;
                    Refresh();
                }
            }

            private void getColor(byte pos, out byte palette, out Font.ColorID _colorid, out bool unlocked)
            {
                unlocked = Unlocked(pos);
                palette = 2;
                _colorid = Font.ColorID.White;
                if (unlocked)
                {
                    if (colorid != null && CURSOR_SELECT == pos && _prevSetting != null)
                    {
                        if (colorid == Font.ColorID.Red)
                        {
                            palette = 5;
                            _colorid = Font.ColorID.Red;
                        }
                        else if (colorid == Font.ColorID.Yellow)
                        {
                            palette = 6;
                            _colorid = Font.ColorID.Yellow;
                        }
                    }
                }
                else
                {
                    palette = 7;
                    _colorid = Font.ColorID.Grey;
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}