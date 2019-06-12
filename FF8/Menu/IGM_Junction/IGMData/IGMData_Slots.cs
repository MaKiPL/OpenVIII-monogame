namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            public abstract class IGMData_Slots<T, T2> : IGMData
            {
                public T[] Contents { get; protected set; }

                public IGMData_Slots(int count, int depth, IGMDataItem container = null, int? cols = null, int? rows = null) : base(count, depth, container, cols, rows) => Contents = new T[Count];

                protected bool eventAdded = false;
                private T2 _prevSetting;

                /// <summary>
                /// Add event if not added already.
                /// </summary>
                protected virtual void AddEventListener()
                {
                    if (!eventAdded)
                    {
                        ModeChangeEventListener += ModeChangeEvent;
                        eventAdded = true;
                    }
                }

                /// <summary>
                /// please overload with a if statement to check for mode you want or else this will
                /// run the checkmode method everytime a mode change happens.
                /// </summary>
                /// <param name="sender"></param>
                /// <param name="e"></param>
                protected virtual void ModeChangeEvent(object sender, Mode e) => CheckMode();


                public T2 GetPrevSetting() => _prevSetting;
                protected void SetPrevSetting(T2 value) => _prevSetting = value;
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

                public abstract void BackupSetting();

                public abstract void UndoChange();

                public override bool Inputs()
                {
                    bool ret = base.Inputs();
                    if (ret) CheckMode();
                    return ret;
                }
            }
        }
    }
}