using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics.Imaging;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.UI;

namespace WebViewComponents.VideoEffects
{
    public sealed class BackgroundVideoEffect : IBasicVideoEffect
    {
        private readonly BackgroundMediaExtension MediaExtension = new BackgroundMediaExtension();

        /// <summary>
        /// Gets a value indicating whether the video effect will modify the contents of the input frame.
        /// </summary>
        public bool IsReadOnly { get; } = false;

        /// <summary>
        /// Gets the encoding properties supported by the custom video effect.
        /// </summary>
        public IReadOnlyList<VideoEncodingProperties> SupportedEncodingProperties { get; } = new List<VideoEncodingProperties>
            {  new VideoEncodingProperties { Subtype = "ARGB32" } };

        /// <summary>
        /// Gets a value that indicates whether the custom video effect supports the use of GPU memory or CPU memory.
        /// </summary>
        public MediaMemoryTypes SupportedMemoryTypes { get; } = MediaMemoryTypes.Gpu;

        private ICanvasImage BackgroundImage { get; set; } = default(ICanvasImage);

        public void ProcessFrame(ProcessVideoFrameContext context)
        {
            try
            {
                using (var inputBitmap = CanvasBitmap.CreateFromDirect3D11Surface(MediaExtension.Device, context.InputFrame.Direct3DSurface))
                using (var renderTarget = CanvasRenderTarget.CreateFromDirect3D11Surface(MediaExtension.Device, context.OutputFrame.Direct3DSurface))
                using (var ds = renderTarget.CreateDrawingSession())
                {
                    if (MediaExtension.ActiveBackgroundSet != null &&
                        MediaExtension.BackgroundSetCache != null &&
                        MediaExtension.BackgroundSetCache.TryGetValue(
                            key: MediaExtension.ActiveBackgroundSet, value: out var set) &&
                        set.IsCompleted &&
                        set.Result.Length > 0)
                    {
                        try
                        {
                            set.Wait();
                        }
                        catch (AggregateException exception)
                        {
                            // .NET async tasks wrap all errors in an AggregateException.
                            // We unpack this so Win2D can directly see any lost device errors.
                            exception.Handle(ex => { throw ex; });
                        }

                        var framesPerMillisecond = MediaExtension.ActiveBackgroundFramesPerSecond / 1000;
                        var durationInMilliseconds = set.Result.Length / framesPerMillisecond;
                        var offsetInDuration = context.InputFrame.RelativeTime?.TotalMilliseconds % durationInMilliseconds / durationInMilliseconds ?? 0;
                        var offsetInFrames = Convert.ToInt32(set.Result.Length * offsetInDuration);

                        // Does someone want to fix my algorithm so this isn't necessary?
                        var offsetInIndex = offsetInFrames <= 0 ? 0
                                          : offsetInFrames >= set.Result.Length ? set.Result.Length - 1
                                          : offsetInFrames;
                        var canvas = set.Result[offsetInIndex];

                        var scale = inputBitmap.Bounds.Width / canvas.Bounds.Width;
                        var bounds = canvas.Bounds;
                        bounds.Width = bounds.Width * scale;
                        bounds.Height = bounds.Height * scale;

                        ds.DrawImage(canvas, bounds);
                    }
                    else
                    {
                        ds.FillRectangle(inputBitmap.Bounds, Colors.Black);
                    }

                    ds.DrawImage(inputBitmap);
                }
            }catch(Exception ex)
            {
                
            }
        }

        public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device) =>
           MediaExtension.SetEncodingProperties(encodingProperties, device);

        public void Close(MediaEffectClosedReason reason)
        {
            try
            {
                BackgroundImage?.Dispose();
                MediaExtension.Close(reason);
            }catch(Exception ex)
            {

            }
        }

        public void DiscardQueuedFrames() =>
            MediaExtension.DiscardQueuedFrames();

        public bool TimeIndependent =>
            MediaExtension.TimeIndependent;

        public void SetProperties(IPropertySet configuration)
        {
            MediaExtension.SetProperties(configuration);
        }
    }
}
