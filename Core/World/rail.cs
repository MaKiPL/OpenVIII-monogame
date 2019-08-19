using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII.Core.World
{
    /// <summary>
    /// Rail.obj is a file that contains waypoints and stops for trains including sequences
    /// </summary>
    class rail
    {
        private const int RAIL_BLOCK_SIZE = 2048;
        private const int BLOCK_HEADER_SIZE = 12;

        [StructLayout(LayoutKind.Sequential, Pack =1, Size =8)]
        private struct RailEntry
        {
            public byte cKeypoints;
            public byte unk;
            public ushort unk2;
            public uint trainStop1;
            public uint trainStop2;
            public Keypoint[] keypoints;

            public Keypoint TrainStop1 => keypoints[trainStop1];
            public Keypoint TrainStop2 => keypoints[trainStop2];
        }
        /// <summary>
        /// to reverse, but probably should be finally changed to vec4- it's normal vector, but relative to world coordinates of vanilla world map
        /// </summary>
        /// 
        [StructLayout(LayoutKind.Sequential, Pack =1, Size =16)]
        private struct Keypoint
        {
            public int x;
            public int y;
            private int z;
            public int padd;

            public int Z => -z;
        }

        RailEntry[] railEntries;

        public rail(byte[] buffer)
        {
            int railEntries = buffer.Length / RAIL_BLOCK_SIZE;
            this.railEntries = new RailEntry[railEntries];
            using(MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
                for (int i = 0; i < railEntries; i++)
                    this.railEntries[i] = ParseBlock(br.ReadBytes(2048));

        }

        private RailEntry ParseBlock(byte[] block)
        {
            RailEntry entry = new RailEntry()
            {
                cKeypoints = block[0],
                unk = block[1],
                unk2 = BitConverter.ToUInt16(block, 2),
                trainStop1 = BitConverter.ToUInt32(block, 4),
                trainStop2 = BitConverter.ToUInt32(block, 8)
            };
            entry.keypoints = new Keypoint[entry.cKeypoints];
            for(int i = 0; i<entry.cKeypoints; i++)
                entry.keypoints[i] = Extended.ByteArrayToStructure<Keypoint>(block.Skip(BLOCK_HEADER_SIZE+(i*16)).Take(16).ToArray());
            return entry;
        }

        /// <summary>
        /// Gets count of all keypoints associated with provided trackId
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        public int GetTrainTrackFrameCount(int trackId) => railEntries[trackId].keypoints.Length;
        /// <summary>
        /// Gets count of all available tracks
        /// </summary>
        /// <returns></returns>
        public int GetTrainTrackCount() => railEntries.Length;
        /// <summary>
        /// Gets Vector3 translated to openviii coordinates from given track animation's frame id
        /// </summary>
        /// <param name="trackId"></param>
        /// <param name="frameId"></param>
        /// <returns></returns>
        public Vector3 GetTrackFrameVector(int trackId, int frameId)
        {
            Keypoint kp = railEntries[trackId].keypoints[frameId];
            return new Vector3(Extended.ConvertVanillaWorldXAxisToOpenVIII(kp.x),
                               Extended.ConvertVanillaWorldYAxisToOpenVIII(kp.y),
                               Extended.ConvertVanillaWorldZAxisToOpenVIII(kp.Z)
                               );
        }
    }
}
