using System;
using System.ComponentModel;
using FF8.Core;
using FF8.Framework;
using FF8.JSM.Format;

namespace FF8.JSM.Instructions
{
    /// <summary>
    /// If the script was called by another script, return to the another script where the current script was requested. Else the script is halted. 
    /// </summary>
    internal sealed class IRET : JsmInstruction
    {
        public Int32 Unknown { get; }

        public IRET(Int32 unknown)
        {
            Unknown = unknown;
        }

        public IRET(Int32 unknown, IStack<IJsmExpression> stack)
            : this(unknown)
        {
        }

        public override String ToString()
        {
            return $"{nameof(IRET)}({nameof(Unknown)}: {Unknown})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.AppendLine($"return {nameof(IRET)}({Unknown});");
        }

        public override IAwaitable TestExecute(IServices services)
        {
            if (Unknown != 8)
            {
                throw new NotSupportedException($"The most common case is {nameof(IRET)}(8)." +
                                                "Probably 8 is a set of some flags." +
                                                "But sometimes there are other combinations." +
                                                "What to do in this case is still unknown.");
            }

            return BreakAwaitable.Instance;
        }
    }
}