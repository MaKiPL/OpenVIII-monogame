using System;
using FF8.Core;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class SETGETA : JsmInstruction
    {
        private Int32 _arg0;

        public SETGETA(Int32 arg0)
        {
            _arg0 = arg0;
        }

        public SETGETA(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SETGETA)}({nameof(_arg0)}: {_arg0})";
        }

        public override IAwaitable TestExecute(IServices services)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(SETGETA)}({_arg0})");
            return DummyAwaitable.Instance;
        }
    }
}