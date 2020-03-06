using System;

namespace OpenVIII.Fields.Scripts.Instructions.Abstract
{
    public abstract class REQ : JsmInstruction
    {
        /// <summary>
        /// The BattleID of the target Entity.
        /// </summary>
        protected readonly Int32 _objectIndex;
        /// <summary>
        /// Priority
        /// </summary>
        protected readonly Int32 _priority;
        /// <summary>
        /// Label 
        /// </summary>
        protected readonly Int32 _scriptId;

        public REQ(Int32 objectIndex, Int32 priority, Int32 scriptId)
        {
            _objectIndex = objectIndex;
            _priority = priority;
            _scriptId = scriptId;
        }

        public REQ(Int32 objectIndex, IStack<IJsmExpression> stack)
            : this(objectIndex,
                scriptId: ((IConstExpression)stack.Pop()).Int32(),
                priority: ((IConstExpression)stack.Pop()).Int32())
        {
        }
        public abstract override string ToString();
        public abstract override IAwaitable TestExecute(IServices services);
        public abstract override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services);
    }
}
