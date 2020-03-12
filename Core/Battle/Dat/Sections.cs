using System;

namespace OpenVIII.Battle.Dat
{
    [Flags]
    public enum Sections : ushort
    {
        None = 0,

        /// <summary>
        /// Section 1
        /// </summary>
        Skeleton = 0x1,

        /// <summary>
        /// Section 2
        /// </summary>
        /// <remarks>Requires Skeleton</remarks>
        ModelGeometry = 0x2 | Skeleton,

        /// <summary>
        /// Section 3
        /// </summary>
        /// <remarks>Requires Model_Geometry</remarks>
        ModelAnimation = 0x4 | ModelGeometry,

        /// <summary>
        /// Section 4
        /// </summary>
        Section4Unknown = 0x8,

        /// <summary>
        /// Section 5
        /// </summary>
        AnimationSequences = 0x10,

        /// <summary>
        /// Section 6
        /// </summary>
        Section6Unknown = 0x20,

        /// <summary>
        /// Section 7
        /// </summary>
        Information = 0x40,

        /// <summary>
        /// Section 8
        /// </summary>
        Scripts = 0x80,

        /// <summary>
        /// Section 9
        /// </summary>
        Sounds = 0x100,

        /// <summary>
        /// Section 10
        /// </summary>
        SoundsUnknown = 0x200,

        /// <summary>
        /// Section 11
        /// </summary>
        Textures = 0x400,

        /// <summary>
        /// All Sections
        /// </summary>
        All = 0x7FF,
    }
}