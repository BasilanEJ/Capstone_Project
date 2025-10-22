using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace RRCManagementSystem.Helpers
{
    public static class ImageHelper
    {
        /// <summary>
        /// Resizes and optimizes an image to specific dimensions
        /// </summary>
        public static string ResizeAndSaveImage(Stream inputStream, string savePath, int maxWidth, int maxHeight, bool maintainAspect = true)
        {
            try
            {
                using (Image originalImage = Image.FromStream(inputStream))
                {
                    int newWidth, newHeight;

                    if (maintainAspect)
                    {
                        // Calculate new dimensions maintaining aspect ratio
                        double ratioX = (double)maxWidth / originalImage.Width;
                        double ratioY = (double)maxHeight / originalImage.Height;
                        double ratio = Math.Min(ratioX, ratioY);

                        newWidth = (int)(originalImage.Width * ratio);
                        newHeight = (int)(originalImage.Height * ratio);
                    }
                    else
                    {
                        // Force exact dimensions (may distort)
                        newWidth = maxWidth;
                        newHeight = maxHeight;
                    }

                    // Create new bitmap with calculated dimensions
                    using (Bitmap resizedImage = new Bitmap(newWidth, newHeight))
                    {
                        using (Graphics graphics = Graphics.FromImage(resizedImage))
                        {
                            // High quality resize settings
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = SmoothingMode.HighQuality;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            graphics.CompositingQuality = CompositingQuality.HighQuality;

                            // Draw the resized image
                            graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
                        }

                        // Determine image format from original
                        ImageFormat format = originalImage.RawFormat;

                        // Use JPEG for photos, PNG for graphics with transparency
                        if (format.Equals(ImageFormat.Png) || format.Equals(ImageFormat.Gif))
                        {
                            resizedImage.Save(savePath, ImageFormat.Png);
                        }
                        else
                        {
                            // Save as JPEG with quality compression
                            SaveJpegWithQuality(resizedImage, savePath, 85L);
                        }
                    }
                }

                return savePath;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to resize image: " + ex.Message);
            }
        }

        /// <summary>
        /// Save JPEG with specified quality level
        /// </summary>
        private static void SaveJpegWithQuality(Image image, string path, long quality)
        {
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);

            ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
            image.Save(path, jpegCodec, encoderParams);
        }

        /// <summary>
        /// Get image encoder for specified MIME type
        /// </summary>
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == mimeType)
                    return codec;
            }
            return null;
        }

        /// <summary>
        /// Crop image to exact dimensions from center
        /// </summary>
        public static string CropAndSaveImage(Stream inputStream, string savePath, int width, int height)
        {
            try
            {
                using (Image originalImage = Image.FromStream(inputStream))
                {
                    // Calculate crop area (center crop)
                    int sourceX = Math.Max(0, (originalImage.Width - width) / 2);
                    int sourceY = Math.Max(0, (originalImage.Height - height) / 2);

                    int sourceWidth = Math.Min(width, originalImage.Width);
                    int sourceHeight = Math.Min(height, originalImage.Height);

                    using (Bitmap croppedImage = new Bitmap(width, height))
                    {
                        using (Graphics graphics = Graphics.FromImage(croppedImage))
                        {
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = SmoothingMode.HighQuality;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                            Rectangle destRect = new Rectangle(0, 0, width, height);
                            Rectangle sourceRect = new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight);

                            graphics.DrawImage(originalImage, destRect, sourceRect, GraphicsUnit.Pixel);
                        }

                        SaveJpegWithQuality(croppedImage, savePath, 85L);
                    }
                }

                return savePath;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to crop image: " + ex.Message);
            }
        }

        /// <summary>
        /// Get recommended dimensions for different image sections
        /// </summary>
        public static class RecommendedDimensions
        {
            // Hero Banner - Wide format
            public static readonly Size HeroBanner = new Size(1920, 600);

            // Service Cards - Square format
            public static readonly Size ServiceCard = new Size(400, 400);

            // About Section - Portrait format
            public static readonly Size About = new Size(600, 800);

            // Video Thumbnail - 16:9 aspect ratio
            public static readonly Size VideoThumbnail = new Size(1280, 720);

            // Blog Cards - Landscape format
            public static readonly Size BlogCard = new Size(640, 400);

            // C&O Banner - Wide format
            public static readonly Size COBanner = new Size(1200, 400);
        }

        /// <summary>
        /// Validate uploaded image
        /// </summary>
        public static bool ValidateImage(Stream imageStream, out string errorMessage, int maxSizeMB = 5)
        {
            errorMessage = string.Empty;

            try
            {
                // Check file size
                if (imageStream.Length > maxSizeMB * 1024 * 1024)
                {
                    errorMessage = $"Image size must be less than {maxSizeMB}MB";
                    return false;
                }

                // Verify it's a valid image
                using (Image img = Image.FromStream(imageStream))
                {
                    // Check minimum dimensions
                    if (img.Width < 200 || img.Height < 200)
                    {
                        errorMessage = "Image must be at least 200x200 pixels";
                        return false;
                    }

                    // Verify supported format
                    if (!img.RawFormat.Equals(ImageFormat.Jpeg) &&
                        !img.RawFormat.Equals(ImageFormat.Png) &&
                        !img.RawFormat.Equals(ImageFormat.Gif))
                    {
                        errorMessage = "Only JPG, PNG, and GIF formats are supported";
                        return false;
                    }
                }

                // Reset stream position
                imageStream.Position = 0;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Invalid image file: " + ex.Message;
                return false;
            }
        }
    }
}