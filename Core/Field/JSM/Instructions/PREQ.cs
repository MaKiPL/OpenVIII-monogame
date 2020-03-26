using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Requests that the entity associated with a character in the current party executes one of its member functions at a specified priority.
    /// The request is asynchronous and returns immediately without waiting for the remote execution to start or finish.
    /// If the specified priority is already busy executing, the request will fail silently. 
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/017_PREQ"/>
    public sealed class PREQ : Abstract.PREQ
    {
        public PREQ(int objectIndex, int priority, int scriptId) : base(objectIndex, priority, scriptId)
        {
        }

        public PREQ(int objectIndex, IStack<IJsmExpression> stack) : base(objectIndex, stack)
        {
        }

        public override String ToString()
        {
            return $"{nameof(PREQ)}({nameof(_partyId)}: {_partyId}, {nameof(_priority)}: {_priority}, {nameof(_scriptId)}: {_scriptId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            formatterContext.GetObjectScriptNamesById(_scriptId, out var typeName, out var methodName);

            sw.AppendLine($"{nameof(PREQ)}(priority: {_priority}, GetObject<{typeName}>().{methodName}());");
        }

        public override IAwaitable TestExecute(IServices services)
        {
            var targetObject = ServiceId.Party[services].FindPartyCharacterObject(_partyId);
            if (targetObject == null)
                throw new NotSupportedException($"Unknown expected behavior when trying to call a method of a nonexistent party character (Slot: {_partyId}).");

            if (!targetObject.IsActive)
                throw new NotSupportedException($"Unknown expected behavior when trying to call a method of the inactive object (Slot: {_partyId}).");

            targetObject.Scripts.TryExecute(_scriptId, _priority);
            return DummyAwaitable.Instance;
        }
    }
}