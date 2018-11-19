using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DirectMidi;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace FF8
{
    static class init_debugger_Audio
    {
        private static CDirectMusic cdm;
        private static CDLSLoader loader;
        private static CSegment segment;
        private static CAPathPerformance path;
        public static CPortPerformance cport; //public explicit
        private static COutputPort outport;
        private static CCollection ccollection;
        private static CInstrument[] instruments;


        public const int S_OK = 0x00000000;


        internal static void DEBUG()
        {
            string pt = Memory.FF8DIR + "/../Music/dmusic/";
            Memory.musices = Directory.GetFiles(pt, "*.sgt");
            PlayMusic();
            //FileStream fs = new FileStream(pt, FileMode.Open, FileAccess.Read);
            //BinaryReader br = new BinaryReader(fs);
            //string RIFF = Encoding.ASCII.GetString(br.ReadBytes(4));
            //if (RIFF != "RIFF") throw new Exception("NewDirectMusic::NOT RIFF");
            //uint eof = br.ReadUInt32();
            //if (fs.Length != eof + 8) throw new Exception("NewDirectMusic::RIFF length/size indicator error");
            //string dmsg = Encoding.ASCII.GetString(br.ReadBytes(4));
            //var SegmentHeader = GetSGTSection(br);
            //var guid = GetSGTSection(br);
            //var list = GetSGTSection(br);
            //var vers = GetSGTSection(br);
            //var list_unfo = GetSGTSection(br);
            //string UNFO = SGT_ReadUNAM(list_unfo).TrimEnd('\0');
            //var trackList = GetSGTSection(br);
            //List<byte[]> Tracks = ProcessTrackList(trackList.Item2);
            //byte[] sequenceTrack = Tracks[2];
            //PseudoBufferedStream pbs = new PseudoBufferedStream(sequenceTrack);
            //pbs.Seek(44, PseudoBufferedStream.SEEK_BEGIN);
            //string seqt = Encoding.ASCII.GetString(BitConverter.GetBytes(pbs.ReadUInt()));
        }

        internal static void update()

        {

        }
        //callable test
        unsafe public static void PlayMusic()
        {
            string pt = Memory.musices[Memory.musicIndex];
            if (cdm == null)
            {
                cdm = new CDirectMusic();
                cdm.Initialize();
                loader = new CDLSLoader();
                loader.Initialize();
                loader.LoadSegment(pt, out segment);
                ccollection = new CCollection();
                loader.LoadDLS(Memory.FF8DIR + "/../Music/dmusic/FF8.dls", out ccollection);
                uint dwInstrumentIndex = 0;
                INSTRUMENTINFO iInfo;
                while (ccollection.EnumInstrument(++dwInstrumentIndex, out iInfo) == S_OK)
                {
                    Debug.WriteLine(iInfo.szInstName);
                }
                instruments = new CInstrument[dwInstrumentIndex];

                path = new CAPathPerformance();
                path.Initialize(cdm, null, null, DMUS_APATH.DYNAMIC_3D, 128);
                cport = new CPortPerformance();
                cport.Initialize(cdm, null, null);
                outport = new COutputPort();
                outport.Initialize(cdm);

                uint dwPortCount = 0;
                INFOPORT infoport;
                do
                    outport.GetPortInfo(++dwPortCount, out infoport);
                while ((infoport.dwFlags & DMUS_PC.SOFTWARESYNTH) == 0);

                outport.SetPortParams(0, 0, 0, SET.REVERB | SET.CHORUS, 44100);
                outport.ActivatePort(infoport);

                cport.AddPort(outport, 0, 1);

                for (int i = 0; i < dwInstrumentIndex; i++)
                {
                    ccollection.GetInstrument(out instruments[i], i);
                    outport.DownloadInstrument(instruments[i]);
                }
                segment.Download(cport);
                cport.PlaySegment(segment);
            }
            else
            {
                cport.Stop(segment);
                segment.Dispose();
                //segment.ConnectToDLS
                loader.LoadSegment(pt, out segment);
                segment.Download(cport);
                cport.PlaySegment(segment);
                cdm.Dispose();
            }
            
            //GCHandle.Alloc(cdm, GCHandleType.Pinned);
            //GCHandle.Alloc(loader, GCHandleType.Pinned);
            //GCHandle.Alloc(segment, GCHandleType.Pinned);
            //GCHandle.Alloc(path, GCHandleType.Pinned);
            //GCHandle.Alloc(cport, GCHandleType.Pinned);
            //GCHandle.Alloc(outport, GCHandleType.Pinned);
            //GCHandle.Alloc(infoport, GCHandleType.Pinned);

            
        }

        public static void KillAudio()
        {
            cport.StopAll();
            cport.Dispose();
            ccollection.Dispose();
            loader.Dispose();
            outport.Dispose();
            path.Dispose();
            cdm.Dispose();
    }

        private static List<byte[]> ProcessTrackList(byte[] item2)
        {
            PseudoBufferedStream pbs = new PseudoBufferedStream(item2);
            string head = Encoding.ASCII.GetString(BitConverter.GetBytes(pbs.ReadUInt()));

            head = Encoding.ASCII.GetString(BitConverter.GetBytes(pbs.ReadUInt())); //RIFF start
            List<byte[]> Tracks = new List<byte[]>();
            //now process tracks
            while (head == "RIFF")
            {
                uint vara = pbs.ReadUInt();
                byte[] buf = pbs.ReadBytes(vara);
                Tracks.Add(buf);
                if (pbs.Tell() == pbs.Length)
                    break;
                head = Encoding.ASCII.GetString(BitConverter.GetBytes(pbs.ReadUInt()));
            }
            return Tracks;
        }

        private static string SGT_ReadUNAM(Tuple<string, byte[]> uNFO)
        {
            PseudoBufferedStream pbs = new PseudoBufferedStream(uNFO.Item2);
            string unfo = Encoding.ASCII.GetString(BitConverter.GetBytes(pbs.ReadUInt()));
            string unam = Encoding.ASCII.GetString(BitConverter.GetBytes(pbs.ReadUInt()));
            uint vara = pbs.ReadUInt();
            return Encoding.Unicode.GetString(pbs.ReadBytes(vara));
        }

        private static Tuple<string, byte[]> GetSGTSection_pbs(byte[] buffer)
        {
            PseudoBufferedStream pbs = new PseudoBufferedStream(buffer);
            string head = Encoding.ASCII.GetString(BitConverter.GetBytes(pbs.ReadUInt()));
            uint vara = pbs.ReadUInt();
            byte[] buf = pbs.ReadBytes(vara);
            return new Tuple<string, byte[]>(head, buf);
        }

        private static Tuple<string, byte[]> GetSGTSection(BinaryReader br)
        {
            string head = Encoding.ASCII.GetString(br.ReadBytes(4));
            uint vara = br.ReadUInt32();
            byte[] segData = br.ReadBytes((int)vara);
            return new Tuple<string, byte[]>(head, segData);
        }


    }
}
