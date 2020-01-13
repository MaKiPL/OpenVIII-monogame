using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class LBL : JsmInstruction
    {
        public Int32 Label { get; }

        public LBL(Int32 label)
        {
            Label = label;
        }

        public LBL(Int32 label, IStack<IJsmExpression> stack)
            : this(label)
        {
        }

        public override String ToString()
        {
            return $"{nameof(LBL)}({nameof(Label)}: {Label})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.AppendLine($"// ScriptId: {Label}");
        }

        public override IAwaitable Execute(IServices services)
        {
            return DummyAwaitable.Instance;
        }
    }
}