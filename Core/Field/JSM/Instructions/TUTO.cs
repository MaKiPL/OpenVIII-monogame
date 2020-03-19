using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Show Tutorial
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/183_UNKNOWN18"/>
    public sealed class TUTO : JsmInstruction
    {
        /// <summary>
        /// Tutorial ID
        /// </summary>
        private readonly IJsmExpression _tutorialID;

        public TUTO(IJsmExpression tutorialID)
        {
            _tutorialID = tutorialID;
        }

        public TUTO(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                tutorialID: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(TUTO)}({nameof(_tutorialID)}: {_tutorialID})";
        }
    }
}