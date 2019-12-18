using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII.Battle
{
    class Mag
    {
        public string FileName { get; private set; }
        public TIM2 TIM { get; private set; }
        public Mag(string filename, BinaryReader br)
        {
            FileName = filename;
            br.BaseStream.Seek(0, SeekOrigin.Begin);
            TryReadTIM(br);
        }
        public TIM2 TryReadTIM(BinaryReader br)
        {
            try
            {
                TIM = new TIM2(br,noExec:true);
                if (TIM.NOT_TIM)
                    return TIM = null;
                return TIM;
            }
            catch(InvalidDataException)
            {
                return TIM = null;
            }
        }

    }
}
