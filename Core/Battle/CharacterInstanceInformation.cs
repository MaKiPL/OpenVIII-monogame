namespace OpenVIII.Battle
{
    /// <summary>
    /// CharacterInstanceInformation should only be used for battle-exclusive data. Manipulating
    /// HP, GFs, junctions and other character-specific things should happen outside battle,
    /// because such information about characters is shared between almost all modules. This
    /// field contains information about the current status of battle rendering like animation
    /// frames/ rendering flags/ effects attached
    /// </summary>
    public class CharacterInstanceInformation
    {
        #region Fields

        public AnimationSystem animationSystem;
        public bool bIsHidden;
        public int characterId;
        public CharacterData Data;

        #endregion Fields

        #region Properties

        //0 is Whatever guy
        public Characters VisibleCharacter => (Characters)Data.character.GetId;

        #endregion Properties

        #region Methods

        //GF sequences, magic...
        public void SetAnimationID(int id)
        {
            if (animationSystem.AnimationId != id &&
                id < Data.character.animHeader.animations.Length &&
                id < Data.weapon.animHeader.animations.Length &&
                id >= 0)
            {
                animationSystem.AnimationId = id;
            }
        }

        #endregion Methods
    }
}