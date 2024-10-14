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
    internal class MovieScriptInfo
    {
        public ushort MovieID { get; }
        public ushort MovieFlag { get; }
        public int MoviePosition { get; }
        public int MovieSyncPosition { get; }

        public MovieScriptInfo(MOVIEREADY movie, int moviePosition, int syncPosition)
        {
            MovieID = movie.MovieId;
            MovieFlag = movie.Flag;
            MoviePosition = moviePosition;
            MovieSyncPosition = syncPosition;
        }
    }

    internal static class DumpMovieInfo
    {
        #region Fields

        public static ConcurrentDictionary<int, Archive> FieldData;

        #endregion Fields

        #region Properties
        private static string Ls => CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        #endregion Properties

        #region Methods

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        internal static async Task Process()
        {
            await LoadFields();
            using (var csvFile = new StreamWriter(new FileStream("MovieInfo.csv", FileMode.Create, FileAccess.Write, FileShare.ReadWrite), System.Text.Encoding.UTF8))
            {
                var header =
                $"{nameof(Fields)}{Ls}"+
                $"MovieId{Ls}"+
                $"MovieFlag{Ls}" +
                $"MoviePosition{Ls}" +
                $"MovieSyncPosition{Ls}" +
                $"MoviePositionDifference{Ls}";
                await csvFile.WriteLineAsync(header);
                foreach (var pair in GroupFieldsWithMovies())
                {
                    var field_name = pair.Key;

                    //check encounters in fields and confirm encounter rate is above 0.

                    var movie_info = pair.Value;

                    await csvFile.WriteLineAsync($"{field_name}{Ls}" +
                        $"{movie_info.MovieID}{Ls}" +
                        $"{movie_info.MovieFlag}{Ls}" +
                        $"{movie_info.MoviePosition}{Ls}" +
                        $"{movie_info.MovieSyncPosition}{Ls}"+
                        $"{movie_info.MovieSyncPosition - movie_info.MoviePosition}{Ls}");
                    
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
        }

            internal static IEnumerable<KeyValuePair<string, MovieScriptInfo>> GroupFieldsWithMovies()
        {
            foreach (var fieldArchive in FieldData)
            {
                if (fieldArchive.Value.JSMObjects == null || fieldArchive.Value.JSMObjects.Count == 0)
                    continue;

                foreach (var jsmObject in fieldArchive.Value.JSMObjects)
                {
                    foreach (var script in jsmObject.Scripts)
                    {
                        var flattenedInstructions = script.Segment.Flatten()
                            .Select((instruction, index) => new { instruction, index });

                        MOVIEREADY currentMovieReady = null;
                        int moviePosition = -1;
                        int movieSyncPosition = -1;

                        foreach (var instructionWithIndex in flattenedInstructions)
                        {
                            if (instructionWithIndex.instruction is MOVIEREADY movieReady)
                            {
                                // Found a new MOVIEREADY, start tracking it
                                currentMovieReady = movieReady;
                            }
                            else if (instructionWithIndex.instruction is MOVIE && currentMovieReady != null)
                            {
                                // Found the first MOVIE after a MOVIEREADY
                                moviePosition = instructionWithIndex.index;
                            }
                            else if (instructionWithIndex.instruction is MOVIESYNC && currentMovieReady != null && moviePosition != -1)
                            {
                                // Found the first MOVIESYNC after a MOVIE
                                movieSyncPosition = instructionWithIndex.index;

                                // Yield the result lazily
                                yield return new KeyValuePair<string, MovieScriptInfo>(
                                    fieldArchive.Value.FileName,
                                    new MovieScriptInfo(currentMovieReady, moviePosition, movieSyncPosition)
                                );

                                // Reset for the next group
                                currentMovieReady = null;
                                moviePosition = -1;
                                movieSyncPosition = -1;
                            }
                        }
                    }
                }
            }
        }

        #endregion Methods
    }
}