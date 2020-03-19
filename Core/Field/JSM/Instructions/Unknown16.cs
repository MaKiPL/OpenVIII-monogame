using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Set Draw Point ID / Assigns this draw point an ID. Draw points with identical IDs share Full/Drained status.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/181_UNKNOWN16"/>
    public sealed class Unknown16 : JsmInstruction
    {
        /// <summary>
        /// Draw point ID
        /// </summary>
        private readonly IJsmExpression _drawPointID;

        public Unknown16(IJsmExpression drawPointID)
        {
            _drawPointID = drawPointID;
        }

        public Unknown16(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                drawPointID: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(Unknown16)}({nameof(_drawPointID)}: {_drawPointID})";
        }
    }
}