using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII.World
{
    /// <summary>
    /// wm2field.tbl = World Map to Field table = helper class that determines the X Y and Z position and also fieldId to warp from field. This file
    /// does not contain the X Y and Z coordinates for world map!
    /// </summary>
    class wm2field
    {
        [StructLayout(LayoutKind.Sequential, Pack =1, Size =24)]
        public struct warpEntry
        {
            public short fieldX;
            public short fieldY;
            public ushort fieldZ;
            public ushort fieldId;
            public byte unk1;
            public byte unk2;
            public byte unk3;
            public byte unk4;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] padd;
        }

        public warpEntry[] warpEntries;

        public wm2field(byte[] buffer)
        {
            int structSize = Marshal.SizeOf(typeof(warpEntry));
            warpEntries = new warpEntry[buffer.Length / structSize];
            for (int i = 0; i < warpEntries.Length; i++)
                warpEntries[i] = Extended.ByteArrayToStructure<warpEntry>(buffer.Skip(i * structSize).Take(structSize).ToArray());
        }

        public Vector3 GetFieldPosition(int entryId) => new Vector3(
            warpEntries[entryId].fieldX,
            warpEntries[entryId].fieldY,
            warpEntries[entryId].fieldZ);

        public int GetFieldId(int entryId) => warpEntries[entryId].fieldId;
    }
}
