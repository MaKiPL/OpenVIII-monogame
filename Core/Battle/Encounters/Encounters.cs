using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Battle
{
    public class Encounters : IReadOnlyList<Encounter>, IEnumerable<Encounter>, IReadOnlyCollection<Encounter>
    {
        #region Fields

        private List<Encounter> encounters;
        private int _currentIndex;
        private int encounterCount;

        public Encounters(int encounterCount) => encounters = new List<Encounter>(encounterCount);

        #endregion Fields

        #region Constructors


        #endregion Constructors

        #region Properties

        #endregion Properties

        #region Indexers

        #endregion Indexers

        #region Methods

        public static Encounters Read(byte[] enc)
        {
            Memory.Log.WriteLine($"{nameof(InitDebuggerBattle)} :: {nameof(Encounters)} :: {nameof(Read)}");
            if (enc == null || enc.Length == 0) return null;
            int encounterCount = enc.Length / 128;
            Encounters e = new Encounters(encounterCount);

            MemoryStream ms = null;

            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(enc)))
            {
                for (int i = 0; i < encounterCount; i++)
                    e.encounters.Add(Encounter.Read(br,e.Count));


                ms = null;
            }
            e.encounters = e.OrderBy(x => x.Scenario).ThenBy(x => x.ID).ToList();
            return e;
        }

   
        public Encounter Current => encounters[_currentIndex];

      

        public Encounter Next()
        {
            _currentIndex++;
            if (_currentIndex >= encounters.Count)
                _currentIndex = 0;
            return Current;
        }

        public Encounter Previous()
        {
            _currentIndex--;
            if (_currentIndex < 0)
                _currentIndex = encounters.Count - 1;
            return Current;
        }
        public int ID { get => Current.ID; set => _currentIndex = encounters.FindIndex(x => x.ID == value); }
        public byte AlternativeCamera => Current.AlternativeCamera;
        public EncounterFlag BattleFlags => Current.BattleFlags;
        public BitArray EnabledEnemy => Current.EnabledEnemy;
        public EnemyCoordinates enemyCoordinates => Current.EnemyCoordinates;
        public BitArray HiddenEnemies => Current.HiddenEnemies;
        public byte PrimaryCamera => Current.PrimaryCamera;
        public byte Scenario => Current.Scenario;
        public BitArray UnloadedEnemy => Current.UnloadedEnemy;
        public BitArray UntargetableEnemy => Current.UntargetableEnemy;
        public byte[] BEnemies => Current.BEnemies;
        public string Filename => Current.Filename;

        public int Count => ((IReadOnlyList<Encounter>)encounters).Count;

        public Encounter this[int index] => ((IReadOnlyList<Encounter>)encounters)[index];

        public int ResolveCameraAnimation(byte cameraPointerValue) => Current.ResolveCameraAnimation(cameraPointerValue);
        public int ResolveCameraSet(byte cameraPointerValue) => Current.ResolveCameraSet(cameraPointerValue);
        public IEnumerator<Encounter> GetEnumerator() => ((IReadOnlyList<Encounter>)encounters).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyList<Encounter>)encounters).GetEnumerator();



        #endregion Methods
    }
}