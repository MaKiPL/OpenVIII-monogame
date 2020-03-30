namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Label</para>
    /// <para>Specify the absolute identifier of the script. Each script starts with this opcode. This may be redundant and optional information.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/005_LBL"/>
    public sealed class LBL : JsmInstruction
    {
        #region Constructors

        public LBL(int label) => Label = label;

        public LBL(int label, IStack<IJsmExpression> stack)
                    : this(label)
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Script ID.
        /// </summary>
        public int Label { get; }

        #endregion Properties

        #region Methods

        public override IAwaitable Execute(IServices services) => DummyAwaitable.Instance;

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.AppendLine($"// ScriptId: {Label}");

        public override string ToString() => $"{nameof(LBL)}({nameof(Label)}: {Label})";

        #endregion Methods
    }
}