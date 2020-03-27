using System;

namespace OpenVIII.Fields
{
    public interface IPartyService
    {
        #region Properties

        bool IsPartySwitchEnabled { get; set; }
        bool IsSupported { get; }

        #endregion Properties

        #region Methods

        void AddPartyCharacter(Characters characterId);

        void AddPlayableCharacter(Characters characterId);

        void ChangeCharacterState(Characters characterId, bool isSwitchable, bool isSelectable);

        void ChangeParty(Characters characterId1, Characters characterId2, Characters characterId3);

        FieldObject FindPartyCharacterObject(int partyId);

        void RemovePartyCharacter(Characters characterId);

        void RemovePlayableCharacter(Characters characterId);

        #endregion Methods
    }
}