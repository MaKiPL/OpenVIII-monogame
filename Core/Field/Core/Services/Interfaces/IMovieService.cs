using System;

namespace OpenVIII.Fields
{
    public interface IMovieService
    {
        #region Properties

        bool IsSupported { get; }

        #endregion Properties

        #region Methods

        void Play();

        void PrepareToPlay(int movieId, bool flag);

        void Wait();

        #endregion Methods
    }
}