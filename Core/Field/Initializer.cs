using System;
using System.Linq;

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
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_FIELD);
            
            ArchiveBase mapdata = aw.GetArchive("mapdata.fs");
            if (mapdata != null)
            {

                string[] v = mapdata.GetListOfFiles();
                string map = v?[0];
                if (map != null)
                {
                    byte[] bytes = mapdata.GetBinaryFile(map);
                    if (bytes != null)
                    {
                        string[] maplistb = System.Text.Encoding.UTF8.GetString(bytes)
                            .Replace("\r", "")
                            .Split('\n');
                        Memory.FieldHolder.fields = maplistb;
                        FieldId.FieldId_ = maplistb;
                    }
                }
            }
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