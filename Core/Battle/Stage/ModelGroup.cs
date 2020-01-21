using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        #region Classes

        private class ModelGroup : IList
        {
            #region Fields

            private List<Model> models;

            #endregion Fields

            #region Constructors

            public ModelGroup(int count) => this.models = new List<Model>(count);

            #endregion Constructors

            #region Properties

            public int Count => ((IList)models).Count;
            public bool IsFixedSize => ((IList)models).IsFixedSize;
            public bool IsReadOnly => ((IList)models).IsReadOnly;
            public bool IsSynchronized => ((IList)models).IsSynchronized;
            public object SyncRoot => ((IList)models).SyncRoot;

            #endregion Properties

            #region Indexers

            public object this[int index] { get => ((IList)models)[index]; set => ((IList)models)[index] = value; }

            #endregion Indexers

            #region Methods

            public int Add(object value) => ((IList)models).Add(value);

            public void Clear() => ((IList)models).Clear();

            public bool Contains(object value) => ((IList)models).Contains(value);

            public void CopyTo(Array array, int index) => ((IList)models).CopyTo(array, index);

            public IEnumerator GetEnumerator() => ((IList)models).GetEnumerator();

            public int IndexOf(object value) => ((IList)models).IndexOf(value);

            public void Insert(int index, object value) => ((IList)models).Insert(index, value);

            public void Remove(object value) => ((IList)models).Remove(value);

            public void RemoveAt(int index) => ((IList)models).RemoveAt(index);

            /// <summary>
            /// Reads Stage model groups pointers and reads/parses them individually. Group0 is stage
            /// ground. It's always enabled except special sequences like GFs Group1 is main geometry.
            /// It's prior to Time Compression deformation Group2 is main/additional geometry. It's prior
            /// to Time Compression deformation Group3 is Sky. It's NON-prior to Time Compression, but
            /// may be modified by SkyRotators and/or TimeCompression last Stage skyRotation multiplier
            /// </summary>
            /// <param name="pointer"></param>
            /// <returns></returns>
            public static ModelGroup Read(uint pointer, BinaryReader br)
            {
                br.BaseStream.Seek(pointer, SeekOrigin.Begin);
                int modelsCount = br.ReadInt32();
                ModelGroup models = new ModelGroup(modelsCount);
                uint[] modelPointers = new uint[modelsCount];
                for (int i = 0; i < modelsCount; i++)
                    modelPointers[i] = pointer + br.ReadUInt32();
                for (int i = 0; i < modelsCount; i++)
                    models.Add(Model.Read(modelPointers[i], br));
                return models;
            }

            #endregion Methods
        }

        #endregion Classes
    }
}