namespace OpenVIII.Fields.Scripts.Instructions.Abstract
{
    public abstract class REQ : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// The ID of the target Entity.
        /// </summary>
        protected readonly int _objectIndex;

        /// <summary>
        /// Priority
        /// </summary>
        protected readonly int _priority;

        /// <summary>
        /// Label
        /// </summary>
        protected readonly int _scriptId;

        #endregion Fields

        #region Constructors

        public REQ(int objectIndex, int priority, int scriptId)
        {
            _objectIndex = objectIndex;
            _priority = priority;
            _scriptId = scriptId;
        }

        public REQ(int objectIndex, IStack<IJsmExpression> stack)
            : this(objectIndex,
                scriptId: ((IConstExpression)stack.Pop()).Int32(),
                priority: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        #endregion Constructors

        #region Methods

        public abstract override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services);

        public abstract override IAwaitable TestExecute(IServices services);

        public abstract override string ToString();

        #endregion Methods
    }
}