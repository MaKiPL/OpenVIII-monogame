using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class DOFFSET : JsmInstruction
    {
        private IJsmExpression _x;
        private IJsmExpression _y;
        private IJsmExpression _z;

        public DOFFSET(IJsmExpression x, IJsmExpression y, IJsmExpression z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public DOFFSET(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                z: (IConstExpression)stack.Pop(),
                y: (IConstExpression)stack.Pop(),
                x: (IConstExpression)stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(DOFFSET)}({nameof(_x)}: {_x}, {nameof(_y)}: {_y}, {nameof(_z)}: {_z})";
        }

        public override IAwaitable TestExecute(IServices services)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(DOFFSET)}({_x}, {_y}, {_z})");
            return DummyAwaitable.Instance;
        }
    }
}