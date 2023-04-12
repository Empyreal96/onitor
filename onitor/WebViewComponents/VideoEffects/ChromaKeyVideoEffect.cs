using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;

namespace WebViewComponents.VideoEffects
{
    public sealed class ChromaKeyVideoEffect : IBasicVideoEffect
    {
        private readonly ChromaKeyMediaExtension MediaExtension = new ChromaKeyMediaExtension();

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

        //private ICanvasImage[] BackgroundImages { get; set; } = default(ICanvasImage[]);

        public void ProcessFrame(ProcessVideoFrameContext context)
        {
            try
            {
                using (var inputBitmap = CanvasBitmap.CreateFromDirect3D11Surface(MediaExtension.Device, context.InputFrame.Direct3DSurface))
                using (var renderTarget = CanvasRenderTarget.CreateFromDirect3D11Surface(MediaExtension.Device, context.OutputFrame.Direct3DSurface))
                using (var ds = renderTarget.CreateDrawingSession())
                using (var chromaKeyEffect = MediaExtension.CreateChromaKeyEffect(inputBitmap))
                {
                    ds.Blend = CanvasBlend.Copy;
                    ds.DrawImage(chromaKeyEffect);
                }
            }catch(Exception ex)
            {

            }
        }

        public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device) =>
           MediaExtension.SetEncodingProperties(encodingProperties, device);

        public void Close(MediaEffectClosedReason reason) =>
            MediaExtension.Close(reason);

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
