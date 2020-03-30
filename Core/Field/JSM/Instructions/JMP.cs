using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Jump</para>
    /// <para>Jump a number of instructions given by Argument. If Argument is negative, jumps backward.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/002_JMP"/>
    public sealed class JMP : JsmInstruction, IJumpToOpcode
    {
        #region Fields

        private int _index = -1;

        #endregion Fields

        #region Constructors

        public JMP(int offset) => Offset = offset;

        public JMP(int offset, IStack<IJsmExpression> stack)
                    : this(offset)
        {
        }

        #endregion Constructors

        #region Properties

        public int Index
        {
            get
            {
                if (_index == -1)
                    throw new ArgumentException($"{nameof(JMP)} instruction isn't indexed yet.", nameof(Index));
                return _index;
            }
            set =>
                //if (_index != -1)
                //    throw new ArgumentException($"{nameof(JMP)} instruction has already been indexed: {_index}.", nameof(Index));
                _index = value;
        }

        /// <summary>
        /// Number of instructions to jump (signed value).
        /// </summary>
        public int Offset { get; set; }

        #endregion Properties

        #region Methods

        public override IAwaitable Execute(IServices services) =>
            // This instruction is part of conditional jumps and isn't exists as is.
            DummyAwaitable.Instance;

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            // This instruction is part of conditional jumps and isn't exists as is.
        }

        public override string ToString() => _index < 0
                ? $"{nameof(JMP)}({nameof(Offset)}: {Offset})"
                : $"{nameof(JMP)}({nameof(Index)}: {Index})";

        #endregion Methods
    }
}