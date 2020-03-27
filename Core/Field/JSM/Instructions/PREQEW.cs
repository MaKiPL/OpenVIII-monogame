using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Requests that the entity associated with a character in the current party executes one of its member functions at a specified priority.
    /// The request will block until remote execution has finished before returning.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/019_PREQEW"/>
    public sealed class PREQEW : Abstract.PREQ
    {
        #region Constructors

        public PREQEW(int objectIndex, IStack<IJsmExpression> stack) : base(objectIndex, stack)
        {
        }

        public PREQEW(int objectIndex, int priority, int scriptId) : base(objectIndex, priority, scriptId)
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            formatterContext.GetObjectScriptNamesById(_scriptId, out var typeName, out var methodName);

            sw.AppendLine($"{nameof(PREQEW)}(priority: {_priority}, GetObject<{typeName}>().{methodName}());");
        }

        public override IAwaitable TestExecute(IServices services)
        {
            var targetObject = ServiceId.Party[services].FindPartyCharacterObject(_partyId);
            if (targetObject == null)
                throw new NotSupportedException($"Unknown expected behavior when trying to call a method of a nonexistent party character (Slot: {_partyId}).");

            if (!targetObject.IsActive)
                throw new NotSupportedException($"Unknown expected behavior when trying to call a method of the inactive object (Slot: {_partyId}).");

            return targetObject.Scripts.Execute(_scriptId, _priority);
        }

        public override string ToString() => $"{nameof(PREQEW)}({nameof(_partyId)}: {_partyId}, {nameof(_priority)}: {_priority}, {nameof(_scriptId)}: {_scriptId})";

        #endregion Methods
    }
}