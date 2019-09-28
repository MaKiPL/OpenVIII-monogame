using Microsoft.Xna.Framework;
using System.Collections.Concurrent;

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

                private IGMData_Container header;

                /// <summary>
                /// Keeps remainder between cycles
                /// </summary>
                private double remaining = 0;

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
                        _exp = value;
                        RefreshEXP();
                    }
                }

                public ConcurrentDictionary<Characters, int> EXPExtra { get; set; }

                private bool remainEXP => (_exp > 0 || EXPExtra != null && EXPExtra.Count > 0);

                #endregion Properties

                #region Methods

                public override void Draw()
                {
                    if (Enabled)
                        header?.Draw();
                    base.Draw();
                }

                public override bool Inputs_CANCEL() => false;

                public override bool Inputs_OKAY()
                {
                    base.Inputs_OKAY();
                    if (!countingDown && remainEXP)
                    {
                        countingDown = true;
                        if (EXPsnd == null)
                            EXPsnd = init_debugger_Audio.PlaySound(34, loop: true);
                        return true;
                    }
                    else if (countingDown && remainEXP)
                    {
                        countingDown = false;
                        EXPsnd.Stop();
                        EXPsnd = null;
                        EXP = 0;
                        return true;
                    }
                    return false;
                }

                public override bool Update()
                {
                    if (countingDown)
                    {
                        if (remainEXP)
                        {
                            if ((remaining += Memory.gameTime.ElapsedGameTime.TotalMilliseconds / speedOfEarningExp) > 1)
                            {
                                if (EXP > 0)
                                {
                                    EXP -= (int)remaining;
                                }
                                else
                                {
                                    int total = 0;
                                    foreach(var e in EXPExtra)
                                    {
                                        if (e.Value > 0)
                                        total += (EXPExtra[e.Key] -= (int)remaining);
                                        RefreshEXP();
                                    }
                                    if (total <= 0)
                                        EXPExtra = null;
                                }
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
                    header = new IGMData_Container(new IGMDataItem_Box(Memory.Strings.Read(Strings.FileID.KERNEL, 30, 23), new Rectangle(0,0, CONTAINER.Width, 78), Icons.ID.INFO, Box_Options.Middle));
                }

                private void RefreshEXP()
                {
                    foreach (Menu_Base i in ITEM)
                    {
                        int tmpexp = EXP;
                        if (EXPExtra != null)
                        {
                            if (EXPExtra.TryGetValue(i.Character, out int bonus))
                                tmpexp += bonus;
                            else if (EXPExtra.TryGetValue(i.VisibleCharacter, out int bonus2))
                                tmpexp += bonus2;
                        }
                        ((IGMData_PlayerEXP)i).EXP = tmpexp;
                    }
                    header.CONTAINER.Width = CONTAINER.Width;
                }

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}