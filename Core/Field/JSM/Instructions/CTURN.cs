using System;

namespace OpenVIII.Fields.Scripts.Instructions
{ 
/// <summary>
/// Make this entity face the entity with the ID of the first parameter.
/// </summary>
internal sealed class CTURN : JsmInstruction
    {
        private readonly Int32 _targetObject;
        private IJsmExpression _frameDuration;

        public CTURN(Int32 targetObject, IJsmExpression frameDuration)
        {
            _targetObject = targetObject;
            _frameDuration = frameDuration;
        }

        public CTURN(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                frameDuration: stack.Pop(),
                targetObject: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(CTURN)}({nameof(_targetObject)}: {_targetObject}, {nameof(_frameDuration)}: {_frameDuration})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Await()
                .Property(nameof(FieldObject.Model))
                .Method(nameof(FieldObjectModel.RotateToObject))
                .Argument("targetObject", _targetObject)
                .Argument("frameDuration", _frameDuration)
                .Comment(nameof(CTURN));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;

            Int32 frameDuration = _frameDuration.Int32(services);
            currentObject.Model.RotateToObject(_targetObject, frameDuration);

            return DummyAwaitable.Instance;
        }
    }
}