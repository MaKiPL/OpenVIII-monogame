using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Return</para>
    /// If the script was called by another script, return to the another script where the current script was requested. Else the script is halted.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/006_RET"/>
    public sealed class IRET : JsmInstruction
    {
        #region Constructors

        public IRET(int unknown) => Unknown = unknown;

        public IRET(int unknown, IStack<IJsmExpression> stack)
            : this(unknown)
        {
        }

        #endregion Constructors

        #region Properties

        public int Unknown { get; }

        #endregion Properties

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.AppendLine($"return {nameof(IRET)}({Unknown});");

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

        public override string ToString() => $"{nameof(IRET)}({nameof(Unknown)}: {Unknown})";

        #endregion Methods
    }
}