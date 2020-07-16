using System;
using System.Security.Cryptography;

using NUnit.Framework;


namespace OpenVIII.Tests
{
    using FFmpeg.AutoGen;

    [TestFixture]
    public class MIDITest
    {
        #region Methods

        [Test]
        public void LoadingTest()
        {
            AV.Midi.Fluid fluidMidi = loadMidiData();

            fluidMidi.FluidWorker_ProduceMid();

            SHA256 mySHA256 = SHA256.Create();
            string actual = byteArrayToString(mySHA256.ComputeHash(fluidMidi.midBuffer));
 
            Assert.AreEqual("9e3ab13fc48c813864fbe8904594c2abfb576364453456d792d788ee3f5e8f99", actual);
        }

        [Test]
        public void ProcessTest()
        {
            AV.Midi.Fluid fluidMidi = loadMidiData();
            Core.AV.Music.Midi.MidiProcessor midiProcessor = new Core.AV.Music.Midi.MidiProcessor(AV.Midi.Fluid.lbinbins, AV.Midi.Fluid.tetr, AV.Midi.Fluid.seqt, AV.Midi.Fluid.tims);

            // It seems that none of the exported MIDIs have an end track?
            // Which makes sense because the music is supposed to loop.
            // But why wasn't the assertion triggered before?
            var mid = midiProcessor.Process();
        }

        // Display the byte array in a readable format.
        // Adapted from https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha256?view=netcore-3.1
        private static string byteArrayToString(byte[] array)
        {
            string hash = "";

            for (int i = 0; i < array.Length; i++)
            {
                hash += $"{array[i]:x2}";
            }

            return hash;
        }

        private AV.Midi.Fluid loadMidiData()
        {
            Memory.Init(null, null, null, null);
            AV.Music.Init();

            // Load the overture MIDI.
            var filename = Memory.DicMusic[(MusicId)79][0];

            AV.Midi.Fluid fluidMidi = new AV.Midi.Fluid();
            fluidMidi.ReadSegmentFileManually(filename);

            return fluidMidi;
        }

        #endregion Methods
    }
}
