using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Numerics;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;

namespace WebViewComponents.VideoEffects
{
    public sealed class ZoomMediaExtension : IMediaExtension
    {
        #region Properties

        /// <summary>
        /// Gets the scale of the zoom effect
        /// </summary>
        public float Scale { get; private set; } = 1f;

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

        internal ZoomMediaExtension() { }

        #endregion


        #region Methods

        public Matrix3x2 CreateTransform(CanvasBitmap inputBitmap)
        {
            var centerPoint = inputBitmap.Size.ToVector2() / 2;
            return Matrix3x2.CreateScale(Scale, centerPoint);
        }

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

            if (configuration.TryGetValue(nameof(Scale), out var scale))
            {
                Scale = (float)scale;
            }
        }

        /// <summary>
        /// Sets the encoding properties
        /// </summary>
        /// <param name="encodingProperties">the encoding properties</param>
        /// <param name="device">the Direct3D device</param>
        public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device)
        {try
            {
                if (Device != null) { 
                Device.Dispose();
                Device = null;
                }
            }
            catch (Exception ex) { 
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
