namespace OpenVIII
{
    public partial class BattleMenus
    {
        #region Classes

        public partial class VictoryMenu
        {
            #region Classes

            private class IGMData_PlayerEXPGroup : IGMData_Group
            {
                #region Fields

                /// <summary>
                /// <para>The Speed the exp counts down.</para>
                /// <para>Cannot be 0.</para>
                /// The smaller the number the faster it'll count down.
                /// </summary>
                private const float speedOfEarningExp = 2;
                /// <summary>
                /// Total exp left to earn.
                /// </summary>
                private int _exp;

                /// <summary>
                /// Are we in counting down exp mode.
                /// </summary>
                private bool countingDown = false;

                /// <summary>
                /// The looping exp sound. Need to track the object here to stop the loop.
                /// </summary>
                private Ffcc EXPsnd = null;

                /// <summary>
                /// Keeps remainder between cycles
                /// </summary>
                double remaining = 0;

                #endregion Fields

                #region Constructors

                public IGMData_PlayerEXPGroup(params IGMData_PlayerEXP[] d) : base(d)
                {
                }

                #endregion Constructors

                #region Properties

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

                #endregion Properties

                #region Methods

                public override void Inputs_OKAY()
                {
                    base.Inputs_OKAY();
                    if (!countingDown && _exp > 0)
                    {
                        countingDown = true;
                        if (EXPsnd == null)
                            EXPsnd = init_debugger_Audio.PlaySound(34, loop: true);
                    }
                    else if (countingDown && _exp > 0)
                    {
                        countingDown = false;
                        EXPsnd.Stop();
                        EXPsnd = null;
                        EXP = 0;
                    }
                }

                public override bool Update()
                {
                    if (countingDown)
                    {
                        if (_exp > 0)
                        {
                            if ((remaining += Memory.gameTime.ElapsedGameTime.TotalMilliseconds / speedOfEarningExp) > 1)
                            {
                                EXP -= (int)remaining;
                                remaining -= (int)remaining;
                            }
                        }
                        else
                        {
                            countingDown = false;
                            EXPsnd.Stop();
                            EXPsnd = null;
                        }
                    }
                    return base.Update();
                }

                protected override void Init()
                {
                    base.Init();
                    Cursor_Status |= (Cursor_Status.Hidden | (Cursor_Status.Enabled | Cursor_Status.Static));
                }

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}