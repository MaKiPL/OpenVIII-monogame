using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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
    public class Cameras : IList<Cameras.Camera>
    {
        #region Fields

        private const int altsizeofCamera = 0x26;
        private const int sizeofCamera = 0x28;
        private List<Camera> cameras;

        #endregion Fields

        #region Constructors

        public Cameras() => this.cameras = new List<Camera>(1);

        #endregion Constructors

        #region Properties

        public int Count => ((IList<Camera>)cameras).Count;
        public bool IsReadOnly => ((IList<Camera>)cameras).IsReadOnly;

        #endregion Properties

        #region Indexers

        public Camera this[int index] { get => ((IList<Camera>)cameras)[index]; set => ((IList<Camera>)cameras)[index] = value; }

        #endregion Indexers

        #region Methods

        public static Cameras Load(byte[] idb)
        {
            if (idb == null || idb.Length == 0) return null;
            Cameras r = new Cameras();

            int _sizeofCamera = Cameras.sizeofCamera;
            int count = 0;
            if (idb.Length / sizeofCamera > idb.Length / altsizeofCamera)
                count = idb.Length / sizeofCamera;
            else
            {
                _sizeofCamera = altsizeofCamera;
                count = idb.Length / _sizeofCamera;
            }
            Debug.Assert(count >= 1);
            for (long offset = 0; offset < count * _sizeofCamera; offset += _sizeofCamera)
                r.Add(Camera.Load(idb, offset));
            return r;
        }

        public void Add(Camera item) => ((IList<Camera>)cameras).Add(item);

        public void Clear() => ((IList<Camera>)cameras).Clear();

        public bool Contains(Camera item) => ((IList<Camera>)cameras).Contains(item);

        public void CopyTo(Camera[] array, int arrayIndex) => ((IList<Camera>)cameras).CopyTo(array, arrayIndex);

        public IEnumerator<Camera> GetEnumerator() => ((IList<Camera>)cameras).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IList<Camera>)cameras).GetEnumerator();

        public int IndexOf(Camera item) => ((IList<Camera>)cameras).IndexOf(item);

        public void Insert(int index, Camera item) => ((IList<Camera>)cameras).Insert(index, item);

        public bool Remove(Camera item) => ((IList<Camera>)cameras).Remove(item);

        public void RemoveAt(int index) => ((IList<Camera>)cameras).RemoveAt(index);

        #endregion Methods

        #region Classes

        /// <summary>
        /// Camera
        /// </summary>
        public class Camera
        {
            #region Fields

            private Vector3 _position;
            private Vector3 space;
            private Vector3[] xyz;

            #endregion Fields

            #region Constructors

            public Camera() => xyz = new Vector3[3];

            #endregion Constructors

            #region Properties

            private uint Blank { get; set; }

            public Vector3 Position { get => _position; private set => _position = value; }

            public Matrix RotationMatrix => new Matrix { Left = xyz[0], Down = xyz[1], Backward = xyz[2] };

            public Vector3[] XYZ { get => xyz; private set => xyz = value; }

            private short Z { get; set; }

            public float Zoom { get; set; }

            private float Zoom2 { get; set; }
            public Vector3 Space { get => space; private set => space = value; }

            #endregion Properties

            #region Methods

            public static Camera Load(byte[] idb, long offset)
            {
                Camera r = new Camera();
                r.ReadData(idb, offset);
                return r;
            }

            public Matrix CreateLookAt() => Matrix.CreateLookAt(Position - xyz[2] * 2, xyz[2] + Position, xyz[1]);

            public Matrix CreateWorld() => Matrix.CreateWorld(Position, xyz[2], xyz[1]);

            private void ReadData(byte[] cab, long offset)
            {
                using (BinaryReader br = new BinaryReader(new MemoryStream(cab)))
                {
                    br.BaseStream.Seek(offset, SeekOrigin.Begin);
                    const float multipleconstant = 4096f;
                    for (int i = 0; i < 3; i++)
                    {
                        xyz[i].X = br.ReadInt16() / multipleconstant;
                        xyz[i].Y = br.ReadInt16() / multipleconstant;
                        xyz[i].Z = br.ReadInt16() / multipleconstant;
                    }
                    Z = br.ReadInt16();
                    space.X = br.ReadInt32();
                    space.Y = br.ReadInt32();
                    space.Z = br.ReadInt32();
                    Blank = br.ReadUInt16();
                    Zoom = br.ReadUInt16();
                    Zoom2 = br.ReadUInt16();
                    Zoom = Zoom2 = Math.Max(Zoom, Zoom2);
                    Matrix r = RotationMatrix;
                    space /= 4096f;
                    _position = -(space * xyz[0] + space * xyz[1] + space * xyz[2]);
                    //_position = -(space * r.Right + space * r.Up + space * r.Backward);
                    //_position = Vector3.Transform(space/4096f, r);
                    //_position = new Vector3
                    //{
                    //    X = -(space.X * xyz[0].X + space.X * xyz[1].X + space.X * xyz[2].X),
                    //    Y = -(space.Y * xyz[0].Y + space.Y * xyz[1].Y + space.Y * xyz[2].Y),
                    //    Z = -(space.Z * xyz[0].Z + space.Z * xyz[1].Z + space.Z * xyz[2].Z),
                    //};
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}