using System;

namespace FF8.Core
{
    public interface IMovieService
    {
        Boolean IsSupported { get; }
        
        void PrepareToPlay(Int32 movieId, Boolean flag);
        void Play();
        void Wait();
    }
}