using System;


namespace OpenVIII
{
    /// <summary>
    /// Make this entity face the PC. Speed is number of frames (larger = slower turn). 
    /// </summary>
    internal sealed class PCTURN : JsmInstruction
    {
        private IJsmExpression _unknown;
        private IJsmExpression _frameDuration;

        public PCTURN(IJsmExpression unknown, IJsmExpression frameDuration)
        {
            _unknown = unknown;
            _frameDuration = frameDuration;
        }

        public PCTURN(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                frameDuration: stack.Pop(),
                unknown: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(PCTURN)}({nameof(_unknown)}: {_unknown}, {nameof(_frameDuration)}: {_frameDuration})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Await()
                .Property(nameof(FieldObject.Model))
                .Method(nameof(FieldObjectModel.RotateToPlayer))
                .Argument("unknown", _unknown)
                .Argument("frameDuration", _frameDuration)
                .Comment(nameof(PCTURN));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;

            Int32 unknown = _unknown.Int32(services);
            Int32 frameDuration = _frameDuration.Int32(services);
            currentObject.Model.RotateToPlayer(unknown, frameDuration);

            return DummyAwaitable.Instance;
        }
    }
}