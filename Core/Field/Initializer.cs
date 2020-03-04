namespace OpenVIII.Fields
{
    public static class Initializer
    {
        /// <summary>
        /// Should be called only once
        /// </summary>
        public static void Init()
        {
            Memory.Log.WriteLine($"{nameof(Fields)} :: {nameof(Initializer)} :: {nameof(Init)}");
            ArchiveBase aw = ArchiveWorker.Load(Memory.Archives.A_FIELD);
            
            // ReSharper disable once StringLiteralTypo
            ArchiveBase mapData = aw.GetArchive("mapdata.fs");
            if (mapData == null) return;
            // ReSharper disable once StringLiteralTypo
            const string map = "maplist";
            byte[] bytes = mapData.GetBinaryFile(map);
            if (bytes == null) return;
            string[] mapList = System.Text.Encoding.UTF8.GetString(bytes)
                .Replace("\r", "")
                .Split('\n');

            //Memory.FieldHolder.FieldMemory = new int[1024];
            Memory.FieldHolder.fields = mapList;
            FieldId.FieldId_ = mapList;
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