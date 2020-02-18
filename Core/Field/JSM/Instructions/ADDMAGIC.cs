using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Add magic stock to character
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/161_ADDMAGIC"/>
    /// <seealso cref="http://wiki.ffrtt.ru/index.php?title=FF8/Magic_Codes"/>
    public sealed class ADDMAGIC : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Character ID
        /// </summary>
        private readonly Characters _characterID;

        /// <summary>
        /// Magic ID
        /// </summary>
        private readonly IJsmExpression _magicID;

        /// <summary>
        /// Quantity
        /// </summary>
        private readonly IJsmExpression _quantity;

        #endregion Fields

        #region Constructors

        public ADDMAGIC(IJsmExpression quantity, IJsmExpression magicID, Characters characterid)
        {
            _quantity = quantity;
            _magicID = magicID;
            _characterID = characterid;
        }

        public ADDMAGIC(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                characterid: ((IConstExpression)stack.Pop()).Characters(),
                magicID: stack.Pop(),
                quantity: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override String ToString() => $"{nameof(ADDMAGIC)}({nameof(_quantity)}: {_quantity}, {nameof(_magicID)}: {_magicID}, {nameof(_characterID)}: {_characterID})";

        #endregion Methods
    }
}