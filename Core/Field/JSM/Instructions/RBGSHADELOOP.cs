using Microsoft.Xna.Framework;
using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// RBG Shade Loop between two colors.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/0D2_RBGSHADELOOP&action=edit&redlink=1"/>
    public sealed class RBGSHADELOOP : JsmInstruction
    {
        private readonly Int32 _frames0;
        private readonly Int32 _frames1;
        private readonly Int32 _arg8;
        private readonly Int32 _arg9;
        private readonly Color _c0;
        private readonly Color _c1;

        public int Arg0 => _frames0;

        public int Arg1 => _frames1;

        public int Arg8 => _arg8;

        public int Arg9 => _arg9;

        public Color C0 => _c0;

        public Color C1 => _c1;

        public RBGSHADELOOP(Int32 frames0, Int32 frames1, byte red0, byte green0, byte blue0, byte red1, byte green1, byte blue1, Int32 arg8, Int32 arg9)
        {
            _frames0 = frames0; // transitional frame time
            _frames1 = frames1; // transitional frame time
            (_c0.R, _c0.G, _c0.B,_c0.A) = (red0, green0, blue0, 0xFF); 
            (_c1.R, _c1.G, _c1.B, _c1.A) = (red1, green1, blue1, 0xFF);
            _arg8 = arg8; //donno
            _arg9 = arg9; //donno
        }

        public RBGSHADELOOP(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg9: ((IConstExpression)stack.Pop()).Int32(),
                arg8: ((IConstExpression)stack.Pop()).Int32(),
                blue1: ((IConstExpression)stack.Pop()).Byte(),
                green1: ((IConstExpression)stack.Pop()).Byte(),
                red1: ((IConstExpression)stack.Pop()).Byte(),
                blue0: ((IConstExpression)stack.Pop()).Byte(),
                green0: ((IConstExpression)stack.Pop()).Byte(),
                red0: ((IConstExpression)stack.Pop()).Byte(),
                frames1: ((IConstExpression)stack.Pop()).Int32(),
                frames0: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(RBGSHADELOOP)}({nameof(_frames0)}: {_frames0}, {nameof(_frames1)}: {_frames1}, {nameof(_c0)}: {_c0}, {nameof(_c1)}: {_c1}, {nameof(_arg8)}: {_arg8}, {nameof(_arg9)}: {_arg9})";
        }
    }
}