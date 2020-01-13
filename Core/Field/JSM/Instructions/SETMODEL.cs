using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class SETMODEL : JsmInstruction
    {
        private Int32 _modelId;

        public SETMODEL(Int32 modelId)
        {
            _modelId = modelId;
        }

        public SETMODEL(Int32 parameter, IStack<IJsmExpression> stack)
            : this(parameter)
        {
        }

        public override String ToString()
        {
            return $"{nameof(SETMODEL)}({nameof(_modelId)}: {_modelId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Method(nameof(FieldObjectModel.Change))
                .Argument("modelId", _modelId)
                .Comment(nameof(SETMODEL));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Model.Change(_modelId);
            return DummyAwaitable.Instance;
        }
    }
}