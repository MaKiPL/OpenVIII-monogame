using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class DOFFSET : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _x;
        private readonly IJsmExpression _y;
        private readonly IJsmExpression _z;

        #endregion Fields

        #region Constructors

        public DOFFSET(IJsmExpression x, IJsmExpression y, IJsmExpression z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public DOFFSET(int parameter, IStack<IJsmExpression> stack)
            : this(
                z: (IConstExpression)stack.Pop(),
                y: (IConstExpression)stack.Pop(),
                x: (IConstExpression)stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override IAwaitable TestExecute(IServices services)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(DOFFSET)}({_x}, {_y}, {_z})");
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(DOFFSET)}({nameof(_x)}: {_x}, {nameof(_y)}: {_y}, {nameof(_z)}: {_z})";

        #endregion Methods
    }
}