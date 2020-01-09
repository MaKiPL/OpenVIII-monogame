using System;


namespace OpenVIII.Fields
{
    internal sealed class MENUDISABLE : JsmInstruction
    {
        public MENUDISABLE()
        {
        }

        public MENUDISABLE(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MENUDISABLE)}()";
        }
    }
}