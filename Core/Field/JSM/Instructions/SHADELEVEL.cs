using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Shade Level; Sets some shading for the actor. 
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/0AF_SHADELEVEL"/>
    public sealed class SHADELEVEL : JsmInstruction
    {
        private IJsmExpression _arg0;

        public SHADELEVEL(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public SHADELEVEL(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SHADELEVEL)}({nameof(_arg0)}: {_arg0})";
        }
    }
}