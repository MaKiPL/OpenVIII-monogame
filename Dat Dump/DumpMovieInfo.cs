using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenVIII.Battle;
using OpenVIII.Fields;
using OpenVIII.Fields.Scripts.Instructions;
using OpenVIII.World;
using static OpenVIII.Saves;
using Sections = OpenVIII.Fields.Sections;

namespace OpenVIII.Dat_Dump
{
    internal static class DumpMovieInfo
    {
        #region Fields

        public static ConcurrentDictionary<int, Archive> FieldData;
        private static HashSet<KeyValuePair<string, MOVIEREADY>> _fieldsWithMovieScripts;
        private static HashSet<ushort> _worldEncounters;

        #endregion Fields

        #region Properties
        private static string Ls => CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        #endregion Properties

        #region Methods

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        internal static async Task Process()
        {
            await Task.WhenAll(LoadFields());
         


            using (var csvFile = new StreamWriter(new FileStream("MovieInfo.csv", FileMode.Create, FileAccess.Write, FileShare.ReadWrite), System.Text.Encoding.UTF8))
            {
                var header =
                $"{nameof(Fields)}{Ls}"+
                $"MovieId{Ls}"+
                $"MovieFlag{Ls}";
                csvFile.WriteLine(header);
                foreach (var pair in _fieldsWithMovieScripts)
                {
                    var field_name = pair.Key;
                    var movie_id = pair.Value.MovieId;
                    var flag = pair.Value.Flag;
                    //check encounters in fields and confirm encounter rate is above 0.

                    csvFile.WriteLine($"{field_name}{Ls}{movie_id}{Ls}{flag}{Ls}");
                }
            }
        }

        private static async Task LoadFields()
        {
            if (FieldData == null)
            {
                FieldData = new ConcurrentDictionary<int, Archive>();

                var tasks = new Task[Memory.FieldHolder.Fields.Length];
                void process(ushort i)
                {
                    if (FieldData.ContainsKey(i)) return;
                    var archive = Archive.Load(i, Sections.MRT | Sections.RAT | Sections.JSM | Sections.SYM);

                    if (archive != null)
                        FieldData.TryAdd(i, archive);
                }
                //foreach (var i1 in Enumerable.Range(0, Memory.FieldHolder.Fields.Length))
                //{
                //    var j = (ushort)i1;
                //    await Task.Run(() => process(j));
                //}
                foreach (var i1 in Enumerable.Range(0, Memory.FieldHolder.Fields.Length))
                {
                    var j = (ushort)i1;
                    tasks[j] = (Task.Run(() => process(j)));
                }
                await Task.WhenAll(tasks);
            }

            _fieldsWithMovieScripts =
            (from fieldArchive in FieldData
             where fieldArchive.Value.JSMObjects != null && fieldArchive.Value.JSMObjects.Count > 0
             from jsmObject in fieldArchive.Value.JSMObjects
             from script in jsmObject.Scripts
             from instruction in script.Segment.Flatten()
             where instruction is MOVIEREADY
             let movie = ((MOVIEREADY)instruction)
             select (new KeyValuePair<string, MOVIEREADY>(fieldArchive.Value.FileName, movie))).ToHashSet();
        }

        #endregion Methods
    }
}