using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;

namespace FF8
{
    internal partial class Icons
    {
        private static Entry[] entries = null;
        private Texture2D[] icons;

        public enum ID
        {
            One,
            Two
        }

        public UInt32 Count { get; private set; }
        public int PalletCount { get; private set; }

        public Icons()
        {
            if (entries == null)
            {
                ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);
                TEX tex;
                byte[] test = ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
                    aw.GetListOfFiles().First(x => x.ToLower().Contains("icon.tex")));

                tex = new TEX(test);
                PalletCount = tex.TextureData.NumOfPalettes;
                icons = new Texture2D[PalletCount];
                for (int i = 0; i < PalletCount; i++)
                {
                    icons[i] = tex.GetTexture(i);
                    using (FileStream fs = File.OpenWrite($"d:\\icons.{i}.png"))
                    {
                        //fs.Write(test, 0, test.Length);

                        icons[i].SaveAsPng(fs, 256, 256);
                    }
                }
                test = ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
                    aw.GetListOfFiles().First(x => x.ToLower().Contains("icon.sp1")));
                //using (FileStream fs = File.OpenWrite(Path.Combine("d:\\", "icons.sp1")))
                //{
                //    fs.Write(test, 0, test.Length);
                //}
                using (MemoryStream ms = new MemoryStream(test))
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        Count = br.ReadUInt32();
                        Loc[] locs = new Loc[Count];
                        for (int i = 0; i < locs.Length; i++)
                        {
                            locs[i].pos = br.ReadUInt16();
                            locs[i].count = br.ReadUInt16();
                            if (locs[i].count > 1) Count += (uint)(locs[i].count - 1);
                        }

                        entries = new Entry[Count];
                        int e = 0;
                        for (int i = 0; i < locs.Length; i++)
                        {
                            ms.Seek(locs[i].pos, SeekOrigin.Begin);
                            byte c = (byte)locs[i].count;
                            if (c > 1)
                            {
                            }
                            do
                            {
                                Entry tmp = new Entry
                                {
                                    CurrentPos = (ushort)ms.Position,
                                    Part = c,
                                    X = br.ReadByte(),
                                    Y = br.ReadByte(),
                                };
                                tmp.SetLoc(locs[i]);

                                tmp.UNK[0] = br.ReadByte();
                                tmp.UNK[1] = br.ReadByte();
                                //ms.Seek(2, SeekOrigin.Current);
                                tmp.Width = br.ReadByte();
                                tmp.Offset_X = br.ReadSByte();
                                //ms.Seek(1, SeekOrigin.Current);
                                tmp.Height = br.ReadByte();
                                tmp.Offset_Y = br.ReadSByte();
                                //ms.Seek(1, SeekOrigin.Current);
                                //if (!(tmp.X == 0 && tmp.Y == 0))
                                entries[e++] = tmp;
                                //tmp.LoadfromStreamSP2(br);
                            }
                            while (--c > 0);
                        }
                    }
                }
            }
        }

        public Entry GetEntry(ID id) => GetEntry((int)id);

        public Entry GetEntry(int id) => entries[id] ?? null;

        internal void Draw(ID id, int pallet, Rectangle dst, float fade = 1f) => Draw((int)id, pallet, dst, fade);

        internal void Draw(int id, int pallet, Rectangle dst, float fade = 1f)
        {
            Viewport vp = Memory.graphics.GraphicsDevice.Viewport;

            Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);
            Memory.spriteBatch.Draw(icons[pallet], dst, entries[id].GetRectangle(), Color.White * fade);
            Memory.SpriteBatchEnd();
            Memory.SpriteBatchStartStencil(SamplerState.PointClamp);
            Memory.font.RenderBasicText(Font.CipherDirty($"pos: {entries[id].GetLoc().pos}\ncount: {entries[id].GetLoc().count}\n\nid: {id}\n\nUNKS: {string.Join(", ", entries[id].UNK)}\nALTS: {string.Join(", ", Array.ConvertAll(entries[id].UNK, item => (sbyte)item))}\n\npallet: {pallet}\nx: {entries[id].X}\ny: {entries[id].Y}\nwidth: {entries[id].Width}\nheight: {entries[id].Height} \n\nOffset X: {entries[id].Offset_X}\nOffset Y: {entries[id].Offset_Y}"), (int)(vp.Width * 0.10f), (int)(vp.Height * 0.05f), 1f, 2f, 0, 1);
            Memory.SpriteBatchEnd();
        }

        internal void Draw(Rectangle dst, int pallet, float fade = 1f) => Memory.spriteBatch.Draw(icons[pallet], dst, Color.White * fade);
    }
}