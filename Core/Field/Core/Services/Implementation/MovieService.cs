using System;

namespace FF8
{
    public sealed class MovieService : IMovieService
    {
        public Boolean IsSupported => true;

        public void PrepareToPlay(Int32 movieId, Boolean flag)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MovieService)}.{nameof(PrepareToPlay)}({nameof(movieId)}: {movieId}, {nameof(flag)}: {flag})");
        }

        public void Play()
        {
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