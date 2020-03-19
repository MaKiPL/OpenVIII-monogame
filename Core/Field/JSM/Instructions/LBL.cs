using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Label</para>
    /// <para>Specify the absolute identifier of the script. Each script starts with this opcode. This may be redundant and optional information.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/005_LBL"/>
    public sealed class LBL : JsmInstruction
    {
        /// <summary>
        /// Script ID.
        /// </summary>
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