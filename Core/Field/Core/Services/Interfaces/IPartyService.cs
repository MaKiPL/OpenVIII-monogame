using System;

namespace OpenVIII.Fields
{
    public interface IPartyService
    {
        Boolean IsSupported { get; }

        Boolean IsPartySwitchEnabled { get; set; }

        void AddPlayableCharacter(Characters characterId);
        void RemovePlayableCharacter(Characters characterId);

        void AddPartyCharacter(Characters characterId);
        void RemovePartyCharacter(Characters characterId);

        void ChangeCharacterState(Characters characterId, Boolean isSwitchable, Boolean isSelectable);
        void ChangeParty(Characters characterId1, Characters characterId2, Characters characterId3);
        
        FieldObject FindPartyCharacterObject(Int32 partyId);
    }
}