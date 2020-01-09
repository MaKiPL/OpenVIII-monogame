using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class GAMEOVER : JsmInstruction
    {
        public GAMEOVER()
        {
        }

        public GAMEOVER(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(GAMEOVER)}()";
        }
    }
}