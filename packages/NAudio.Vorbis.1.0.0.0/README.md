NAudio.Vorbis    [![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/naudio/Vorbis?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
-------

NAudio.Vorbis is a convenience wrapper to enable easy integration of [NVorbis](https://github.com/ioctlLR/NVorbis) into NAudio projects.

To use:

```cs
// add a reference to NVorbis.dll
// add a reference to NAudio.Vorbis.dll

using (var vorbisStream = new NAudio.Vorbis.VorbisWaveReader("path/to/file.ogg"))
using (var waveOut = new NAudio.Wave.WaveOutEvent())
{
    waveOut.Init(vorbisStream);
    waveOut.Play();
   
    // wait here until playback stops or should stop
}
```

If you have any questions or comments, feel free to join us on Gitter.  If you have any issues or feature requests, please submit them in the issue tracker.
