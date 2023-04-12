using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace WebViewComponents.VideoEffects
{
    public sealed class BackgroundMediaExtension : IMediaExtension
    {
        #region Properties
        internal IReadOnlyDictionary<string, Task<CanvasBitmap[]>> BackgroundSetCache { get; private set; } = new Dictionary<string, Task<CanvasBitmap[]>>();

        /// <summary>
        /// Dicoverability only.
        /// </summary>
        public BackgroundSet BackgroundSet
        {
            get;
            /*get => throw new NotImplementedException(
                "This property exists for discoverability only. You can set it via SetProperties() and it'll be cached." +
                $"Use {nameof(BackgroundSetCache)} to access values.");*/
        }

        /// <summary>
        /// Gets the currently active background set.
        /// Settable via <see cref="BackgroundSet"/>.
        /// </summary>
        public string ActiveBackgroundSet { get; private set; }

        /// <summary>
        /// Gets the currently active background set.
        /// Settable via <see cref="BackgroundSet"/>.
        /// </summary>
        public double ActiveBackgroundFramesPerSecond { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the compositor is time-independent
        /// </summary>
        public bool TimeIndependent => true;

        /// <summary>
        /// Gets the encoding properties for the media
        /// </summary>
        internal VideoEncodingProperties EncodingProperties { get; private set; }

        /// <summary>
        /// Gets the canvas drawing device for the media
        /// </summary>
        internal CanvasDevice Device { get; private set; }

        #endregion


        #region Constructors

        internal BackgroundMediaExtension() { }

        #endregion


        #region Methods

        /// <summary>
        /// Sets the properties passed into the compositor
        /// </summary>
        /// <param name="configuration">the configuration</param>
        public void SetProperties(IPropertySet configuration)
        {
            if (configuration == null)
            {
                return;
            }

            try
            {
                if (configuration.TryGetValue(nameof(BackgroundSet), out var backgroundSet))
                {
                    var set = (BackgroundSet)backgroundSet;
                    if (!BackgroundSetCache.ContainsKey(set.Key))
                    {
                        async Task<CanvasBitmap> CreateBitmap(StorageFile file)
                        {
                            using (var stream = await file.OpenReadAsync())
                            {
                                var bitmap = await CanvasBitmap.LoadAsync(Device, stream).AsTask();
                                return bitmap;
                            }
                        }

                        var assets = Task.WhenAll(set.Frames.Select(CreateBitmap));

                        lock (BackgroundSetCache)
                        {
                            var updatedCache = BackgroundSetCache.ToDictionary(kv => kv.Key, kv => kv.Value);
                            updatedCache[set.Key] = assets;
                            BackgroundSetCache = updatedCache;
                            ActiveBackgroundSet = set.Key;
                            ActiveBackgroundFramesPerSecond = set.FramesPerSecond;
                        }
                    }

                    else
                    {
                        lock (BackgroundSetCache)
                        {
                            ActiveBackgroundSet = set.Key;
                            ActiveBackgroundFramesPerSecond = set.FramesPerSecond;
                        }
                    }
                }
            }catch(Exception ex)
            {

            }
        }

        /// <summary>
        /// Sets the encoding properties
        /// </summary>
        /// <param name="encodingProperties">the encoding properties</param>
        /// <param name="device">the Direct3D device</param>
        public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device)
        {
            try
            {
                EncodingProperties = encodingProperties;
                Device = CanvasDevice.CreateFromDirect3D11Device(device);
            }catch(Exception ex)
            {

            }
        }

        /// <summary>
        /// Close the compositor & dispose of the canvas device
        /// </summary>
        /// <param name="reason">the media effect closed reason</param>
        public void Close(MediaEffectClosedReason reason)
        {
            if (Device != null)
            {
                try
                {
                    Device.Dispose();
                    Device = null;
                }catch(Exception ex)
                {

                }
            }

            try
            {
                if (BackgroundSetCache != null)
                {
                    foreach (var set in BackgroundSetCache)
                        foreach (var bg in set.Value.GetAwaiter().GetResult())
                            bg.Dispose();
                }
            }catch(Exception ex)
            {

            }
        }

        /// <summary>
        /// Discard of the queued frames
        /// </summary>
        /// <remarks>this does nothing</remarks>
        public void DiscardQueuedFrames() { }

        #endregion
    }
}
