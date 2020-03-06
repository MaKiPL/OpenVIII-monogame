using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>SPU Sync</para>
    /// <para>Pauses this script until frame Count frames have passed since SPUREADY was called.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/164_SPUSYNC"/>
    public sealed class SPUSYNC : JsmInstruction
    {
        /// <summary>
        /// Frame Count
        /// </summary>
        private IJsmExpression _frameCount;

        public SPUSYNC(IJsmExpression frameCount)
        {
            _frameCount = frameCount;
        }

        public SPUSYNC(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                frameCount: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SPUSYNC)}({nameof(_frameCount)}: {_frameCount})";
        }
    }
}