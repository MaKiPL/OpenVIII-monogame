using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

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
                new TexProps{Filename="mc{0:00}.tex",Count =10 },
            };
            TextureStartOffset = 0;
            EntriesPerTexture = 11;
            IndexFilename = "cardanm.sp2";
        }

        public const float AspectRatio = 62f / 88f; //B6 paper

        protected override void Init() => base.Init();

        public override void Draw(Enum id, Rectangle dst, float fade = 1)
        {
            int v = Convert.ToInt32(id);
            //int t =  (v / EntriesPerTexture);
            //int pos = v % EntriesPerTexture;

            //    pos = (int) Memory.Cards.Count - 1;
            //base.Draw((Cards.ID)(pos+(EntriesPerTexture *t)), dst, fade);
            if (v >= (uint)Cards.ID.Card_Back)
            {
                int ept = EntriesPerTexture;
                EntriesPerTexture = -1;
                id = (Cards.ID)(ept);
                //Rectangle src = GetEntry(ID).GetRectangle;
                //TextureHandler tex = GetTexture(ID,v/ept);
                //tex.Draw(dst, src, Color.White * fade);

                base.Draw(id, dst, fade);
                EntriesPerTexture = ept;
            }
            else
                base.Draw(id, dst, fade);
        }

        #endregion Methods
    }
}