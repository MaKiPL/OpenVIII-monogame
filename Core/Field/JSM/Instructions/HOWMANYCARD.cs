using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>How Many cards? how many cards you have?</para>
    /// <para>Only used on tipub1 with man2 </para>
    /// <para>See if you have this card. So you can return it to the Drifter blocking the door.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/15F_HOWMANYCARD&action=edit&redlink=1"/>
    /// <seealso cref="https://www.ign.com/wikis/final-fantasy-viii/Timber_TV_Station"/>
    public sealed class HOWMANYCARD : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Card BattleID? only value is 12 = Buel. 
        /// </summary>
        private Cards.ID _cardID;

        #endregion Fields

        #region Constructors

        public HOWMANYCARD(Cards.ID cardID) => _cardID = cardID;

        public HOWMANYCARD(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                cardID: ((IConstExpression)stack.Pop()).Cards())
        {
        }

        #endregion Constructors

        #region Methods

        public override String ToString() => $"{nameof(HOWMANYCARD)}({nameof(_cardID)}: {_cardID})";

        #endregion Methods
    }
}