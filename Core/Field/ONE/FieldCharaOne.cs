using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace OpenVIII.Fields
{
    //this is static, because it's always alive
    public static class FieldMainCharaOne
    {
        #region Fields

        public static bool BAlreadyInitialized;

        public static MainFieldChara[] MainFieldCharacters;

        #endregion Fields

        #region Methods

        public static void Init(bool bForce = false)
        {
            if (BAlreadyInitialized && !bForce)
                return;

            var mfc = new List<MainFieldChara>();

            var aw = ArchiveWorker.Load(Memory.Archives.A_FIELD);
            var test = aw.GetListOfFiles();

            var collectionEntry = test.Where(x => x.ToLower().Contains("main_chr")).ToList().AsReadOnly();
            if (!collectionEntry.Any()) return;
            var fieldArchiveName = collectionEntry.First();
            var fieldArchive = aw.GetArchive(fieldArchiveName);
            var test2 = fieldArchive.GetListOfFiles();

            foreach (var fieldArchiveFileName in test2)
            {
                //if (test_[i].Contains("d008.mch"))
                //    continue;
                if (string.IsNullOrWhiteSpace(fieldArchiveFileName))
                    continue;
                var oneBytes = fieldArchive.GetBinaryFile(fieldArchiveFileName);

                if (oneBytes.Length < 64) //Hello Kazuo Suzuki! I will skip your dummy files
                    continue;

                var currentLocalChara = ReadMainChara(oneBytes);
                var localId = int.Parse(Path.GetFileNameWithoutExtension(fieldArchiveFileName).Substring(1, 3));
                currentLocalChara.ID = localId;
                mfc.Add(currentLocalChara);
            }
            MainFieldCharacters = mfc.ToArray();
            BAlreadyInitialized = true;
        }

        private static MainFieldChara ReadMainChara(byte[] oneBytes)
        {
            var localMfc = new MainFieldChara();
            using (var ms = new MemoryStream(oneBytes))
            using (var br = new BinaryReader(ms))
            {
                var timOffsets = new List<uint>();
                uint timOffset;
                while ((timOffset = br.ReadUInt32()) != 0xffffffff)
                    timOffsets.Add(timOffset & 0x00FFFFFF);
                var modelPointer = br.ReadUInt32();

                //read textures
                var texture2DReader = timOffsets
                    .Select(x => new TIM2(br, x))
                    .Select(x => x.GetTexture()).ToList()
                    .AsReadOnly();

                localMfc.Textures = texture2DReader.ToArray();

                //read models
                ms.Seek(modelPointer, SeekOrigin.Begin);

                var mch = new Debug_MCH(ms, br, Debug_MCH.mchMode.FieldMain);
                localMfc.MCH = mch;
            }
            return localMfc;
        }

        #endregion Methods

        #region Structs

        public struct MainFieldChara
        {
            #region Fields

            public int ID;
            public Debug_MCH MCH;
            public Texture2D[] Textures;

            #endregion Fields
        }

        #endregion Structs
    }

    internal class FieldCharaOne
    {
        #region Fields

        public CharaModelHeaders[] FieldModels;
        [SuppressMessage("ReSharper", "NotAccessedField.Local")] private readonly int _fieldId;

        #endregion Fields

        #region Constructors

        public FieldCharaOne(int fieldId)
        {
            _fieldId = fieldId;
            if (!FieldMainCharaOne.BAlreadyInitialized)
                FieldMainCharaOne.Init();
            var aw = ArchiveWorker.Load(Memory.Archives.A_FIELD);
            var test = aw.GetListOfFiles();

            var collectionEntry = test.Where(x => x.IndexOf(Memory.FieldHolder.fields[Memory.FieldHolder.FieldID], StringComparison.OrdinalIgnoreCase) >= 0).ToList().AsReadOnly();
            if (!collectionEntry.Any()) return;
            var fieldArchiveName = collectionEntry.First();
            var fieldArchive = aw.GetArchive(fieldArchiveName);

            var test2 = fieldArchive.GetListOfFiles();

            string one;
            //string main_chr;
            try
            {
                one = test2.First(x => x.EndsWith(".one", StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return;
            }

            var oneBytes = fieldArchive.GetBinaryFile(one);
            if (oneBytes.Length == 0)
                return;
            ReadBuffer(oneBytes);
        }

        #endregion Constructors

        #region Methods

        private void ReadBuffer(byte[] oneBuffer)
        {
            var cmh = new List<CharaModelHeaders>();
            using (var ms = new MemoryStream(oneBuffer))
            using (var br = new BinaryReader(ms))
            {
                var nModels = br.ReadUInt32();
                for (var i = 0; i < nModels; i++)
                {
                    var localCmh = new CharaModelHeaders
                    {
                        Offset = br.ReadUInt32() + 4,
                        Size = br.ReadUInt32(),
                        Size2 = br.ReadUInt32(),
                        FlagDword = br.ReadUInt32()
                    };
                    var bIgnorePadding = false;
                    var bMainChara = false;
                    if (localCmh.FlagDword >> 24 == 0xD0) //main character file
                    {
                        localCmh.TIMOffset = new uint[0];
                        localCmh.ModelDataOffset = 0xFFFFFFFF;
                        ms.Seek(4, SeekOrigin.Current);
                        bMainChara = true;
                    }
                    else if (localCmh.FlagDword >> 24 == 0xa0) //unknown- object without texture/ placeholder?
                    {
                        localCmh.TIMOffset = new uint[0];
                        localCmh.ModelDataOffset = 0xFFFFFFFF;
                        ms.Seek(8, SeekOrigin.Current);
                        bIgnorePadding = true;
                    }
                    else
                    {
                        var timOffsets = new List<uint>();
                        // ReSharper disable once UnusedVariable
                        var flagTimOffset = localCmh.FlagDword & 0x00FFFFFF;

                        timOffsets.Add(localCmh.FlagDword << 8);
                        uint localTimOffset;
                        while ((localTimOffset = br.ReadUInt32()) != 0xFFFFFFFF)
                            timOffsets.Add(localTimOffset & 0x00FFFFFF);
                        localCmh.TIMOffset = timOffsets.ToArray();
                        localCmh.ModelDataOffset = br.ReadUInt32();
                    }
                    localCmh.ModelName = br.ReadChars(8);
                    localCmh.Padding = br.ReadUInt32();
                    if (localCmh.Padding != 0xEEEEEEEE && !bIgnorePadding) //null models for placeholders are 2 not eeeeeeee
                        throw new Exception("Chara one- padding was not 0xEEEEEEEE- check code for ReadBuffer in FieldCharaOne");

                    int lastMsPosition;
                    if (localCmh.ModelDataOffset != 0xFFFFFFFF)
                    {
                        lastMsPosition = (int)ms.Position;
                        ms.Seek(localCmh.Offset + localCmh.ModelDataOffset, SeekOrigin.Begin);
                        localCmh.MCH = new Debug_MCH(ms, br, Debug_MCH.mchMode.FieldNPC, 3f);
                        //ms.Seek(localCmh.offset + 4, SeekOrigin.Begin);
                        var texList = new List<Texture2D>();
                        for (var n = 0; n < localCmh.TIMOffset.Length; n++)
                        {
                            if (localCmh.TIMOffset[n] > 0x10000000)
                                localCmh.TIMOffset[n] = localCmh.TIMOffset[n] & 0x00FFFFFF;
                            var localTim = new TIM2(br, localCmh.Offset + localCmh.TIMOffset[n]);
                            texList.Add(localTim.GetTexture());
                        }
                        localCmh.Textures = texList.ToArray();
                        ms.Seek(lastMsPosition, SeekOrigin.Begin);
                    }
                    else if (bMainChara)
                    {
                        lastMsPosition = (int)ms.Position;
                        //this is main chara, so please grab data from main_chr.fs
                        var getRefId = int.Parse(new string(localCmh.ModelName).Substring(1, 3));
                        var chara = FieldMainCharaOne.MainFieldCharacters.First(x => x.ID == getRefId);
                        localCmh.ModelDataOffset = 1;
                        localCmh.MCH = chara.MCH;
                        localCmh.Textures = chara.Textures;
                        ms.Seek(localCmh.Offset, SeekOrigin.Begin);
                        localCmh.MCH.MergeAnimations(ms, br);
                        ms.Seek(lastMsPosition, SeekOrigin.Begin);
                    }

                    cmh.Add(localCmh);
                }
            }
            FieldModels = cmh.ToArray();
        }

        #endregion Methods

        #region Structs

        public struct CharaModelHeaders
        {
            #region Fields

            ///this is either tim or indicator of main model
            public uint FlagDword;

            public Debug_MCH MCH;

            public uint ModelDataOffset;

            ///8
            public char[] ModelName;

            ///points to texture
            public uint Offset;

            ///0xEEEEEEEE
            public uint Padding;

            ///size of whole segment
            public uint Size;

            ///as above
            public uint Size2;

            public Texture2D[] Textures;

            ///pointer to zero
            public uint[] TIMOffset;

            #endregion Fields
        }

        #endregion Structs
    }
}