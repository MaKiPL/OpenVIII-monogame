using System;


namespace FF8
{
    /// <summary>
    /// Requests that the entity associated with a character in the current party executes one of its member functions at a specified priority.
    /// The request will block until remote execution has finished before returning. 
    /// </summary>
    internal sealed class PREQEW : JsmInstruction
    {
        private Int32 _partyId;
        private Int32 _priority;
        private Int32 _scriptId;

        public PREQEW(Int32 partyId, Int32 priority, Int32 scriptId)
        {
            _partyId = partyId;
            _priority = priority;
            _scriptId = scriptId;
        }

        public PREQEW(Int32 partyId, IStack<IJsmExpression> stack)
            : this(partyId,
                scriptId: ((IConstExpression)stack.Pop()).Int32(),
                priority: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(PREQEW)}({nameof(_partyId)}: {_partyId}, {nameof(_priority)}: {_priority}, {nameof(_scriptId)}: {_scriptId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            formatterContext.GetObjectScriptNamesById(_scriptId, out String typeName, out String methodName);

            sw.AppendLine($"{nameof(PREQEW)}(priority: {_priority}, GetObject<{typeName}>().{methodName}());");
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject targetObject = ServiceId.Party[services].FindPartyCharacterObject(_partyId);
            if (targetObject == null)
                throw new NotSupportedException($"Unknown expected behavior when trying to call a method of a nonexistent party character (Slot: {_partyId}).");

            if (!targetObject.IsActive)
                throw new NotSupportedException($"Unknown expected behavior when trying to call a method of the inactive object (Slot: {_partyId}).");

            return targetObject.Scripts.Execute(_scriptId, _priority);
        }
    }
}