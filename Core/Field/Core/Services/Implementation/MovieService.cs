using System;

namespace OpenVIII
{
    public sealed class MovieService : IMovieService
    {
        public Boolean IsSupported => true;
        

        public void PrepareToPlay(Int32 movieId, Boolean flag)
        {
            Module_movie_test.Index = movieId;
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MovieService)}.{nameof(PrepareToPlay)}({nameof(movieId)}: {movieId}, {nameof(flag)}: {flag})");
        }

        public void Play()
        {
            Module_movie_test.Play();
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