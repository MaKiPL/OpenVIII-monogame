using System;


namespace OpenVIII.Fields
{
    /// <summary>
    /// Pushes a random number into temp variable 0 in the range [0-255]. 
    /// </summary>
    internal sealed class RND : JsmInstruction
    {
        public static ScriptResultId ResultVariable { get; } = new ScriptResultId(0);

        public RND()
        {
        }

        public RND(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(RND)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.AppendLine($"R{ResultVariable.ResultId} = {nameof(Rnd)}.{nameof(Rnd.NextByte)}();");
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Interaction[services][ResultVariable] = Rnd.NextByte();

            return DummyAwaitable.Instance;
        }
    }
}