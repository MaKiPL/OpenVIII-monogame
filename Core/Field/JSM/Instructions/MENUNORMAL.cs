using System;


namespace OpenVIII.Fields
{
    internal sealed class MENUNORMAL : JsmInstruction
    {
        public MENUNORMAL()
        {
        }

        public MENUNORMAL(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MENUNORMAL)}()";
        }
    }
}