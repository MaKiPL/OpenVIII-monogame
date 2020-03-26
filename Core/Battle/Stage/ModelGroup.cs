using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        #region Classes

        private class ModelGroups : IEnumerable<ModelGroup>, IReadOnlyList<ModelGroup>, IEnumerable
        {
            private List<ModelGroup> modelgroups;

            public ModelGroups(params ModelGroup[] modelgroups) => this.modelgroups = new List<ModelGroup>(modelgroups);

            public ModelGroup this[int index] => ((IReadOnlyList<ModelGroup>)modelgroups)[index];

            public int Count => ((IReadOnlyList<ModelGroup>)modelgroups).Count;

            public IEnumerator<ModelGroup> GetEnumerator() => ((IEnumerable<ModelGroup>)modelgroups).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ModelGroup>)modelgroups).GetEnumerator();
        }

        private class ModelGroup : IEnumerable<Model>, IReadOnlyList<Model>
        {
            #region Fields

            private List<Model> models;

            #endregion Fields

            #region Constructors

            public ModelGroup(int count) => this.models = new List<Model>(count);

            #endregion Constructors

            #region Properties

            public int Count => ((IReadOnlyList<Model>)models).Count;

            #endregion Properties

            #region Indexers

            public Model this[int index] => ((IReadOnlyList<Model>)models)[index];

            #endregion Indexers

            #region Methods

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
                var modelsCount = br.ReadInt32();
                var models = new ModelGroup(modelsCount);
                var modelPointers = new uint[modelsCount];
                for (var i = 0; i < modelsCount; i++)
                    modelPointers[i] = pointer + br.ReadUInt32();
                for (var i = 0; i < modelsCount; i++)
                    models.models.Add(Model.Read(modelPointers[i], br));
                return models;
            }

            public IEnumerator<Model> GetEnumerator() => ((IEnumerable<Model>)models).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Model>)models).GetEnumerator();

            #endregion Methods
        }

        #endregion Classes
    }
}