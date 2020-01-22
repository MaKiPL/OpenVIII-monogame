using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII.Battle
{
    public class Encounters : IList<Encounter>, IReadOnlyList<Encounter>, IEnumerable<Encounter>, IReadOnlyCollection<Encounter>, ICollection, IEnumerable
    {
        #region Fields

        private List<Encounter> encounters;

        #endregion Fields

        #region Constructors

        public Encounters(int count = 0) => encounters = new List<Encounter>(count);

        #endregion Constructors

        #region Properties

        public int Count => ((IList)encounters).Count;
        public int CurrentIndex { get; set; }
        public bool IsFixedSize => ((IList)encounters).IsFixedSize;
        public bool IsReadOnly => ((IList)encounters).IsReadOnly;
        public bool IsSynchronized => ((IList)encounters).IsSynchronized;
        public object SyncRoot => ((IList)encounters).SyncRoot;

        #endregion Properties

        #region Indexers

        public Encounter this[int index] => ((IReadOnlyList<Encounter>)encounters)[index];

        Encounter IList<Encounter>.this[int index] { get => ((IList<Encounter>)encounters)[index]; set => ((IList<Encounter>)encounters)[index] = value; }

        #endregion Indexers

        #region Methods

        public static Encounters Read(byte[] enc)
        {
            Memory.Log.WriteLine($"{nameof(Init_debugger_battle)} :: {nameof(Encounters)} :: {nameof(Read)}");
            if (enc == null || enc.Length == 0) return null;
            int encounterCount = enc.Length / 128;
            Encounters encounters = new Encounters(encounterCount);

            MemoryStream ms = null;

            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(enc)))
            {
                for (int i = 0; i < encounterCount; i++)
                    encounters.Add(new Encounter()
                    {
                        Scenario = br.ReadByte(),
                        BattleFlags = (EncounterFlag)br.ReadByte(),
                        PrimaryCamera = br.ReadByte(),
                        AlternativeCamera = br.ReadByte(),
                        HiddenEnemies = br.ReadByte(),
                        UnloadedEnemy = br.ReadByte(),
                        UntargetableEnemy = br.ReadByte(),
                        EnabledEnemy = br.ReadByte(),
                        enemyCoordinates = EnemyCoordinates.Read(br),
                        BEnemies = br.ReadBytes(8),
                        bUnk2 = br.ReadBytes(16 * 3 + 8),
                        bLevels = br.ReadBytes(8)
                    });
                ms = null;
            }
            return encounters;
        }

        public void Add(Encounter item) => ((IList<Encounter>)encounters).Add(item);

        public void Clear() => ((IList)encounters).Clear();

        public bool Contains(Encounter item) => ((IList<Encounter>)encounters).Contains(item);

        public void CopyTo(Array array, int index) => ((IList)encounters).CopyTo(array, index);

        public void CopyTo(Encounter[] array, int arrayIndex) => ((IList<Encounter>)encounters).CopyTo(array, arrayIndex);

        public Encounter Current(int? index = null)
        {
            if (index.HasValue)
                CurrentIndex = index.Value;
            return encounters[CurrentIndex];
        }

        public IEnumerator GetEnumerator() => ((IList)encounters).GetEnumerator();

        IEnumerator<Encounter> IEnumerable<Encounter>.GetEnumerator() => ((IList<Encounter>)encounters).GetEnumerator();

        public int IndexOf(Encounter item) => ((IList<Encounter>)encounters).IndexOf(item);

        public void Insert(int index, Encounter item) => ((IList<Encounter>)encounters).Insert(index, item);

        public Encounter Next()
        {
            CurrentIndex++;
            if (CurrentIndex >= encounters.Count)
                CurrentIndex = 0;
            return Current();
        }

        public Encounter Previous()
        {
            CurrentIndex--;
            if (CurrentIndex < 0)
                CurrentIndex = encounters.Count - 1;
            return Current();
        }

        public bool Remove(Encounter item) => ((IList<Encounter>)encounters).Remove(item);

        public void RemoveAt(int index) => ((IList)encounters).RemoveAt(index);

        #endregion Methods
    }
}