using System;


namespace OpenVIII.Fields
{
    /// <summary>
    /// Requests that the entity associated with a character in the current party executes one of its member functions at a specified priority.
    /// If the specified priority is already busy executing, the request will block until it becomes available and only then return.
    /// The remote execution is still carried out asynchronously, with no notification of completion. 
    /// </summary>
    internal sealed class PREQSW : JsmInstruction
    {
        private Int32 _partyId;
        private Int32 _priority;
        private Int32 _scriptId;

        public PREQSW(Int32 partyId, Int32 priority, Int32 scriptId)
        {
            _partyId = partyId;
            _priority = priority;
            _scriptId = scriptId;
        }

        public PREQSW(Int32 partyId, IStack<IJsmExpression> stack)
            : this(partyId,
                scriptId: ((IConstExpression)stack.Pop()).Int32(),
                priority: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(PREQSW)}({nameof(_partyId)}: {_partyId}, {nameof(_priority)}: {_priority}, {nameof(_scriptId)}: {_scriptId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            formatterContext.GetObjectScriptNamesById(_scriptId, out String typeName, out String methodName);

            sw.AppendLine($"{nameof(PREQSW)}(priority: {_priority}, GetObject<{typeName}>().{methodName}());");
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject targetObject = ServiceId.Party[services].FindPartyCharacterObject(_partyId);
            if (targetObject == null)
                throw new NotSupportedException($"Unknown expected behavior when trying to call a method of a nonexistent party character (Slot: {_partyId}).");

            if (!targetObject.IsActive)
                throw new NotSupportedException($"Unknown expected behavior when trying to call a method of the inactive object (Slot: {_partyId}).");

            targetObject.Scripts.Execute(_scriptId, _priority);
            return DummyAwaitable.Instance;
        }
    }
}