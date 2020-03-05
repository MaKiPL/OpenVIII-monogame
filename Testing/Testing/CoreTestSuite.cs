using NUnit.Framework;
using System;

namespace OpenVIII.Tests
{
    [TestFixture]
    public class CoreTestSuite
    {
        #region Methods

        [Test]
        public void SimpleKernelTest()
        {
            Memory.Init(null, null, null, null);
            //Memory.Archives.A_WORLD._Root = Memory.FF8DIRdata_lang;
            //Memory.Archives.A_MAIN._Root = Memory.FF8DIRdata_lang;
            //Memory.Archives.A_MENU._Root = Memory.FF8DIRdata_lang;

            Memory.Strings = new Strings();

            //TODO initiate kernel_bin with out new;
            // ReSharper disable once ObjectCreationAsStatement
            new Kernel_bin();
            Console.WriteLine("Printing items: ");
            foreach (Kernel_bin.Non_battle_Items_Data item in Kernel_bin.s_nonbattleItemsData)
            {
                Console.WriteLine("item: " + item.ID + " - " + item.Name + " - " + item.Description);
            }
        }

        [Test]
        public void SimpleWorkerTest()
        {
            string[] fileNames = new[]
            {
                "music0.obj",
                "music1.obj",
                "music2.obj",
                "music3.obj",
                "music4.obj",
                "music5.obj",
                "rail.obj",
                "texl.obj",
                "wmset.obj",
                "wmsetXX.obj",
                "wmx.obj",
                "chara.one"
            };

            Memory.Init(null, null, null, null);

            ArchiveBase aw = ArchiveWorker.Load(Memory.Archives.A_WORLD);
            //Console.WriteLine(aw.archive);
            string[] wmxPath = aw.GetListOfFiles();
            Console.WriteLine("Files in Archive (" + Memory.Archives.A_WORLD + ")");
            for (int i = 0; i < wmxPath.Length; i++)
            {
                string filePath = wmxPath[i];
                Console.WriteLine(i + 1 + ".) " + filePath);
            }

            // From the wiki we know, that there are 12 files in the archive
            // http://wiki.ffrtt.ru/index.php/FF8#World_map_files
            Assert.AreEqual(wmxPath.Length, fileNames.Length);
        }

        #endregion Methods
    }
}