using System;

namespace OpenVIII.Fields.Scripts.Instructions.Abstract
{
    public abstract class KEY : JsmInstruction
    {
        #region Fields

        protected readonly KeyFlags _flags;

        #endregion Fields

        #region Constructors

        public KEY(KeyFlags flags) => _flags = flags;

        public KEY(int parameter, IStack<IJsmExpression> stack)
            : this(
                flags: (KeyFlags)((IConstExpression)stack.Pop()).Int32())
        {
        }

        #endregion Constructors

        #region Enums

        [Flags]
        public enum KeyFlags : byte
        {
            Cancel = 0x10,
            Menu = 0x20,
            Okay = 0x40,
            Card = 0x80,
        }

        #endregion Enums

        #region Methods

        public abstract override string ToString();

        #endregion Methods
    }
}