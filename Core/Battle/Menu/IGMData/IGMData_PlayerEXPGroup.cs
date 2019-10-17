using Microsoft.Xna.Framework;
using OpenVIII.IGMDataItem;
using System;
using System.Collections.Concurrent;

namespace OpenVIII
{
    public partial class BattleMenus
    {
        #region Classes

        public partial class VictoryMenu
        {
            #region Classes

            private class IGMData_PlayerEXPGroup : IGMData.Group.Base, IDisposable
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

                private Box header;

                #endregion Fields

                #region Constructors

                public static new IGMData_PlayerEXPGroup Create(params Menu_Base[] d) => Create<IGMData_PlayerEXPGroup>(d);

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
                                    foreach (System.Collections.Generic.KeyValuePair<Characters, int> e in EXPExtra)
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
                    header = new IGMDataItem.Box { Data = Memory.Strings.Read(Strings.FileID.KERNEL, 30, 23), Pos = new Rectangle(0, 0, CONTAINER.Width, 78), Title = Icons.ID.INFO, Options = Box_Options.Middle };
                }

                private void RefreshEXP()
                {
                    foreach (Menu_Base i in ITEM)
                    {
                        int tmpexp = EXP;
                        if (EXPExtra != null && i.Damageable.GetCharacterData(out Saves.CharacterData c) && EXPExtra.TryGetValue(c.ID, out int bonus))
                            tmpexp += bonus;
                        ((IGMData_PlayerEXP)i).EXP = tmpexp;
                    }
                    header.Width = Width;
                }

                #region IDisposable Support

                private bool disposedValue = false; // To detect redundant calls

                protected virtual void Dispose(bool disposing)
                {
                    if (!disposedValue)
                    {
                        if (disposing)
                        {
                            // TODO: dispose managed state (managed objects).
                        }

                        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                        // TODO: set large fields to null.
                        header.Dispose();
                        disposedValue = true;
                    }
                }

                // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
                ~IGMData_PlayerEXPGroup()
                {
                    // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                    Dispose(false);
                }

                // This code added to correctly implement the disposable pattern.
                public void Dispose()
                {
                    // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                    Dispose(true);
                    // TODO: uncomment the following line if the finalizer is overridden above.
                    GC.SuppressFinalize(this);
                }

                #endregion IDisposable Support

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}