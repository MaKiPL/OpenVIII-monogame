using System;

namespace OpenVIII.Fields
{
    public sealed class FieldObject
    {
        #region Constructors

        public FieldObject(int objId, string internalName)
        {
            Id = objId;
            InternalName = internalName;
        }

        #endregion Constructors

        #region Properties

        public FieldObjectAnimation Animation { get; } = new FieldObjectAnimation();
        public Characters CharacterId { get; set; }
        public int Id { get; }
        public FieldObjectInteraction Interaction { get; } = new FieldObjectInteraction();
        public string InternalName { get; }

        public bool IsActive { get; set; } = true;
        public FieldObjectModel Model { get; } = new FieldObjectModel();
        public FieldObjectScripts Scripts { get; } = new FieldObjectScripts();

        #endregion Properties

        #region Methods

        public void BindCharacter(Characters characterId)
        {
            CharacterId = characterId;

            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObject)}.{nameof(BindCharacter)}({characterId})");
        }

        #endregion Methods
    }
}