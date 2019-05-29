using System;


namespace FF8
{
    internal sealed class RANIMELOOP : JsmInstruction
    {
        private Int32 _parameter;

        public RANIMELOOP(Int32 parameter)
        {
            _parameter = parameter;
        }

        public RANIMELOOP(Int32 parameter, IStack<IJsmExpression> stack)
            : this(parameter)
        {
        }

        public override String ToString()
        {
            return $"{nameof(RANIMELOOP)}({nameof(_parameter)}: {_parameter})";
        }
    }
}