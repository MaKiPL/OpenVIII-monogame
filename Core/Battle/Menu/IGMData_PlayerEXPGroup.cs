using Microsoft.Xna.Framework;

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
                /// <list type="bullet">
                /// <item>
                /// <term>1</term>
                /// <description>1000 per second</description>
                /// </item>
                /// <item>
                /// <term>2</term>
                /// <description>500 per second</description>
                /// </item>
                /// <item>
                /// <term>3</term>
                /// <description>333.333... per second</description>
                /// </item>
                /// <item>
                /// <term>4</term>
                /// <description>250 per second</description>
                /// </item>
                /// </list>
                private const float speedOfEarningExp = 4;

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
                private double remaining = 0;

                private IGMData_Container header;

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

                public override bool Inputs_CANCEL() => false;

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
                    if (header == null && CONTAINER != null)
                        header = new IGMData_Container(new IGMDataItem_Box(Memory.Strings.Read(Strings.FileID.KERNEL, 30, 23), new Rectangle(Point.Zero, new Point(CONTAINER.Width, 78)), Icons.ID.INFO, Box_Options.Middle));
                    return base.Update();
                }

                protected override void Init()
                {
                    base.Init();
                    Cursor_Status |= (Cursor_Status.Hidden | (Cursor_Status.Enabled | Cursor_Status.Static));
                }

                public override void Draw()
                {
                    if (Enabled)
                        header?.Draw();
                    base.Draw();
                }

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}