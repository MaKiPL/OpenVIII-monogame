using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII
{
    class FieldCharaOne
    {
        public CharaModelHeaders[] fieldModels;

        public struct CharaModelHeaders
        {
            public uint offset; //points to texture
            public uint size; //size of whole segment
            public uint size2; //as above
            public uint flagDWORD; //this is either tim or indicator of main model
            public uint[] timOffset; //pointer to zero
            public uint modelDataOffset;
            public char[] modelName; //8
            public uint padding; //0xEEEEEEEE
            public Debug_MCH mch;
            public Texture2D[] textures;
        }


        public FieldCharaOne(int fieldId)
        {
            if (!FieldMainCharaOne.bAlreadyInitialized)
                FieldMainCharaOne.Init();
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_FIELD);
            string[] test = aw.GetListOfFiles();

            var CollectionEntry = test.Where(x => x.ToLower().Contains(Memory.FieldHolder.fields[Memory.FieldHolder.FieldID]));
            if (!CollectionEntry.Any()) return;
            string fieldArchive = CollectionEntry.First();
            int fieldLen = fieldArchive.Length - 3;
            fieldArchive = fieldArchive.Substring(0, fieldLen);


            byte[] fs = aw.GetBinaryFile($"{fieldArchive}{Memory.Archive.B_FileArchive}");
            byte[] fi = aw.GetBinaryFile($"{fieldArchive}{Memory.Archive.B_FileIndex}");
            byte[] fl = aw.GetBinaryFile($"{fieldArchive}{Memory.Archive.B_FileList}");
            string[] test_ = aw.GetBinaryFileList(fl);


            string one;
            string main_chr;
            try
            {
                one = test_.First(x => x.ToLower().Contains(".one"));
            }
            catch
            {
                return;
            }

            byte[] oneb = aw.FileInTwoArchives(fi, fs, fl, one);
            if (oneb.Length == 0)
                return;
            ReadBuffer(oneb);
        }

        private void ReadBuffer(byte[] oneBuffer)
        {
            List<CharaModelHeaders> cmh = new List<CharaModelHeaders>();
            int lastMsPosition = 0;
            using (MemoryStream ms = new MemoryStream(oneBuffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                uint nModels = br.ReadUInt32();
                for (int i = 0; i < nModels; i++)
                {

                    CharaModelHeaders localCmh = new CharaModelHeaders();
                    localCmh.offset = br.ReadUInt32() + 4;
                    localCmh.size = br.ReadUInt32();
                    localCmh.size2 = br.ReadUInt32();
                    localCmh.flagDWORD = br.ReadUInt32();
                    bool bIgnorePadding = false;
                    bool bMainChara = false;
                    if (localCmh.flagDWORD >> 24 == 0xD0) //main character file
                    {
                        localCmh.timOffset = new uint[0];
                        localCmh.modelDataOffset = 0xFFFFFFFF;
                        ms.Seek(4, SeekOrigin.Current);
                        bMainChara = true;
                    }
                    else if (localCmh.flagDWORD >> 24 == 0xa0) //unknown- object without texture/ placeholder?
                    {
                        localCmh.timOffset = new uint[0];
                        localCmh.modelDataOffset = 0xFFFFFFFF;
                        ms.Seek(8, SeekOrigin.Current);
                        bIgnorePadding = true;
                    }
                    else
                    {
                        List<uint> timOffsets = new List<uint>();
                        uint flagTimOffset = localCmh.flagDWORD & 0x00FFFFFF;

                        timOffsets.Add(localCmh.flagDWORD << 8);
                        uint localTimOffset;
                        while ((localTimOffset = br.ReadUInt32()) != 0xFFFFFFFF)
                            timOffsets.Add(localTimOffset & 0x00FFFFFF);
                        localCmh.timOffset = timOffsets.ToArray();
                        localCmh.modelDataOffset = br.ReadUInt32();
                    }
                    localCmh.modelName = br.ReadChars(8);
                    localCmh.padding = br.ReadUInt32();
                    if (localCmh.padding != 0xEEEEEEEE && !bIgnorePadding) //null models for placeholders are 2 not eeeeeeee
                        throw new Exception("Chara one- padding was not 0xEEEEEEEE- check code for ReadBuffer in FieldCharaOne");

                    if (localCmh.modelDataOffset != 0xFFFFFFFF)
                    {
                        lastMsPosition = (int)ms.Position;
                        ms.Seek(localCmh.offset + localCmh.modelDataOffset, SeekOrigin.Begin);
                        localCmh.mch = new Debug_MCH(ms, br, Debug_MCH.mchMode.FieldNPC, 3f);
                        //ms.Seek(localCmh.offset + 4, SeekOrigin.Begin);
                        List<Texture2D> texList = new List<Texture2D>();
                        for (int n = 0; n < localCmh.timOffset.Length; n++)
                        {
                            if (localCmh.timOffset[n] > 0x10000000)
                                localCmh.timOffset[n] = localCmh.timOffset[n] & 0x00FFFFFF;
                            TIM2 localTim = new TIM2(br, localCmh.offset + localCmh.timOffset[n]);
                            texList.Add(localTim.GetTexture());
                        }
                        localCmh.textures = texList.ToArray();
                        ms.Seek(lastMsPosition, SeekOrigin.Begin);
                    }
                    else if (bMainChara)
                    {
                        lastMsPosition = (int)ms.Position;
                        //this is main chara, so please grab data from main_chr.fs
                        int getRefId = int.Parse(new string(localCmh.modelName).Substring(1, 3));
                        var chara = FieldMainCharaOne.MainFieldCharacters.Where(x => x.id == getRefId).First();
                        localCmh.modelDataOffset = 1;
                        localCmh.mch = chara.mch;
                        localCmh.textures = chara.textures;
                        ms.Seek(localCmh.offset, SeekOrigin.Begin);
                        localCmh.mch.MergeAnimations(ms, br);
                        ms.Seek(lastMsPosition, SeekOrigin.Begin);
                    }

                    cmh.Add(localCmh);
                }
            }
            fieldModels = cmh.ToArray();
        }
    }

    //this is static, because it's always alive
    public static class FieldMainCharaOne
    {
        public static bool bAlreadyInitialized = false;

        public static MainFieldChara[] MainFieldCharacters;

        public struct MainFieldChara
        {
            public int id;
            public Debug_MCH mch;
            public Texture2D[] textures;
        }


        public static void Init(bool bForce = false)
        {
            if (bAlreadyInitialized && !bForce)
                return;

            List<MainFieldChara> mfc = new List<MainFieldChara>();

            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_FIELD);
            string[] test = aw.GetListOfFiles();

            var CollectionEntry = test.Where(x => x.ToLower().Contains("main_chr"));
            if (!CollectionEntry.Any()) return;
            string fieldArchive = CollectionEntry.First();
            int fieldLen = fieldArchive.Length - 3;
            fieldArchive = fieldArchive.Substring(0, fieldLen);

            byte[] fs = aw.GetBinaryFile($"{fieldArchive}{Memory.Archive.B_FileArchive}");
            byte[] fi = aw.GetBinaryFile($"{fieldArchive}{Memory.Archive.B_FileIndex}");
            byte[] fl = aw.GetBinaryFile($"{fieldArchive}{Memory.Archive.B_FileList}");
            string[] test_ = aw.GetBinaryFileList(fl);

            for(int i = 0; i<test_.Length; i++)
            {
                //if (test_[i].Contains("d008.mch"))
                //    continue;
                if (string.IsNullOrWhiteSpace(test_[i]))
                    continue;
                byte[] oneb = aw.FileInTwoArchives(fi, fs, fl, test_[i]);

                if (oneb.Length < 64) //Hello Kazuo Suzuki! I will skip your dummy files
                    continue;

                MainFieldChara currentLocalChara = ReadMainChara(oneb);
                int localId = int.Parse(Path.GetFileNameWithoutExtension(test_[i]).Substring(1,3));
                currentLocalChara.id = localId;
                mfc.Add(currentLocalChara);
            }
            MainFieldCharacters = mfc.ToArray();
            bAlreadyInitialized = true;
        }

        private static MainFieldChara ReadMainChara(byte[] oneb)
        {
            MainFieldChara localMfc = new MainFieldChara();
            using (MemoryStream ms = new MemoryStream(oneb))
            using (BinaryReader br = new BinaryReader(ms))
            {
                List<uint> timOffsets = new List<uint>();
                uint timOffset;
                while((timOffset = br.ReadUInt32())!=0xffffffff )
                    timOffsets.Add(timOffset & 0x00FFFFFF);
                uint modelPointer = br.ReadUInt32();

                //read textures
                List<Texture2D> tex2Dreader = new List<Texture2D>();
                for(int i = 0; i<timOffsets.Count; i++)
                {
                    TIM2 tim = new TIM2(br, timOffsets[i]);
                    tex2Dreader.Add(tim.GetTexture());
                }
                localMfc.textures = tex2Dreader.ToArray();

                //read models
                ms.Seek(modelPointer, SeekOrigin.Begin);

                Debug_MCH mch = new Debug_MCH(ms, br, Debug_MCH.mchMode.FieldMain);
                localMfc.mch = mch;
            }
            return localMfc;
        }
    }
}
