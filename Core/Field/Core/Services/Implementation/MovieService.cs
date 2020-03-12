using System;

namespace OpenVIII.Fields
{
    public sealed class MovieService : IMovieService
    {
        public Boolean IsSupported => true;
        

        public void PrepareToPlay(Int32 movieId, Boolean flag)
        {
            ModuleMovieTest.Index = movieId;
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MovieService)}.{nameof(PrepareToPlay)}({nameof(movieId)}: {movieId}, {nameof(flag)}: {flag})");
        }

        public void Play()
        {
            ModuleMovieTest.Play();
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MovieService)}.{nameof(Play)}()");
        }

        public void Wait()
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MovieService)}.{nameof(Wait)}()");
        }
    }
}