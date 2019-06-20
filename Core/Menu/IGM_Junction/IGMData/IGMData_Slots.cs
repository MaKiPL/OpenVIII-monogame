using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class Module_main_menu_debug
    {
        #region Classes

        private partial class IGM_Junction
        {
            #region Classes

            public abstract class IGMData_Slots<C> : IGMData
                where C : class
            {
                #region Fields

                public static EventHandler<C> PrevSettingUpdateEventListener;
                protected bool eventAdded = false;
                protected List<Kernel_bin.Abilities> unlocked;
                private C _prevSetting;
                private Font.ColorID? colorid = null;

                #endregion Fields

                #region Constructors

                public IGMData_Slots(int count, int depth, IGMDataItem container = null, int? cols = null, int? rows = null) : base(count, depth, container, cols, rows) => Contents = new Kernel_bin.Stat[Count];

                #endregion Constructors

                #region Properties

                public Kernel_bin.Stat[] Contents { get; protected set; }

                #endregion Properties

                #region Methods

                public abstract void BackupSetting();

                public virtual void CheckMode(bool cursor = true) => CheckMode(0, Mode.None, Mode.None, false, false, cursor);

                public void CheckMode(int pos, Mode one, Mode two, bool slots, bool pools, bool cursor = true)
                {
                    if (InGameMenu_Junction != null && slots && Enabled)
                    {
                        Cursor_Status &= ~Cursor_Status.Horizontal;
                        Cursor_Status |= Cursor_Status.Vertical;
                        Cursor_Status &= ~Cursor_Status.Blinking;
                        if (CURSOR_SELECT > pos)
                            InGameMenu_Junction.SetMode(two);
                        else
                            InGameMenu_Junction.SetMode(one);
                    }
                    else if (InGameMenu_Junction != null && pools && Enabled)
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
                    bool ret = false;
                    if (CONTAINER.Pos.Contains(Input.MouseLocation.Transform(Menu.Focus)))
                    {
                        if (Input.Button(Buttons.MouseWheelup))
                        {
                            PageLeft();
                            ret = true;
                        }
                        else if (Input.Button(Buttons.MouseWheeldown))
                        {
                            PageRight();
                            ret = true;
                        }
                        if (ret)
                        {
                            Input.ResetInputLimit();
                            if (!skipsnd)
                                init_debugger_Audio.PlaySound(0);
                        }
                    }
                    if (!ret)
                        ret = base.Inputs();
                    if (ret) CheckMode();
                    return ret;
                }

                public override void ReInit()
                {
                    unlocked = Memory.State.Characters[Character].UnlockedGFAbilities;
                    AddEventListener();
                    CheckMode();
                    base.ReInit();
                }

                public abstract void UndoChange();

                /// <summary>
                /// Add event if not added already.
                /// </summary>
                protected virtual void AddEventListener()
                {
                    if (!eventAdded)
                    {
                        ModeChangeEventListener += ModeChangeEvent;
                        IGMData_Values.ColorChangeEventListener += ColorChangeEvent;
                        eventAdded = true;
                    }
                }

                private void ColorChangeEvent(object sender, Font.ColorID e)
                {
                    if (colorid != e)
                    {
                        colorid = e;
                        ReInit();
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

                protected void FillData(Icons.ID starticon, Kernel_bin.Stat statatk, Kernel_bin.Stat statdef)
                {
                    byte pos = 0;
                    Contents[0] = statatk;
                    getColor(pos, out byte palette, out Font.ColorID _colorid, out bool unlocked);
                    FF8String name = Kernel_bin.MagicData[Memory.State.Characters[Character].Stat_J[statatk]].Name;
                    if (name.Length == 0)
                        name = Misc[Items._];
                    ITEM[pos, 0] = new IGMDataItem_Icon(starticon, new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0), palette);
                    ITEM[pos, 1] = new IGMDataItem_String(name, new Rectangle(SIZE[pos].X + 60, SIZE[pos].Y, 0, 0), color: _colorid);
                    BLANKS[pos] = !unlocked;
                    for (pos = 1; pos < Count; pos++)
                    {
                        Contents[pos] = statdef + pos - 1;
                        getColor(pos, out palette, out _colorid, out unlocked);
                        name = Kernel_bin.MagicData[Memory.State.Characters[Character].Stat_J[statdef + pos - 1]].Name;
                        if (name.Length == 0)
                            name = Misc[Items._];
                        ITEM[pos, 0] = new IGMDataItem_Icon(starticon + 1, new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0), palette);
                        ITEM[pos, 1] = new IGMDataItem_String(name, new Rectangle(SIZE[pos].X + 60, SIZE[pos].Y, 0, 0), color: _colorid);
                        BLANKS[pos] = !unlocked;
                    }
                }

                /// <summary>
                /// please overload with a if statement to check for mode you want or else this will
                /// run the checkmode method everytime a mode change happens.
                /// </summary>
                /// <param name="sender"></param>
                /// <param name="e"></param>
                protected virtual void ModeChangeEvent(object sender, Mode e) =>
                    ReInit();

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

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}