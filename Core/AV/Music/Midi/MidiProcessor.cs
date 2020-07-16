using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII.Core.AV.Music.Midi
{
    using OpenVIII.AV.Midi;

    public class MidiProcessor
    {
        private List<Fluid.DMUS_IO_INSTRUMENT> lbinbins;
        private Fluid.DMUS_IO_TEMPO_ITEM tetr;
        private List<Fluid.DMUS_IO_SEQ_ITEM> seqt;
        private List<Fluid.DMUS_IO_TIMESIGNATURE_ITEM> tims;

        public MidiProcessor(List<Fluid.DMUS_IO_INSTRUMENT> instruments, Fluid.DMUS_IO_TEMPO_ITEM tempo, List<Fluid.DMUS_IO_SEQ_ITEM> sequences, List<Fluid.DMUS_IO_TIMESIGNATURE_ITEM> timeSignatures)
        {
            lbinbins = instruments;
            tetr = tempo;
            seqt = sequences;
            tims = timeSignatures;
        }

        public NAudio.Midi.MidiEventCollection Process()
        {
            var mid = new NAudio.Midi.MidiEventCollection(1, Fluid.DMUS_PPQ);
            mid.AddTrack();

            for (var i = 0; i < lbinbins.Count; i++)
            {
                var lbin = lbinbins[i];
                var patch_ = (int)(lbin.dwPatch & 0xFF); //MSB, LSB + patch on the least 8 bits
                var patch = new NAudio.Midi.PatchChangeEvent(0, (int)lbin.dwPChannel + 1, patch_);
                mid.AddEvent(patch, 0);
            }

            mid.AddEvent(new NAudio.Midi.TempoEvent((int)(Fluid.DMUS_MusicTimeMilisecond / tetr.dblTempo), 0), 0);
            for (var i = 0; i < tims.Count; i++)
            {
                var tim = tims[i];
                //NAudio.Midi.TimeSignatureEvent time = new NAudio.Midi.TimeSignatureEvent(tim.lTime, ,,tim);
            }

            for (var i = 0; i < seqt.Count; i++)
            {
                var ss = seqt[i];
                var note = new NAudio.Midi.NoteEvent(ss.mtTime, (int)ss.dwPChannel + 1, NAudio.Midi.MidiCommandCode.NoteOn, ss.bByte1, ss.bByte2);
                mid.AddEvent(note, 0);
                note = new NAudio.Midi.NoteEvent(ss.mtTime + ss.mtDuration, (int)ss.dwPChannel + 1, NAudio.Midi.MidiCommandCode.NoteOff, ss.bByte1, ss.bByte2);
                mid.AddEvent(note, 0);
            }

            for (var i = 0; i < 16; i++)
            {
                //native build of naudio doesn't have the numbers in the enum.
                //you can manually force it to take the number by doing (NAudio.Midi.MidiController)number

                //as suggested on https://github.com/FluidSynth/fluidsynth/issues/544#issuecomment-507844553
                mid.AddEvent(new NAudio.Midi.ControlChangeEvent(0, i + 1, (NAudio.Midi.MidiController)OpenVIII.AV.Midi.MidiController.NRPN_MSB, 120), 0);//99
                mid.AddEvent(new NAudio.Midi.ControlChangeEvent(0, i + 1, (NAudio.Midi.MidiController)OpenVIII.AV.Midi.MidiController.NRPN_LSB, 38), 0);//98
                mid.AddEvent(new NAudio.Midi.ControlChangeEvent(0, i + 1, (NAudio.Midi.MidiController)OpenVIII.AV.Midi.MidiController.LSBGenerator38, 127), 0);//38
                mid.AddEvent(new NAudio.Midi.ControlChangeEvent(0, i + 1, (NAudio.Midi.MidiController)OpenVIII.AV.Midi.MidiController.MSGgenerator38, 110), 0);//6
                //The DLS loader has wrong release/hold/attack so we need to tweak it via generators. It's prior to change
            }

            return mid;
        }
    }
}
