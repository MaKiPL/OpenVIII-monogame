using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
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