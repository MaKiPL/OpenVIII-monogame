using Microsoft.Xna.Framework;
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

        public short CameraHeight { get; private set; }
        public Rectangle[] CamerasRanges { get; private set; }
        public byte ControlDirection { get; private set; }
        public ushort LikePVP { get; private set; }
        public FF8String Name { get; private set; }
        public Rectangle[] ScreenRanges { get; private set; }
        public byte[] Unknown { get; private set; }

        #endregion Properties

        #region Methods

        public static INF Load(byte[] infb)
        {
            if (infb == null || infb.Length == 0) return default;

            INF r = new INF();
            r.ReadData(infb);
            return r;
        }

        private void ReadData(byte[] infb)
        {
            using (BinaryReader br = new BinaryReader(new MemoryStream(infb)))
            {
                Name = new FF8String(br.ReadBytes(9));
                ControlDirection = br.ReadByte();
                Unknown = br.ReadBytes(6);
                LikePVP = br.ReadUInt16();
                CameraHeight = br.ReadInt16();
                CamerasRanges = new Rectangle[8];
                ScreenRanges = new Rectangle[2];
                foreach (int i in Enumerable.Range(0, 8))
                {
                    CamerasRanges[i].Y = br.ReadInt16();
                    CamerasRanges[i].Height = br.ReadInt16();
                    CamerasRanges[i].Width = br.ReadInt16();
                    CamerasRanges[i].X = br.ReadInt16();
                }
                foreach (int i in Enumerable.Range(0, 2))
                {
                    ScreenRanges[i].Y = br.ReadInt16();
                    ScreenRanges[i].Height = br.ReadInt16();
                    ScreenRanges[i].Width = br.ReadInt16();
                    ScreenRanges[i].X = br.ReadInt16();
                }
            }
        }

        #endregion Methods
    }
}