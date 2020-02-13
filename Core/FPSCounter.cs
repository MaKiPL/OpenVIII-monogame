using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// https://stackoverflow.com/questions/20676185/xna-monogame-getting-the-frames-per-second
    /// </summary>
    internal static class FPSCounter
    {
        public static double CurrentFramesPerSecond { get; private set; }
        public static double AverageFramesPerSecond { get; private set; }

        private const int MAX_SAMPLES = 100;
        private static Queue<double> SAMPLES = new Queue<double>();

        public static void Update()
        {
            if (SAMPLES != null)
            {
                if (Memory.ElapsedGameTime.TotalSeconds > 0)
                    CurrentFramesPerSecond = 1.0d / Memory.ElapsedGameTime.TotalSeconds;
                SAMPLES.Enqueue(CurrentFramesPerSecond);
                while (SAMPLES.Count > MAX_SAMPLES)
                    SAMPLES.Dequeue();
                AverageFramesPerSecond = SAMPLES.Average(x => x);
            }
        }
    }
}