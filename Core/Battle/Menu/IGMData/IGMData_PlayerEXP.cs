using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace OpenVIII
{

    public partial class BattleMenus
    {

        public partial class VictoryMenu
        {
            private class IGMData_PlayerEXP : IGMData
            {

                public IGMData_PlayerEXP(sbyte partypos) : base(1, 4, new IGMDataItem_Box(pos:new Rectangle(35,78+partypos*150,808,150)),1,1,partypos:partypos)
                {
                    Debug.Assert(partypos >= 0 && partypos <= 2);
                }
                protected override void Init()
                {
                    if (Character != Characters.Blank)
                    {
                        if(ECN == null)
                            ECN = Memory.Strings.Read(Strings.FileID.KERNEL, 30, 29) + "\n" +
                                Memory.Strings.Read(Strings.FileID.KERNEL, 30, 30) + "\n" +
                                Memory.Strings.Read(Strings.FileID.KERNEL, 30, 31);
                        uint exp = Memory.State[Character].Experience;
                        ushort expTNL = Memory.State[Character].ExperienceToNextLevel;
                        byte lvl = Memory.State[Character].Level;
                        base.Init();
                    }
                }
                public override bool Update()
                {
                    if (Character != Characters.Blank)
                    {
                        return base.Update();
                    }
                    return false;
                }
            }
        }

    }
}