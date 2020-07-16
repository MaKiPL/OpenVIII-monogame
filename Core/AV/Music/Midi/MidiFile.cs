using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using NAudio.Midi;
using NAudio.Utils;

namespace OpenVIII.AV.Midi
{
    public class MidiFile : NAudio.Midi.MidiFile
    {
        public class MergeSort
        {
            /// <summary>
            /// In-place and stable implementation of MergeSort
            /// </summary>
            static void Sort<T>(IList<T> list, int lowIndex, int highIndex, IComparer<T> comparer)
            {
                if (lowIndex >= highIndex)
                {
                    return;
                }


                int midIndex = (lowIndex + highIndex) / 2;


                // Partition the list into two lists and Sort them recursively
                Sort(list, lowIndex, midIndex, comparer);
                Sort(list, midIndex + 1, highIndex, comparer);

                // Merge the two sorted lists
                int endLow = midIndex;
                int startHigh = midIndex + 1;


                while ((lowIndex <= endLow) && (startHigh <= highIndex))
                {
                    // MRH, if use < 0 sort is not stable
                    if (comparer.Compare(list[lowIndex], list[startHigh]) <= 0)
                    {
                        lowIndex++;
                    }
                    else
                    {
                        // list[lowIndex] > list[startHigh]
                        // The next element comes from the second list, 
                        // move the list[start_hi] element into the next 
                        //  position and shuffle all the other elements up.
                        T t = list[startHigh];

                        for (int k = startHigh - 1; k >= lowIndex; k--)
                        {
                            list[k + 1] = list[k];
                        }

                        list[lowIndex] = t;
                        lowIndex++;
                        endLow++;
                        startHigh++;
                    }
                }
            }

            /// <summary>
            /// MergeSort a list of comparable items
            /// </summary>
            public static void Sort<T>(IList<T> list) where T : IComparable<T>
            {
                Sort(list, 0, list.Count - 1, Comparer<T>.Default);
            }

            /// <summary>
            /// MergeSort a list 
            /// </summary>
            public static void Sort<T>(IList<T> list, IComparer<T> comparer)
            {
                Sort(list, 0, list.Count - 1, comparer);
            }
        }

        public MidiFile(string filename) : base(filename, true)
        {
        }

        private static void ExportBinary(BinaryWriter writer, MidiEventCollection events)
        {

            writer.Write(System.Text.Encoding.UTF8.GetBytes("MThd"));
            writer.Write(SwapUInt32(6)); // chunk size
            writer.Write(SwapUInt16((ushort)events.MidiFileType));
            writer.Write(SwapUInt16((ushort)events.Tracks));
            writer.Write(SwapUInt16((ushort)events.DeltaTicksPerQuarterNote));

            for (int track = 0; track < events.Tracks; track++)
            {
                IList<MidiEvent> eventList = events[track];

                writer.Write(System.Text.Encoding.UTF8.GetBytes("MTrk"));
                long trackSizePosition = writer.BaseStream.Position;
                writer.Write(SwapUInt32(0));

                long absoluteTime = events.StartAbsoluteTime;

                // use a stable sort to preserve ordering of MIDI events whose 
                // absolute times are the same
                MergeSort.Sort(eventList, new MidiEventComparer());
                if (eventList.Count > 0 && !MidiEvent.IsEndTrack(eventList[eventList.Count - 1])) {
                    Memory.Log.WriteLine("Exporting a track with a missing end track");
                }
                foreach (var midiEvent in eventList)
                {
                    midiEvent.Export(ref absoluteTime, writer);
                }

                uint trackChunkLength = (uint)(writer.BaseStream.Position - trackSizePosition) - 4;
                writer.BaseStream.Position = trackSizePosition;
                writer.Write(SwapUInt32(trackChunkLength));
                writer.BaseStream.Position += trackChunkLength;
            }
        }

        private static uint SwapUInt32(uint i)
        {
            return ((i & 0xFF000000) >> 24) | ((i & 0x00FF0000) >> 8) | ((i & 0x0000FF00) << 8) | ((i & 0x000000FF) << 24);
        }

        private static ushort SwapUInt16(ushort i)
        {
            return (ushort)(((i & 0xFF00) >> 8) | ((i & 0x00FF) << 8));
        }

        private static void ExportCheckTracks(MidiEventCollection events)
        {
            if (events.MidiFileType == 0 && events.Tracks > 1)
            {
                throw new ArgumentException("Can't export more than one track to a type 0 file");
            }
        }

        /// <summary>
        /// Exports a MIDI file
        /// </summary>
        /// <param name="filename">Filename to export to</param>
        /// <param name="events">Events to export</param>
        public static void Export(string filename, MidiEventCollection events)
        {
            ExportCheckTracks(events);
            using (var writer = new BinaryWriter(File.Create(filename)))
                ExportBinary(writer, events);
        }

        /// <summary>
        /// Exports a MIDI file
        /// </summary>
        /// <param name="stream">Stream to work with</param>
        /// <param name="events">Events to export</param>
        public static void Export(Stream stream, MidiEventCollection events)
        {
            ExportCheckTracks(events);
            using (var writer = new BinaryWriter(stream))
                ExportBinary(writer, events);
        }
    }
}
