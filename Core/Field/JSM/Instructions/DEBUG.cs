using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Debug (unused)</para>
    /// <para>No apparent function. Most likely, this did various things during the game's development.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/01B_DEBUG"/>
    internal sealed class DEBUG : JsmInstruction
    {
        #region Constructors

        public DEBUG() => throw new NotSupportedException();

        #endregion Constructors
    }
}