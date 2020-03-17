using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using OpenVIII.AV.Midi;

namespace OpenVIII.Battle.Dat
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Animation
    {
        #region Fields

        public readonly byte CFrames;
        public readonly IReadOnlyList<AnimationFrame> AnimationFrames;

        public Animation(BinaryReader br,Skeleton skeleton) : this()
        {
            CFrames = br.ReadByte();
            AnimationFrames = AnimationFrame.CreateInstances(br, CFrames, skeleton);
        }

        #endregion Fields

        public static Animation CreateInstance(BinaryReader br, long byteOffset,Skeleton skeleton)
        {
            br.BaseStream.Seek(byteOffset, SeekOrigin.Begin);
            return new Animation(br, skeleton);
        }
        public static IReadOnlyList<Animation> CreateInstances(BinaryReader br, IEnumerable<uint> pAnimations, Skeleton skeleton)
        {
            return pAnimations.Select(x => CreateInstance(br, x,skeleton)).ToList().AsReadOnly();
        }
    }
}