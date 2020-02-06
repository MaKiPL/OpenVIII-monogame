using System;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenVIII.AV
{
    public static partial class Sound
    {
        #region Structs

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct Entry
        {
            public UInt32 Size;
            public UInt32 Offset;
            private UInt32 output_TotalSize => Size + 70; // Total bytes of file -8 because for some reason 8 bytes don't count
            private const UInt32 output_HeaderSize = 50; //Total bytes of Header
            private UInt32 output_DataSize => Size; //Total bytes of Data Section

            //public byte[] UNK; //12
            //public WAVEFORMATEX WAVFORMATEX; //18 header starts here
            //public ushort SamplesPerBlock; //2
            //public ushort ADPCM; //2
            //public ADPCMCOEFSET[] ADPCMCoefSets; //array should be of [ADPCM] size //7*4 = 28
            public byte[] HeaderData;

            public void fillHeader(BinaryReader br)
            {
                if (HeaderData == null)
                {
                    HeaderData = new byte[output_HeaderSize + 28];
                    using (BinaryWriter bw = new BinaryWriter(new MemoryStream(HeaderData)))
                    {
                        bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                        bw.Write(output_TotalSize);
                        bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                        bw.Write(output_HeaderSize);
                        bw.Write(br.ReadBytes((int)output_HeaderSize));
                        bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                        bw.Write(output_DataSize);
                    }
                }
            }
        }

        #endregion Structs

    }
}