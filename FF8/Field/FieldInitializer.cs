using FF8.Core;
using System;
using System.Linq;

namespace FF8
{
    public static class FieldInitializer
    {
        /// <summary>
        /// Should be called only once
        /// </summary>
        public static void Init()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_FIELD);
            string[] lists = aw.GetListOfFiles();
            string maplist = lists.First(x => x.ToLower().Contains("mapdata.fs"));
            maplist = maplist.Substring(0,maplist.Length - 3);
            byte[] fs = ArchiveWorker.GetBinaryFile(Memory.Archives.A_FIELD, $"{maplist}{Memory.Archives.B_FileArchive}");
            byte[] fl = ArchiveWorker.GetBinaryFile(Memory.Archives.A_FIELD, $"{maplist}{Memory.Archives.B_FileList}");
            byte[] fi = ArchiveWorker.GetBinaryFile(Memory.Archives.A_FIELD, $"{maplist}{Memory.Archives.B_FileIndex}");
            string map = System.Text.Encoding.UTF8.GetString(fl).TrimEnd();
            string[] maplistb = System.Text.Encoding.UTF8.GetString(
                ArchiveWorker.FileInTwoArchives(fi, fs, fl, map))
                .Replace("\r", "")
                .Split('\n');
            Memory.FieldHolder.fields = maplistb;
            Core.FieldId.FieldId_ = maplistb;


        }

        public static IServices GetServices()
        {
            ServiceProvider services = new ServiceProvider();
            EventEngine engine = new EventEngine();

            services.Register(ServiceId.Interaction, new InteractionService());
            services.Register(ServiceId.Field, new FieldService(engine));
            services.Register(ServiceId.Global, new GlobalVariableService());
            services.Register(ServiceId.Gameplay, new GameplayService());
            services.Register(ServiceId.Salary, new SalaryService());
            services.Register(ServiceId.Party, new PartyService());
            services.Register(ServiceId.Movie, new MovieService());
            services.Register(ServiceId.Message, new MessageService());
            services.Register(ServiceId.Menu, new MenuService());
            services.Register(ServiceId.Music, new MusicService());
            services.Register(ServiceId.Sound, new SoundService());
            services.Register(ServiceId.Rendering, new RenderingService());

            return services;
        }
    }
}