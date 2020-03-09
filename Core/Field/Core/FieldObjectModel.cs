using System;

namespace OpenVIII.Fields
{
    public sealed class FieldObjectModel
    {
        public Int32 ModelId { get;  }
        public Boolean IsVisible { get;  }

        public void Change(Int32 modelId)
        {
            ModelId = modelId;

            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectModel)}.{nameof(Change)}({nameof(modelId)}: {modelId})");
        }

        public void Hide()
        {
            IsVisible = false;

            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectModel)}.{nameof(Hide)}()");
        }

        public void Show()
        {
            IsVisible = true;

            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectModel)}.{nameof(Show)}()");
        }

        public void SetPosition(WalkmeshCoords position)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectModel)}.{nameof(SetPosition)}({nameof(position)}: {position})");
        }

        public void SetDirection(Degrees angle)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectModel)}.{nameof(SetDirection)}({nameof(angle)}: {angle})");
        }

        public void Rotate(Degrees angle, Int32 frameDuration)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectModel)}.{nameof(Rotate)}({nameof(angle)}: {angle}, {nameof(frameDuration)}: {frameDuration})");
        }

        public void RotateToObject(Int32 targetObject, Int32 frameDuration)
        {
            // TODO: Field script
            // TODO: Implement through Rotate(angle, frameDuration)
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectModel)}.{nameof(RotateToObject)}({nameof(targetObject)}: {targetObject}, {nameof(frameDuration)}: {frameDuration})");
        }

        public void RotateToPlayer(Int32 unknown, Int32 frameDuration)
        {
            // TODO: Field script
            // TODO: Implement through Rotate(angle, frameDuration)
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectModel)}.{nameof(RotateToPlayer)}({nameof(unknown)}: {unknown}, {nameof(frameDuration)}: {frameDuration})");
        }

        public void SetHitbox(Coords3D p1, Coords3D p2)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectModel)}.{nameof(SetHitbox)}({nameof(p1)}: {p1}, {nameof(p2)}: {p2})");
        }
    }
}