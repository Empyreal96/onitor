using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.UI;

namespace WebViewComponents.VideoEffects
{
    public sealed class ChromaKeyMediaExtension : IMediaExtension
    {
        #region Properties

        /// <summary>
        /// Gets the chroma-key color
        /// </summary>
        public Color Color { get; private set; } = Colors.Black;

        /// <summary>
        /// Gets a value indicating whether to feather the edges of the chroma key
        /// </summary>
        public bool Feather { get; private set; } = false;

        /// <summary>
        /// Gets the color tolerance 
        /// </summary>
        public float Tolerance { get; private set; } = 0.1f;

        /// <summary>
        /// Gets a value indicating whether to invert the alpha transparency
        /// </summary>
        public bool InvertAlpha { get; private set; } = false;

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

        internal ChromaKeyMediaExtension() { }

        #endregion


        #region Methods

        internal ChromaKeyEffect CreateChromaKeyEffect(CanvasBitmap inputBitmap) => new ChromaKeyEffect
        {
            Source = inputBitmap,
            Color = Color,
            Feather = Feather,
            Tolerance = Tolerance,
            InvertAlpha = InvertAlpha
        };

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

            if (configuration.TryGetValue(nameof(Color), out var color))
            {
                Color = (Color)color;
            }

            if (configuration.TryGetValue(nameof(Tolerance), out var tolerance))
            {
                Tolerance = (float)tolerance;
            }

            if (configuration.TryGetValue(nameof(Feather), out var feather))
            {
                Feather = (bool)feather;
            }

            if (configuration.TryGetValue(nameof(InvertAlpha), out var invertAlpha))
            {
                InvertAlpha = (bool)invertAlpha;
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
        }

        /// <summary>
        /// Discard of the queued frames
        /// </summary>
        /// <remarks>this does nothing</remarks>
        public void DiscardQueuedFrames() { }

        #endregion
    }
}
