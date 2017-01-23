// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.IO;
using Avalonia.Platform;
using SharpDX.WIC;

namespace Avalonia.Direct2D1.Media
{
    /// <summary>
    /// A WIC implementation of a <see cref="Avalonia.Media.Imaging.Bitmap"/>.
    /// </summary>
    public class WicBitmapImpl : BitmapImpl
    {
        private readonly ImagingFactory _factory;

        private SharpDX.Direct2D1.Bitmap _direct2D;

        /// <summary>
        /// Initializes a new instance of the <see cref="WicBitmapImpl"/> class.
        /// </summary>
        /// <param name="factory">The WIC imaging factory to use.</param>
        /// <param name="fileName">The filename of the bitmap to load.</param>
        public WicBitmapImpl(ImagingFactory factory, string fileName)
        {
            _factory = factory;

            using (BitmapDecoder decoder = new BitmapDecoder(factory, fileName, DecodeOptions.CacheOnDemand))
            {
                WicImpl = new Bitmap(factory, decoder.GetFrame(0), BitmapCreateCacheOption.CacheOnDemand);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WicBitmapImpl"/> class.
        /// </summary>
        /// <param name="factory">The WIC imaging factory to use.</param>
        /// <param name="stream">The stream to read the bitmap from.</param>
        public WicBitmapImpl(ImagingFactory factory, Stream stream)
        {
            _factory = factory;

            using (BitmapDecoder decoder = new BitmapDecoder(factory, stream, DecodeOptions.CacheOnLoad))
            {
                WicImpl = new Bitmap(factory, decoder.GetFrame(0), BitmapCreateCacheOption.CacheOnLoad);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WicBitmapImpl"/> class.
        /// </summary>
        /// <param name="factory">The WIC imaging factory to use.</param>
        /// <param name="width">The width of the bitmap.</param>
        /// <param name="height">The height of the bitmap.</param>
        public WicBitmapImpl(ImagingFactory factory, int width, int height)
        {
            _factory = factory;
            WicImpl = new Bitmap(
                factory,
                width,
                height,
                PixelFormat.Format32bppPBGRA,
                BitmapCreateCacheOption.CacheOnLoad);
        }

        /// <summary>
        /// Gets the width of the bitmap, in pixels.
        /// </summary>
        public override int PixelWidth => WicImpl.Size.Width;

        /// <summary>
        /// Gets the height of the bitmap, in pixels.
        /// </summary>
        public override int PixelHeight => WicImpl.Size.Height;

        public override void Dispose()
        {
            WicImpl.Dispose();
            _direct2D?.Dispose();
        }

        /// <summary>
        /// Gets the WIC implementation of the bitmap.
        /// </summary>
        public Bitmap WicImpl { get; }

        /// <summary>
        /// Gets a Direct2D bitmap to use on the specified render target.
        /// </summary>
        /// <param name="renderTarget">The render target.</param>
        /// <returns>The Direct2D bitmap.</returns>
        public override SharpDX.Direct2D1.Bitmap GetDirect2DBitmap(SharpDX.Direct2D1.RenderTarget renderTarget)
        {
            FormatConverter converter = new FormatConverter(_factory);
            converter.Initialize(WicImpl, PixelFormat.Format32bppPBGRA);
            return SharpDX.Direct2D1.Bitmap.FromWicBitmap(renderTarget, converter);
        }

        /// <summary>
        /// Saves the bitmap to a file.
        /// </summary>
        /// <param name="fileName">The filename.</param>
        public override void Save(string fileName)
        {
            if (Path.GetExtension(fileName) != ".png")
            {
                // Yeah, we need to support other formats.
                throw new NotSupportedException("Use PNG, stoopid.");
            }

            using (FileStream s = new FileStream(fileName, FileMode.Create))
            {
                Save(s);
            }
        }

        public override void Save(Stream stream)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder(_factory);
            encoder.Initialize(stream);

            BitmapFrameEncode frame = new BitmapFrameEncode(encoder);
            frame.Initialize();
            frame.WriteSource(WicImpl);
            frame.Commit();
            encoder.Commit();
        }
    }
}
