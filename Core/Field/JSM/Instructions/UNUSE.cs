using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Unuse entity</para>
    /// <para>Disable this entity's scripts, hides its model, and makes it throughable. Call USE to re-enable. </para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/01A_UNUSE"/>
    public sealed class UNUSE : JsmInstruction
    {
        /// <summary>
        /// Always 0.
        /// </summary>
        private Int32 _parameter;

        public UNUSE(Int32 parameter)
        {
            _parameter = parameter;
        }

        public UNUSE(Int32 parameter, IStack<IJsmExpression> stack)
            : this(parameter)
        {
        }

        public override String ToString()
        {
            return $"{nameof(UNUSE)}({nameof(_parameter)}: {_parameter})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.IsActive))
                .Assign(false)
                .Comment(nameof(UNUSE));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.IsActive = false;
            return DummyAwaitable.Instance;
        }
    }
}