using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;

namespace WebViewComponents.VideoEffects
{
    /// <summary>
    /// Chroma key compositor
    /// <list type="Bullet">
    /// <listheader>Properties</listheader>
    /// <item>Color (Color) : Chroma key color (default is black)</item>
    /// <item>Feather (Boolean): true to soften edges of the output (default is false)</item>
    /// <item>Tolerance (float): Color tolerance 0-1 (default is 0.1)</item>
    /// <item>InvertAlpha (Boolean): invert the alpha value (default is false)</item>
    /// </list>
    /// </summary>
    public sealed class ChromaKeyVideoCompositor : IVideoCompositor
    {
        private readonly ChromaKeyMediaExtension MediaExtension = new ChromaKeyMediaExtension();

        /// <summary>
        /// Composite the frame
        /// </summary>
        /// <param name="context">the composite frame context</param>
        public void CompositeFrame(CompositeVideoFrameContext context)
        {
            try
            {
                foreach (var surface in context.SurfacesToOverlay)
                {
                    using (var inputBitmap = CanvasBitmap.CreateFromDirect3D11Surface(MediaExtension.Device, surface))
                    using (var renderTarget = CanvasRenderTarget.CreateFromDirect3D11Surface(MediaExtension.Device, context.OutputFrame.Direct3DSurface))
                    using (var ds = renderTarget.CreateDrawingSession())
                    using (var chromaKeyEffect = MediaExtension.CreateChromaKeyEffect(inputBitmap))
                    {
                        var overlay = context.GetOverlayForSurface(surface);
                        var destinationRectangle = overlay.Position;
                        var sourceRectangle = inputBitmap.Bounds;
                        var opacity = System.Convert.ToSingle(overlay.Opacity);

                        ds.DrawImage(chromaKeyEffect, destinationRectangle, sourceRectangle, opacity);
                    }
                }
            }catch(Exception ex)
            {

            }
        }

        public void SetEncodingProperties(VideoEncodingProperties backgroundProperties, IDirect3DDevice device) =>
            MediaExtension.SetEncodingProperties(backgroundProperties, device);

        public void Close(MediaEffectClosedReason reason) =>
            MediaExtension.Close(reason);

        public void DiscardQueuedFrames() =>
            MediaExtension.DiscardQueuedFrames();

        public bool TimeIndependent =>
            MediaExtension.TimeIndependent;

        public void SetProperties(IPropertySet configuration) =>
            MediaExtension.SetProperties(configuration);
    }
}

