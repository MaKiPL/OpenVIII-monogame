using System.Collections.Generic;

namespace OpenVIII.Card
{
    public class Faces : SP2
    {
        protected override void DefaultValues()
        {
            base.DefaultValues();
            Props = new List<TexProps>()
            {
                new TexProps{
                    Filename = EXE_Offsets.FileName[Memory.Year],
                    Count = 14,
                    Big = new List<BigTexProps>{
                        new BigTexProps{
                            Filename = "cards_{0:0}.png" } },
                    Offset = EXE_Offsets.TIM[Memory.Year][2],
                },
            };
            TextureStartOffset = 0;
            EntriesPerTexture = 8;
            IndexFilename = "";
        }
    }
}