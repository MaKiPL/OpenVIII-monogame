using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Unlock walkmesh ID</para>
    /// <para>Unlocks a walkmesh triangle so things can walk over it.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/020_IDUNLOCK"/>
    public sealed class IDUNLOCK : JsmInstruction
    {
        /// <summary>
        /// Walkmesh triangle ID
        /// </summary>
        private Int32 _parameter;

        public IDUNLOCK(Int32 parameter)
        {
            _parameter = parameter;
        }

        public IDUNLOCK(Int32 parameter, IStack<IJsmExpression> stack)
            : this(parameter)
        {
        }

        public override String ToString()
        {
            return $"{nameof(IDUNLOCK)}({nameof(_parameter)}: {_parameter})";
        }
    }
}