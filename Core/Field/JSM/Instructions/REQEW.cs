using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Request remote execution (synchronous, guaranteed)
    /// Go to the method Label in the group Argument with a specified Priority.
    /// Requests that a remote entity executes one of its member functions at a specified priority. The request will block until remote execution has finished before returning. 
    /// </summary>
    internal sealed class REQEW : JsmInstruction
    {
        private Int32 _objectIndex;
        private Int32 _priority;
        private Int32 _scriptId;

        public REQEW(Int32 objectIndex, Int32 priority, Int32 scriptId)
        {
            _objectIndex = objectIndex;
            _priority = priority;
            _scriptId = scriptId;
        }

        public REQEW(Int32 objectIndex, IStack<IJsmExpression> stack)
            : this(objectIndex,
                scriptId: ((IConstExpression)stack.Pop()).Int32(),
                priority: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(REQEW)}({nameof(_objectIndex)}: {_objectIndex}, {nameof(_priority)}: {_priority}, {nameof(_scriptId)}: {_scriptId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            formatterContext.GetObjectScriptNamesById(_scriptId, out String typeName, out String methodName);

            sw.AppendLine($"{nameof(REQEW)}(priority: {_priority}, GetObject<{typeName}>().{methodName}());");
        }

        public override IAwaitable TestExecute(IServices services)
        {
            EventEngine engine = ServiceId.Field[services].Engine;

            FieldObject targetObject = engine.GetObject(_objectIndex);
            if (!targetObject.IsActive)
                throw new NotSupportedException($"Unknown expected behavior when trying to call a method of the inactive object (Id: {_objectIndex}).");

            return targetObject.Scripts.Execute(_scriptId, _priority);
        }
    }
}