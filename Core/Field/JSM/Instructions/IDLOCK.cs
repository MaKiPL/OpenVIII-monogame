namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Lock walkmesh ID</para>
    /// <para>Locks a walkmesh triangle so nothing can walk over it.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/01F_IDLOCK"/>
    public sealed class IDLOCK : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Walkmesh triangle ID
        /// </summary>
        private readonly int _parameter;

        #endregion Fields

        #region Constructors

        public IDLOCK(int parameter) => _parameter = parameter;

        public IDLOCK(int parameter, IStack<IJsmExpression> stack)
            : this(parameter)
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(IDLOCK)}({nameof(_parameter)}: {_parameter})";

        #endregion Methods
    }
}