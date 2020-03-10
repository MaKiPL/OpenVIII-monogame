using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenVIII.Fields
{
    /// <summary>
    /// Gateways, Triggers, Camera Limits
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_INF"/>
    public class INF
    {
        #region Properties

        public short CameraHeight { get; set; }
        public Rectangle[] CamerasRanges { get; set; }
        public byte ControlDirection { get; set; }
        public Gateways Gateways { get; set; }
        public ushort LikePVP { get; set; }
        public FF8String Name { get; set; }
        public Rectangle[] ScreenRanges { get; set; }
        public Triggers Triggers { get; set; }
        public int Type { get; set; }
        public byte[] Unknown { get; set; }

        #endregion Properties

        #region Methods

        public static INF Load(byte[] inf)
        {
            if (inf == null || inf.Length == 0) return default;

            INF r = new INF();
            r.ReadData(inf);
            return r;
        }

        private void ReadData(byte[] inf)
        {
            using (BinaryReader br = new BinaryReader(new MemoryStream(inf)))
            {
                switch (inf.Length)
                {
                    case 676:
                        Type = 0;
                        break;

                    case 672:
                        Type = 1;
                        break;

                    case 576:
                        Type = 2;
                        break;

                    case 504:
                        Type = 3;
                        break;

                    default:
                        throw new Exception("unknown format INF");
                }
                Name = new FF8String(br.ReadBytes(9));
                ControlDirection = br.ReadByte();
                if (Type == 0)
                {
                    Unknown = br.ReadBytes(6);
                    LikePVP = br.ReadUInt16();
                }
                else if (Type >= 1)
                {
                    Unknown = br.ReadBytes(4);
                }
                CameraHeight = br.ReadInt16();
                CamerasRanges = new Rectangle[Type <= 2 ? 8 : 1];
                foreach (int i in Enumerable.Range(0, CamerasRanges.Length))
                {
                    CamerasRanges[i].Y = br.ReadInt16();
                    CamerasRanges[i].Height = br.ReadInt16();
                    CamerasRanges[i].Width = br.ReadInt16();
                    CamerasRanges[i].X = br.ReadInt16();
                    CamerasRanges[i].Height -= CamerasRanges[i].Y;
                    CamerasRanges[i].Width -= CamerasRanges[i].X;
                }
                if (Type <= 2)
                {
                    ScreenRanges = new Rectangle[2];
                    foreach (int i in Enumerable.Range(0, ScreenRanges.Length))
                    {
                        ScreenRanges[i].Y = br.ReadInt16();
                        ScreenRanges[i].Height = br.ReadInt16();
                        ScreenRanges[i].Width = br.ReadInt16();
                        ScreenRanges[i].X = br.ReadInt16();

                        ScreenRanges[i].Height -= ScreenRanges[i].Y;
                        ScreenRanges[i].Width -= ScreenRanges[i].X;
                    }
                }
                Gateways = Gateways.Read(br, Type);
                Triggers = Triggers.Read(br);
                Debug.Assert(br.BaseStream.Length == br.BaseStream.Position);
            }
        }

        #endregion Methods
    }
}