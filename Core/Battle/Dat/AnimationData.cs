using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    /// <summary>
    /// Section 3a: Model animation - Header
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/FileFormat_DAT#Header_.28data_sub_table.29_2"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct AnimationData
    {
        #region Fields

        public readonly int CAnimations;
        public readonly IReadOnlyList<uint> PAnimations;
        public readonly IReadOnlyList<Animation> Animations;

        public AnimationData(BinaryReader br, long byteOffset,Skeleton skeleton) : this()
        {
            CAnimations = br.ReadInt32();
            PAnimations = Enumerable.Range(0, CAnimations).Select(_ => checked((uint)(byteOffset + br.ReadUInt32()))).ToList()
                .AsReadOnly();
            Animations = Animation.CreateInstances(br, PAnimations, skeleton);
        }

        #endregion Fields

        public static AnimationData CreateInstance(BinaryReader br, long byteOffset,Skeleton skeleton)
        {
            br.BaseStream.Seek(byteOffset, SeekOrigin.Begin);
            return new AnimationData(br,byteOffset,skeleton);
        }

    }
}