using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Request remote execution (synchronous, guaranteed)
    /// Go to the method Label in the group Argument with a specified Priority.
    /// Requests that a remote entity executes one of its member functions at a specified priority. The request will block until remote execution has finished before returning. 
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/016_REQEW"/>
    public sealed class REQEW : Abstract.REQ
    {
        public REQEW(int objectIndex, int priority, int scriptId) : base(objectIndex, priority, scriptId)
        {
        }

        public REQEW(int objectIndex, IStack<IJsmExpression> stack) : base(objectIndex, stack)
        {
        }

        public override String ToString()
        {
            return $"{nameof(REQEW)}({nameof(_objectIndex)}: {_objectIndex}, {nameof(_priority)}: {_priority}, {nameof(_scriptId)}: {_scriptId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            formatterContext.GetObjectScriptNamesById(_scriptId, out var typeName, out var methodName);

            sw.AppendLine($"{nameof(REQEW)}(priority: {_priority}, GetObject<{typeName}>().{methodName}());");
        }

        public override IAwaitable TestExecute(IServices services)
        {
            var engine = ServiceId.Field[services].Engine;

            var targetObject = engine.GetObject(_objectIndex);
            if (!targetObject.IsActive)
                throw new NotSupportedException($"Unknown expected behavior when trying to call a method of the inactive object (Id: {_objectIndex}).");

            return targetObject.Scripts.Execute(_scriptId, _priority);
        }
    }
}