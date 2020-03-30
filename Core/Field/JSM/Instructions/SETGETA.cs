using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class SETGETA : JsmInstruction
    {
        #region Fields

        private readonly int _arg0;

        #endregion Fields

        #region Constructors

        public SETGETA(int arg0) => _arg0 = arg0;

        public SETGETA(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        #endregion Constructors

        #region Methods

        public override IAwaitable TestExecute(IServices services)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(SETGETA)}({_arg0})");
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(SETGETA)}({nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}