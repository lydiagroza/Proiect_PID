using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Avalonia.Platform.Storage;
using Emgu.CV;
using Emgu.CV.Structure;

using Framework.View;

namespace Framework.Utilities
{
    internal static class FileHelper
    {
        private static readonly FilePickerFileType ImageFiles = new FilePickerFileType("Image files")
        {
            Patterns = new[] { "*.jpg", "*.jpeg", "*.jfif", "*.jpe", "*.bmp", "*.png", "*.gif", "*.webp" },
            AppleUniformTypeIdentifiers = new[] { "public.image" },
            MimeTypes = new[] { "image/*" }
        };

        private static readonly FilePickerFileType JpegFiles = new FilePickerFileType("JPEG image")
        {
            Patterns = new[] { "*.jpg", "*.jpeg" },
            AppleUniformTypeIdentifiers = new[] { "public.jpeg" },
            MimeTypes = new[] { "image/jpeg" }
        };

        private static readonly FilePickerFileType PngFiles = new FilePickerFileType("PNG image")
        {
            Patterns = new[] { "*.png" },
            AppleUniformTypeIdentifiers = new[] { "public.png" },
            MimeTypes = new[] { "image/png" }
        };

        private static readonly FilePickerFileType BmpFiles = new FilePickerFileType("BMP image")
        {
            Patterns = new[] { "*.bmp" },
            AppleUniformTypeIdentifiers = new[] { "com.microsoft.bmp" },
            MimeTypes = new[] { "image/bmp" }
        };

        #region Show File Dialog for loading an image
        public static async Task<string> LoadFileDialog(string title)
        {
            IStorageProvider storageProvider = UiHelper.StorageProvider;
            if (storageProvider == null)
                return null;

            IReadOnlyList<IStorageFile> files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = new[] { ImageFiles, FilePickerFileTypes.All }
            });

            return files.Select(file => file.TryGetLocalPath()).FirstOrDefault(path => path != null);
        }
        #endregion

        #region Load an image
        public static Image<TColor, byte> LoadImage<TColor>(string imagePath)
            where TColor : struct, IColor
        {
            using (var image = new Image<Bgr, byte>(ImageCodec.DecodeBgr(imagePath)))
                return image.Convert<TColor, byte>();
        }
        #endregion

        #region Show File Dialog for saving an image
        public static async Task<string> SaveFileDialog(string imageName)
        {
            IStorageProvider storageProvider = UiHelper.StorageProvider;
            if (storageProvider == null)
                return null;

            IStorageFile file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save the processed image",
                SuggestedFileName = imageName,
                DefaultExtension = "jpg",
                FileTypeChoices = new[] { JpegFiles, PngFiles, BmpFiles },
                ShowOverwritePrompt = true
            });

            return file?.TryGetLocalPath();
        }
        #endregion

        #region Save an image
        public static void SaveImage<TColor>(Image<TColor, byte> image, string imagePath)
            where TColor : struct, IColor
        {
            if (image.NumberOfChannels == 1)
            {
                ImageCodec.Encode(imagePath, image.Data, image.Width, image.Height);
                return;
            }

            using (var colorImage = image.Convert<Bgr, byte>())
                ImageCodec.Encode(imagePath, colorImage.Data, colorImage.Width, colorImage.Height);
        }
        #endregion
    }
}
