using System;

namespace FF8.Core
{
    public interface IPartyService
    {
        Boolean IsSupported { get; }

        Boolean IsPartySwitchEnabled { get; set; }

        void AddPlayableCharacter(CharacterId characterId);
        void RemovePlayableCharacter(CharacterId characterId);

        void AddPartyCharacter(CharacterId characterId);
        void RemovePartyCharacter(CharacterId characterId);

        void ChangeCharacterState(CharacterId characterId, Boolean isSwitchable, Boolean isSelectable);
        void ChangeParty(CharacterId characterId1, CharacterId characterId2, CharacterId characterId3);
        
        FieldObject FindPartyCharacterObject(Int32 partyId);
    }
}