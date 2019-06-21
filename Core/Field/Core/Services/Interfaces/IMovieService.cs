using System;

namespace OpenVIII
{
    public interface IMovieService
    {
        Boolean IsSupported { get; }
        
        void PrepareToPlay(Int32 movieId, Boolean flag);
        void Play();
        void Wait();
    }
}