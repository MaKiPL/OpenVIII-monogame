using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    class Cards : Faces, I_SP2
    {
        protected new const int TextureCount = 10;
        protected new const string TextureFilename = "mc{0:00}.tex";

        public Cards() : base()
        {
        }
    }
}
