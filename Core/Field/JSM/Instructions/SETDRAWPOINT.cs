using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Set Draw Point hidden</para>
    /// <para>If isHidden is bigger or equal to 1, then hides draw point. If not, the draw point is visible.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/155_SETDRAWPOINT"/>
    public sealed class SETDRAWPOINT : JsmInstruction
    {
        private readonly Boolean _isHidden;

        public SETDRAWPOINT(Boolean isHidden)
        {
            _isHidden = isHidden;
        }

        public SETDRAWPOINT(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                isHidden: ((IConstExpression)stack.Pop()).Boolean())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SETDRAWPOINT)}({nameof(_isHidden)}: {_isHidden})";
        }
    }
}