using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public sealed partial class Cards : SP2
    {
        #region Constructors

        /// <summary>
        /// Card images used in menus. The images used in the triple triad game are in the ff8.exe in
        /// tim files.
        /// </summary>
        /// <seealso cref="http://forums.qhimm.com/index.php?topic=11084.0"/>
        public Cards()
        {
        }

        #endregion Constructors

        #region Methods

        public static Cards Load() => Load<Cards>();

        protected override void DefaultValues()
        {
            base.DefaultValues();
            Props = new List<TexProps>()
            {
                new TexProps{Filename="mc00.tex", Count = 1 },
                new TexProps{Filename="mc01.tex", Count = 1 },
                new TexProps{Filename="mc02.tex", Count = 1 },
                new TexProps{Filename="mc03.tex", Count = 1 },
                new TexProps{Filename="mc04.tex", Count = 1 },
                new TexProps{Filename="mc05.tex", Count = 1 },
                new TexProps{Filename="mc06.tex", Count = 1 },
                new TexProps{Filename="mc07.tex", Count = 1 },
                new TexProps{Filename="mc08.tex", Count = 1 },
                new TexProps{Filename="mc09.tex", Count = 1 }
            };
            TextureStartOffset = 0;
            EntriesPerTexture = 11;
            IndexFilename = "cardanm.sp2";
        }

        public const float AspectRatio = 62f / 88f; //B6 paper

        protected override void Init() => base.Init();

        public override TextureHandler GetTexture(Enum id, int file = -1)
        {
            var pos = Convert.ToInt32(id);

            var pageFile = pos / EntriesPerTexture;

            return pos >= (int)(Cards.ID.Card_Back) ? Textures[0] : Textures[pageFile];
        }

        public override void Draw(Enum id, Rectangle dst, float fade = 1)
        {
            var v = Convert.ToUInt32(id);

            uint pos;
            if (v >= Convert.ToUInt32(Cards.ID.Card_Back))
            {
                //assuming to use back card for Card_Back, Immune and Fail
                pos = Memory.Cards.Count - 1;
            }
            else
            {
                pos = (uint)(v % EntriesPerTexture);

            }

            var src = GetEntry(pos).GetRectangle;
            var tex = GetTexture(id);
            tex.Draw(dst, src, Color.White * fade);
        }

        public override Entry GetEntry(uint id)
        {
            if (Entries.ContainsKey(id))
            {
                return Entries[id];
            }

            return null;
        }


        #endregion Methods
    }
}