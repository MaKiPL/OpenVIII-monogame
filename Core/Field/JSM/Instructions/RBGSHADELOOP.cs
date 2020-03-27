using Microsoft.Xna.Framework;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// RBG Shade Loop between two colors.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/0D2_RBGSHADELOOP&action=edit&redlink=1"/>
    public sealed class RBGSHADELOOP : JsmInstruction
    {
        #region Fields

        private readonly Color _c0;
        private readonly Color _c1;
        private readonly int _fadeFrames0;
        private readonly int _fadeFrames1;
        private readonly int _holdFrames0;
        private readonly int _holdFrames1;

        #endregion Fields

        #region Constructors

        public RBGSHADELOOP(int fadeFrames0, int fadeFrames1, byte red0, byte green0, byte blue0, byte red1, byte green1, byte blue1, int holdFrames0, int holdFrames1)
        {
            _fadeFrames0 = fadeFrames0; // transitional frame time 0 being instant color change
            _fadeFrames1 = fadeFrames1; // transitional frame time 0 being instant color change
            (_c0.R, _c0.G, _c0.B, _c0.A) = (red0, green0, blue0, 0xFF);
            (_c1.R, _c1.G, _c1.B, _c1.A) = (red1, green1, blue1, 0xFF);
            _holdFrames0 = holdFrames0 < 1 ? 1 : holdFrames0; //frames to stay on color
            _holdFrames1 = holdFrames1 < 1 ? 1 : holdFrames1; //frames to stay on color
        }

        public RBGSHADELOOP(int parameter, IStack<IJsmExpression> stack)
            : this(
                holdFrames1: ((IConstExpression)stack.Pop()).Int32(),
                holdFrames0: ((IConstExpression)stack.Pop()).Int32(),
                blue1: ((IConstExpression)stack.Pop()).Byte(),
                green1: ((IConstExpression)stack.Pop()).Byte(),
                red1: ((IConstExpression)stack.Pop()).Byte(),
                blue0: ((IConstExpression)stack.Pop()).Byte(),
                green0: ((IConstExpression)stack.Pop()).Byte(),
                red0: ((IConstExpression)stack.Pop()).Byte(),
                fadeFrames1: ((IConstExpression)stack.Pop()).Int32(),
                fadeFrames0: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        #endregion Constructors

        #region Properties

        public Color C0 => _c0;
        public Color C1 => _c1;
        public int FadeFrames0 => _fadeFrames0;

        public int FadeFrames1 => _fadeFrames1;

        public int HoldFrames0 => _holdFrames0;

        public int HoldFrames1 => _holdFrames1;

        #endregion Properties

        #region Methods

        public override string ToString() => $"{nameof(RBGSHADELOOP)}({nameof(_fadeFrames0)}: {_fadeFrames0}, {nameof(_fadeFrames1)}: {_fadeFrames1}, {nameof(_c0)}: {_c0}, {nameof(_c1)}: {_c1}, {nameof(_holdFrames0)}: {_holdFrames0}, {nameof(_holdFrames1)}: {_holdFrames1})";

        #endregion Methods
    }
}