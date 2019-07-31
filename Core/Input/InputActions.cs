using OpenVIII.Encoding.Tags;
using System.Collections.Generic;

namespace OpenVIII
{
    public static class InputActions
    {
        #region Fields

        public static readonly List<FF8TextTagKey> Cancel = new List<FF8TextTagKey> { FF8TextTagKey.Cancel, FF8TextTagKey.x34 };
        public static readonly List<FF8TextTagKey> Cards = new List<FF8TextTagKey> { FF8TextTagKey.Cards, FF8TextTagKey.x37 };
        public static readonly List<FF8TextTagKey> Confirm = new List<FF8TextTagKey> { FF8TextTagKey.Confirm, FF8TextTagKey.x36 };
        public static readonly List<FF8TextTagKey> Down = new List<FF8TextTagKey> { FF8TextTagKey.Down, FF8TextTagKey.x3E };
        public static readonly List<FF8TextTagKey> EscapeLeft = new List<FF8TextTagKey> { FF8TextTagKey.EscapeLeft, FF8TextTagKey.x30 };
        public static readonly List<FF8TextTagKey> EscapeRight = new List<FF8TextTagKey> { FF8TextTagKey.EscapeRight, FF8TextTagKey.x31 };
        public static readonly List<FF8TextTagKey> Exit = new List<FF8TextTagKey> { FF8TextTagKey.Exit };
        public static readonly List<FF8TextTagKey> ExitMenu = new List<FF8TextTagKey> { FF8TextTagKey.ExitMenu };
        public static readonly List<FF8TextTagKey> Left = new List<FF8TextTagKey> { FF8TextTagKey.Left, FF8TextTagKey.x3F };
        public static readonly List<FF8TextTagKey> Menu = new List<FF8TextTagKey> { FF8TextTagKey.Menu, FF8TextTagKey.x35 };
        public static readonly List<FF8TextTagKey> Pause = new List<FF8TextTagKey> { FF8TextTagKey.Pause, FF8TextTagKey.x3B };
        public static readonly List<FF8TextTagKey> Reset = new List<FF8TextTagKey> { FF8TextTagKey.Reset };
        public static readonly List<FF8TextTagKey> Right = new List<FF8TextTagKey> { FF8TextTagKey.Right, FF8TextTagKey.x3D };
        public static readonly List<FF8TextTagKey> RotateLeft = new List<FF8TextTagKey> { FF8TextTagKey.RotateLeft, FF8TextTagKey.x32 };
        public static readonly List<FF8TextTagKey> RotateRight = new List<FF8TextTagKey> { FF8TextTagKey.RotateRight, FF8TextTagKey.x33 };
        public static readonly List<FF8TextTagKey> Select = new List<FF8TextTagKey> { FF8TextTagKey.Select, FF8TextTagKey.x38 };
        public static readonly List<FF8TextTagKey> Up = new List<FF8TextTagKey> { FF8TextTagKey.Up, FF8TextTagKey.x3C };

        #endregion Fields
    }
}