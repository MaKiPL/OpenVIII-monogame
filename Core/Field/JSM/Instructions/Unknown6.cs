using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Clockwise Turn</para>
    /// <para>Turns this entity clockwise to face some direction. The only noticeable difference between this and the other turn functions is that the turn is always clockwise.</para>
    /// <para>It is unknown how this differs from UNKNOWN8.</para>
    /// </summary>
    /// <para>http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/171_UNKNOWN6</para>
    public sealed class Unknown6 : Abstract.TURN
    {
        public Unknown6(IJsmExpression frames, IJsmExpression angle) : base(frames, angle)
        {
        }

        public Unknown6(int parameter, IStack<IJsmExpression> stack) : base(parameter, stack)
        {
        }

        public override String ToString()
        {
            return $"{nameof(Unknown6)}({nameof(_frames)}: {_frames}, {nameof(_angle)}: {_angle})";
        }
    }
}