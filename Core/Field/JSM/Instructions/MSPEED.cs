using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Sets this entity's movement speed. 
    /// </summary>
    internal sealed class MSPEED : JsmInstruction
    {
        private IJsmExpression _speed;

        public MSPEED(IJsmExpression speed)
        {
            _speed = speed;
        }

        public MSPEED(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                speed: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(MSPEED)}({nameof(_speed)}: {_speed})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Property(nameof(FieldObjectInteraction.MovementSpeed))
                .Assign(_speed)
                .Comment(nameof(MSPEED));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            var currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Interaction.MovementSpeed = _speed.Int32(services);
            return DummyAwaitable.Instance;
        }
    }
}