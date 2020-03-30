namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Clockwise Turn</para>
    /// <para>Turns this entity clockwise to face some direction. The only noticeable difference between this and the other turn functions is that the turn is always clockwise.</para>
    /// <para>It is unknown how this differs from UNKNOWN6.</para>
    /// </summary>
    /// <para>http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/173_UNKNOWN8</para>
    public sealed class Unknown8 : Abstract.TURN
    {
        #region Constructors

        public Unknown8(IJsmExpression frames, IJsmExpression angle) : base(frames, angle)
        {
        }

        public Unknown8(int parameter, IStack<IJsmExpression> stack) : base(parameter, stack)
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(Unknown8)}({nameof(_frames)}: {_frames}, {nameof(_angle)}: {_angle})";

        #endregion Methods
    }
}