using System;

namespace OpenVIII.Fields
{
    public sealed class PartyService : IPartyService
    {
        #region Properties

        public bool IsPartySwitchEnabled { get; set; }
        public bool IsSupported => true;

        #endregion Properties

        #region Methods

        public void AddPartyCharacter(Characters characterId) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(PartyService)}.{nameof(AddPartyCharacter)}({nameof(characterId)}: {characterId})");

        public void AddPlayableCharacter(Characters characterId) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(PartyService)}.{nameof(AddPlayableCharacter)}({nameof(characterId)}: {characterId})");

        public void ChangeCharacterState(Characters characterId, bool isSwitchable, bool isSelectable) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(PartyService)}.{nameof(ChangeCharacterState)}({nameof(characterId)}: {characterId}, {nameof(isSwitchable)}: {isSwitchable}, {nameof(isSelectable)}: {isSelectable})");

        public void ChangeParty(Characters characterId1, Characters characterId2, Characters characterId3) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(PartyService)}.{nameof(ChangeParty)}({nameof(characterId1)}: {characterId1}, {nameof(characterId2)}: {characterId2}, {nameof(characterId3)}: {characterId3})");

        public FieldObject FindPartyCharacterObject(int partyId)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(PartyService)}.{nameof(FindPartyCharacterObject)}({nameof(partyId)}: {partyId})");

            return null;
        }

        public void RemovePartyCharacter(Characters characterId) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(PartyService)}.{nameof(RemovePartyCharacter)}({nameof(characterId)}: {characterId})");

        public void RemovePlayableCharacter(Characters characterId) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(PartyService)}.{nameof(RemovePlayableCharacter)}({nameof(characterId)}: {characterId})");

        #endregion Methods
    }
}