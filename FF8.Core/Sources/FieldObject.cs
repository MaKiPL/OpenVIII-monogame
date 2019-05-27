using System;
using System.Threading.Tasks;

namespace FF8.Core
{
    public sealed class FieldObject
    {
        public Int32 Id { get; }
        public String InternalName { get; }

        public FieldObjectScripts Scripts { get; } = new FieldObjectScripts();
        public FieldObjectModel Model { get; } = new FieldObjectModel();
        public FieldObjectInteraction Interaction { get; } = new FieldObjectInteraction();
        public FieldObjectAnimation Animation { get; } = new FieldObjectAnimation();

        public FieldObject(Int32 objId, String internalName)
        {
            Id = objId;
            InternalName = internalName;
        }

        public Boolean IsActive { get; set; } = true;
        public CharacterId CharacterId { get; private set; }

        public void BindChracter(CharacterId characterId)
        {
            CharacterId = characterId;

            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObject)}.{nameof(BindChracter)}({characterId})");
        }
    }
}