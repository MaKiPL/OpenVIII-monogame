using System;

namespace OpenVIII.Fields
{
    public sealed class MovieService : IMovieService
    {
        #region Properties

        public bool IsSupported => true;

        #endregion Properties

        #region Methods

        public void Play()
        {
            ModuleMovieTest.Play();
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MovieService)}.{nameof(Play)}()");
        }

        public void PrepareToPlay(int movieId, bool flag)
        {
            ModuleMovieTest.Index = movieId;
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MovieService)}.{nameof(PrepareToPlay)}({nameof(movieId)}: {movieId}, {nameof(flag)}: {flag})");
        }

        public void Wait() =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MovieService)}.{nameof(Wait)}()");

        #endregion Methods
    }
}