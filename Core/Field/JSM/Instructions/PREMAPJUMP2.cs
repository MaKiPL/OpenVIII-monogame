namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Something having to do with field loading.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/182_UNKNOWN17"/>
    public sealed class PREMAPJUMP2 : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Field map ID
        /// </summary>
        private readonly int _fieldMapId;

        #endregion Fields

        #region Constructors

        public PREMAPJUMP2(int fieldMapId) => _fieldMapId = fieldMapId;

        public PREMAPJUMP2(int parameter, IStack<IJsmExpression> stack)
            : this(
                fieldMapId: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .CommentLine(FieldName.Get(_fieldMapId))
                .StaticType(nameof(IFieldService))
                .Method(nameof(IFieldService.PrepareGoTo))
                .Enum(_fieldMapId)
                .Comment(nameof(PREMAPJUMP2));

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Field[services].PrepareGoTo(_fieldMapId);
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(PREMAPJUMP2)}({nameof(_fieldMapId)}: {_fieldMapId})";

        #endregion Methods
    }
}