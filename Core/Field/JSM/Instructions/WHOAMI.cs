namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Get Junction Correspondent?</para>
    /// <para>Pushes the value of this character's "real world" character into temp variable 0.</para>
    /// <para>This is only used twice in the game - both at Esthar's "front gate" before the last dream sequence.</para>
    /// </summary>
    public sealed class WHOAMI : JsmInstruction
    {
        #region Fields

        private Characters _characterID;

        #endregion Fields

        #region Constructors

        public WHOAMI(Characters characterID) => _characterID = characterID;

        public WHOAMI(int parameter, IStack<IJsmExpression> stack)
            : this(
                characterID: ((IConstExpression)stack.Pop()).Characters())
        {
        }

        #endregion Constructors

        #region Properties

        public Characters CharacterID { get => _characterID; set => _characterID = value; }

        #endregion Properties

        #region Methods

        public override string ToString() => $"{nameof(WHOAMI)}({nameof(_characterID)}: {_characterID})";

        #endregion Methods
    }
}