using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Battle.Dat
{
    /// <summary>
    /// Section 3c: Model animation frames container
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/FileFormat_DAT#Animation"/>
    public struct Animation : IReadOnlyList<AnimationFrame>
    {
        #region Fields

        private readonly IReadOnlyList<AnimationFrame> _animationFrames;

        #endregion Fields

        #region Constructors

        public Animation(BinaryReader br, Skeleton skeleton) : this()
        {
            byte cFrames = br.ReadByte();
            _animationFrames = AnimationFrame.CreateInstances(br, cFrames, skeleton);
        }

        #endregion Constructors

        #region Properties

        public int Count => _animationFrames.Count;

        #endregion Properties

        #region Indexers

        public AnimationFrame this[int index] => _animationFrames[index];

        #endregion Indexers

        #region Methods

        public static Animation CreateInstance(BinaryReader br, long byteOffset, Skeleton skeleton)
        {
            br.BaseStream.Seek(byteOffset, SeekOrigin.Begin);
            return new Animation(br, skeleton);
        }

        public static IReadOnlyList<Animation> CreateInstances(BinaryReader br, IEnumerable<uint> pAnimations, Skeleton skeleton) => pAnimations.Select(x => CreateInstance(br, x, skeleton)).ToList().AsReadOnly();

        public IEnumerator<AnimationFrame> GetEnumerator() => _animationFrames.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_animationFrames).GetEnumerator();

        #endregion Methods
    }
}