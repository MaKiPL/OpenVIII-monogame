using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Battle.Dat
{
    /// <summary>
    /// Section 3a: Model animation - Header
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/FileFormat_DAT#Header_.28data_sub_table.29_2"/>
    public struct AnimationData : IReadOnlyList<Animation>
    {
        #region Fields

        private readonly IReadOnlyList<Animation> _animations;

        #endregion Fields

        #region Constructors

        public AnimationData(BinaryReader br, long byteOffset, Skeleton skeleton) : this()
        {
            int cAnimations = br.ReadInt32();
            IReadOnlyList<uint> pAnimations = Enumerable.Range(0, cAnimations).Select(_ => checked((uint)(byteOffset + br.ReadUInt32()))).ToList()
                .AsReadOnly();
            _animations = Animation.CreateInstances(br, pAnimations, skeleton);
        }

        #endregion Constructors

        #region Properties

        public int Count => _animations?.Count ?? 0;

        #endregion Properties

        #region Indexers

        public Animation this[int index] => _animations[index];

        #endregion Indexers

        #region Methods

        public static AnimationData CreateInstance(BinaryReader br, long byteOffset, Skeleton skeleton)
        {
            br.BaseStream.Seek(byteOffset, SeekOrigin.Begin);
            return new AnimationData(br, byteOffset, skeleton);
        }

        public IEnumerator<Animation> GetEnumerator() => _animations.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_animations).GetEnumerator();

        #endregion Methods
    }
}