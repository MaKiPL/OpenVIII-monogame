using System;


namespace OpenVIII.Fields
{
    /// <summary>
    /// Exits the current script and all scripts that are waiting on it. To end only the current script, use RET instead. 
    /// </summary>
    internal sealed class HALT : JsmInstruction
    {
        private Int32 _parameter;

        public HALT(Int32 parameter)
        {
            _parameter = parameter;
        }

        public HALT(Int32 parameter, IStack<IJsmExpression> stack)
            : this(parameter)
        {
        }

        public override String ToString()
        {
            return $"{nameof(HALT)}({nameof(_parameter)}: {_parameter})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Scripts))
                .Method(nameof(FieldObjectScripts.CancelAll))
                .Comment(nameof(HALT));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Scripts.CancelAll();
            return BreakAwaitable.Instance;
        }
    }
}