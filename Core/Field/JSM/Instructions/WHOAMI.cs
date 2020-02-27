using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Get Junction Correspondent?</para>
    /// <para>Pushes the value of this character's "real world" character into temp variable 0.</para>
    /// <para>This is only used twice in the game - both at Esthar's "front gate" before the last dream sequence.</para>
    /// </summary>
    public sealed class WHOAMI : JsmInstruction
    {
        private Characters _characterID;

        public WHOAMI(Characters characterID)
        {
            _characterID = characterID;
        }

        public WHOAMI(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                characterID: ((IConstExpression)stack.Pop()).Characters())
        {
        }

        public Characters CharacterID { get => _characterID; set => _characterID = value; }

        public override String ToString()
        {
            return $"{nameof(WHOAMI)}({nameof(_characterID)}: {_characterID})";
        }
    }
}