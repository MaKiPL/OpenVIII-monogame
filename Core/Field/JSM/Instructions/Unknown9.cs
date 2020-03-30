namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Counterclockwise Turn</para>
    /// <para>Turns this entity counterclockwise to face some direction. The only noticeable difference between this and the other turn functions is that the turn is always counterclockwise.</para>
    /// <para>It is unknown how this differs from UNKNOWN7.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/174_UNKNOWN9"/>
    public sealed class Unknown9 : Abstract.TURN
    {
        #region Constructors

        public Unknown9(IJsmExpression frames, IJsmExpression angle) : base(frames, angle)
        {
        }

        public Unknown9(int parameter, IStack<IJsmExpression> stack) : base(parameter, stack)
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(Unknown9)}({nameof(_frames)}: {_frames}, {nameof(_angle)}: {_angle})";

        #endregion Methods
    }
}