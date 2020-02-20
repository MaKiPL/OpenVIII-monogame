using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Lock walkmesh ID</para>
    /// <para>Locks a walkmesh triangle so nothing can walk over it.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/01F_IDLOCK"/>
    public sealed class IDLOCK : JsmInstruction
    {
        /// <summary>
        /// Walkmesh triangle ID
        /// </summary>
        private Int32 _parameter;

        public IDLOCK(Int32 parameter)
        {
            _parameter = parameter;
        }

        public IDLOCK(Int32 parameter, IStack<IJsmExpression> stack)
            : this(parameter)
        {
        }

        public override String ToString()
        {
            return $"{nameof(IDLOCK)}({nameof(_parameter)}: {_parameter})";
        }
    }
}