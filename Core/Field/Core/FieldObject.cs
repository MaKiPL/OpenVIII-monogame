using System;
using System.Threading.Tasks;

namespace OpenVIII.Fields
{
    public sealed class FieldObject
    {
        public int Id { get; }
        public string InternalName { get; }

        public FieldObjectScripts Scripts { get; } = new FieldObjectScripts();
        public FieldObjectModel Model { get; } = new FieldObjectModel();
        public FieldObjectInteraction Interaction { get; } = new FieldObjectInteraction();
        public FieldObjectAnimation Animation { get; } = new FieldObjectAnimation();

        public FieldObject(int objId, string internalName)
        {
            Id = objId;
            InternalName = internalName;
        }

        public bool IsActive { get; set; } = true;
        public Characters CharacterId { get; set; }

        public void BindCharacter(Characters characterId)
        {
            CharacterId = characterId;

            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObject)}.{nameof(BindCharacter)}({characterId})");
        }
    }
}