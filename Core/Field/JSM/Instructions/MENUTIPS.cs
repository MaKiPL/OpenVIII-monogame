using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Trigger a tip?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/156_MENUTIPS&action=edit&redlink=1"/>
    /// <remarks>seen in fields: bgroom_1, bvboat_1, bggate_5, tiagit2, gltown1, gfelone2, gpexit1, 
    /// gmout1, gdsand3, seroom1, elview2, sdisle1, rgcock2, bgmast_1, dogate_1, domt2_1, eciway11,
    /// cdfield1, glfurin5, tistud21, ssadel1, sscont1, sslock1, ssroad2, rghatch1, bgmd1_1, timania5,
    /// gflain11, fhdeck4a, gpbigin4, tmele1, fhwisef2, bghall_6, bggate_1, domt3_3, tiagit4, ggview2,
    /// etsta1, testbl6, rgair3, crodin1, cwwood1, cwwood2, cwwood3, cwwood5, cwwood6, cwwood7, bgsido_2,
    /// bdview1, bdifrit1, bgroad_3</remarks>
    public sealed class MENUTIPS : JsmInstruction
    {
        /// <summary>
        /// Tip BattleID?
        /// </summary>
        /// <remarks>known values: 0, 1, 2, 3, 4, 5, 6, 7, 8, 11, 15, 16, 18, 19, 21, 22, 23, 24, 25, 
        /// 26, 27, 28, 32, 33, 34, 35, 36, 37, 40, 41, 42, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 
        /// 55, 56, 57, 59, 63, 64, 65, 66, 67, 68, 69, 70</remarks>
        private readonly IJsmExpression _arg0;

        public MENUTIPS(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public MENUTIPS(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        //public IJsmExpression Arg0 => _arg0;

        public override String ToString()
        {
            return $"{nameof(MENUTIPS)}({nameof(_arg0)}: {_arg0})";
        }
    }
}