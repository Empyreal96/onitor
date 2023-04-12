using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.Streams;

namespace WebViewComponents.VideoEffects
{
    public sealed class BackgroundSet
    {
        public string Key { get; }
        public IEnumerable<StorageFile> Frames { get; }
        public double FramesPerSecond { get; }

        public BackgroundSet(string key, IEnumerable<StorageFile> frames, double framesPerSecond)
        {
            Key = key;
            Frames = frames;
            FramesPerSecond = framesPerSecond;
        }
    }
}
