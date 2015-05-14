using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace CoopImage
{
    public class ImageRepository
    {
        public static byte[] Get(ImageRequest req)
        {
            // CACHED?
            var cache = CoopRelay.Tools.Cache.Get<byte[]>(GetCacheKey(req));
            if (cache != null)
                return cache;

            Bitmap imgIn = new Bitmap(req.ImagePath);
            ImageFormat imgInFormat = GetImageFormat(req.ImagePath);

            var r = GetResizeRatio(req, imgIn);

            int outWidth = (int)(imgIn.Width * r.Width);
            int outHeight = (int)(imgIn.Height * r.Height);

            using (var outStream = new System.IO.MemoryStream())
            {
                using (var imgOut = new Bitmap(outWidth,outHeight))
                {
                    imgOut.SetResolution(72, 72);
                    using (var g = Graphics.FromImage(imgOut))
                    {
                        g.Clear(Color.White);
                        g.DrawImage(imgIn, 
                            new Rectangle(0, 0, outWidth, outHeight), 
                            new Rectangle(0, 0, imgIn.Width, imgIn.Height), 
                            GraphicsUnit.Pixel);

                        // WATERMARK
                        if (!string.IsNullOrEmpty(req.Watermark))
                        {
                            using (var stream = new FileStream(req.Watermark, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                using (var overlay = System.Drawing.Image.FromStream(stream))
                                {
                                    stream.Close();
                                    g.DrawImage(overlay, 
                                        new Rectangle(outWidth - overlay.Width - 20, outHeight - overlay.Height - 20, overlay.Width, overlay.Height),
                                        0, 
                                        0, 
                                        overlay.Width, 
                                        overlay.Height,
                                        GraphicsUnit.Pixel);
                                }
                            }
                        }
                        if (imgInFormat == ImageFormat.Jpeg)
                        {
                            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                            var encoderparameters = GetEncoderParameters((long)req.Quality);
                            imgOut.Save(outStream, jpgEncoder, encoderparameters);
                        }
                        else
                        {
                            imgOut.Save(outStream, imgInFormat);
                        }
                        var results = outStream.ToArray();
                        CoopRelay.Tools.Cache.Insert(GetCacheKey(req), results, CoopRelay.Tools.AppSettings.CacheInterval);
                        return results;
                    }
                };
            };
        }

        public static string GetCacheKey(ImageRequest req)
        {
            return string.Format("CoopImage_{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}",
                req.ImagePath,
                req.Constrain,
                req.Height,
                req.Width,
                req.MaxHeight,
                req.MaxWidth,
                req.Quality,
                req.Watermark
            );
        }

        private static ImageFormat GetImageFormat(string imagepath)
        {
            switch (Path.GetExtension(imagepath))
            {
                case ".bmp": return ImageFormat.Bmp;
                case ".gif": return ImageFormat.Gif;
                case ".jpg": return ImageFormat.Jpeg;
                case ".png": return ImageFormat.Png;
                default: break;
            }
            return ImageFormat.Jpeg;
        }

        private static ImageResizeRatio GetResizeRatio(ImageRequest req, Bitmap imgIn)
        {
            var r = new ImageResizeRatio();

            r.Height = 1;
            r.Width = 1;

            // HEIGHT WIDTH
            if (req.Height > 0)
                r.Height = req.Height / (double)imgIn.Height;
            if (req.Width > 0)
                r.Width = req.Width / (double)imgIn.Width;

            // MAXHEIGHT MAXWIDTH
            if (0 < req.MaxHeight)
            {
                if (req.MaxHeight < imgIn.Height)
                    r.Height = req.MaxHeight / (double)imgIn.Height;
            }
            if (0 < req.MaxWidth)
            {
                if (req.MaxWidth < imgIn.Width)
                    r.Width = req.MaxWidth / (double)imgIn.Width;
            }

            //CONSTRAIN
            if (r.Height < r.Width)
                r.Height = r.Width;
            else
                r.Width = r.Height;

            return r;
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private static EncoderParameters GetEncoderParameters(long quality)
        {
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

            EncoderParameters myEncoderParameters = new EncoderParameters(1);

            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, (long)quality);
            myEncoderParameters.Param[0] = myEncoderParameter;
            return myEncoderParameters;
        }
    }
}
