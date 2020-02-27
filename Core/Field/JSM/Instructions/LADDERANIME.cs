using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// ladder climbing animation?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/037_LADDERANIME"/>
    public sealed class LADDERANIME : JsmInstruction
    {
        private Int32 _parameter;
        private IJsmExpression _arg0;
        private IJsmExpression _arg1;

        public LADDERANIME(Int32 parameter, IJsmExpression arg0, IJsmExpression arg1)
        {
            _parameter = parameter;
            _arg0 = arg0;
            _arg1 = arg1;
        }

        public LADDERANIME(Int32 parameter, IStack<IJsmExpression> stack)
            : this(parameter,
                arg1: stack.Pop(),
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(LADDERANIME)}({nameof(_parameter)}: {_parameter}, {nameof(_arg0)}: {_arg0}, {nameof(_arg1)}: {_arg1})";
        }
    }
}