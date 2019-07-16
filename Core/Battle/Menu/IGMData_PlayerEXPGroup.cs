namespace OpenVIII
{
    public partial class BattleMenus
    {
        public partial class VictoryMenu
        {
            private class IGMData_PlayerEXPGroup : IGMData_Group
            {
                private int _exp;

                public IGMData_PlayerEXPGroup(params IGMData_PlayerEXP[] d) : base(d)
                {
                }

                protected override void Init()
                {
                    base.Init();
                    Cursor_Status |= (Cursor_Status.Hidden | (Cursor_Status.Enabled | Cursor_Status.Static));
                }

                public int EXP
                {
                    get => _exp; set
                    {
                        foreach (IGMDataItem_IGMData i in ITEM)
                        {
                            ((IGMData_PlayerEXP)i.Data).EXP = value;
                        }
                        _exp = value;
                    }
                }

                public override bool Update()
                {
                    if (countingDown)
                    {
                        if (_exp > 0)
                            EXP--;
                        else
                        {
                            countingDown = false;
                            snd.Stop();
                            snd = null;
                        }
                    }
                    return base.Update();
                }

                private bool countingDown = false;
                private Ffcc snd = null;
                public override void Inputs_OKAY()
                {
                    base.Inputs_OKAY();
                    if (!countingDown && _exp > 0)
                    {
                        countingDown = true;
                        if(snd == null)
                            snd = init_debugger_Audio.PlaySound(34, loop: true);
                    }
                    else if(countingDown && _exp > 0)
                    {
                        countingDown = false;
                        snd.Stop();
                        snd = null;
                        EXP = 0;
                    }
                }
            }
        }
    }
}