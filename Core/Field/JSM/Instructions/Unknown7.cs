namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Counterclockwise Turn</para>
    /// <para>Turns this entity counterclockwise to face some direction. The only noticeable difference between this and the other turn functions is that the turn is always counterclockwise.</para>
    /// <para>It is unknown how this differs from UNKNOWN9.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/172_UNKNOWN7"/>
    public sealed class Unknown7 : Abstract.TURN
    {
        #region Constructors

        public Unknown7(IJsmExpression frames, IJsmExpression angle) : base(frames, angle)
        {
        }

        public Unknown7(int parameter, IStack<IJsmExpression> stack) : base(parameter, stack)
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(Unknown7)}({nameof(_frames)}: {_frames}, {nameof(_angle)}: {_angle})";

        #endregion Methods
    }
}