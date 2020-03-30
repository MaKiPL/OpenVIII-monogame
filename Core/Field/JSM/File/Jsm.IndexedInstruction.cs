using OpenVIII.Fields.Scripts.Instructions;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public sealed class IndexedInstruction
        {
            #region Constructors

            public IndexedInstruction(int index, JsmInstruction instruction)
            {
                Index = index;
                Instruction = instruction;
            }

            #endregion Constructors

            #region Properties

            public int Index { get; }
            public JsmInstruction Instruction { get; }

            #endregion Properties

            #region Methods

            public override string ToString() => $"[Index: {Index}] {Instruction}";

            #endregion Methods
        }

        #endregion Classes
    }
}