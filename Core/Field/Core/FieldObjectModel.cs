﻿using System;

namespace OpenVIII.Fields
{
    public sealed class FieldObjectModel
    {
        public int ModelID { get; set; }
        public bool IsVisible { get; set; }

        public void Change(int modelID)
        {
            ModelID = modelID;

            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectModel)}.{nameof(Change)}({nameof(modelID)}: {modelID})");
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

        public void Rotate(Degrees angle, int frameDuration)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectModel)}.{nameof(Rotate)}({nameof(angle)}: {angle}, {nameof(frameDuration)}: {frameDuration})");
        }

        public void RotateToObject(int targetObject, int frameDuration)
        {
            // TODO: Field script
            // TODO: Implement through Rotate(angle, frameDuration)
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectModel)}.{nameof(RotateToObject)}({nameof(targetObject)}: {targetObject}, {nameof(frameDuration)}: {frameDuration})");
        }

        public void RotateToPlayer(int unknown, int frameDuration)
        {
            // TODO: Field script
            // TODO: Implement through Rotate(angle, frameDuration)
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectModel)}.{nameof(RotateToPlayer)}({nameof(unknown)}: {unknown}, {nameof(frameDuration)}: {frameDuration})");
        }

        public void SetHitBox(Coords3D p1, Coords3D p2)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectModel)}.{nameof(SetHitBox)}({nameof(p1)}: {p1}, {nameof(p2)}: {p2})");
        }
    }
}