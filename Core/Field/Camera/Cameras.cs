using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenVIII.Fields
{
    /// <summary>
    /// Cameras
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF7/Field/Camera_Matrix"/>
    /// <seealso cref="https://github.com/myst6re/deling/blob/master/files/CaFile.cpp"/>
    /// <seealso cref="https://github.com/myst6re/deling/blob/master/files/CaFile.h"/>
    /// <seealso cref="https://github.com/myst6re/deling/blob/master/WalkmeshGLWidget.cpp"/>
    /// <seealso cref="https://github.com/myst6re/deling/blob/master/WalkmeshGLWidget.h"/>
    public class Cameras : IReadOnlyList<Camera>
    {
        #region Fields

        private const int AltSizeOfCamera = 0x26;
        private const int SizeOfCamera = 0x28;
        private readonly IReadOnlyList<Camera> _cameras;

        #endregion Fields

        #region Constructors

        public Cameras(IReadOnlyList<Camera> cameras) => _cameras = cameras;

        #endregion Constructors

        #region Properties

        public int Count => _cameras.Count;

        #endregion Properties

        #region Indexers

        public Camera this[int index] => _cameras[index];

        #endregion Indexers

        #region Methods

        public static Cameras CreateInstance(byte[] idb)
        {
            if (idb == null || idb.Length == 0) return null;

            var sizeofCamera = SizeOfCamera;
            int count;
            if (idb.Length / SizeOfCamera > idb.Length / AltSizeOfCamera)
                count = idb.Length / SizeOfCamera;
            else
            {
                sizeofCamera = AltSizeOfCamera;
                count = idb.Length / sizeofCamera;
            }
            Debug.Assert(count >= 1);
            return new Cameras(Enumerable.Range(0, count).Select(x => Camera.CreateInstance(idb, x * sizeofCamera)).ToList()
                .AsReadOnly());
        }

        public IEnumerator<Camera> GetEnumerator() => _cameras.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_cameras).GetEnumerator();

        #endregion Methods
    }
}