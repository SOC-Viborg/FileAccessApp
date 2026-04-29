using FileAccessApp.Domain;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.StaticFiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FileAccessApp.Services
{
    public class FileService
    {
        public FileService(IConfiguration configuration)
        {
            this.Root = "/app/data/files";

            if (!Directory.Exists(this.Root))
            {
                Directory.CreateDirectory(this.Root);
            }
        }

        private readonly string Root;

        public void CreateDir(string currentPath, string dirName)
        {
            string absolutePath = Path.Combine(Root, currentPath, dirName);
            Directory.CreateDirectory(absolutePath);
        }

        public async Task UploadFileAsync(string currentPath, IBrowserFile file)
        {
            string absolutePath = Path.Combine(Root, currentPath);
            var safeFileName = Path.GetFileName(file.Name);

            string filePath = Path.Combine(absolutePath, safeFileName);

            if (File.Exists(filePath))
            {
                throw new IOException("File already exists.");
            }

            using (var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024))
            using (var fileStream = File.Create(filePath))
            {
                await stream.CopyToAsync(fileStream);
            }
        }

        public void DeleteEntry(string filePath)
        {
            string absolutePath = Path.Combine(Root, filePath);

            // Prevent path traversal (../../ etc.)
            if (!absolutePath.StartsWith(Path.GetFullPath(Root)))
            {
                throw new UnauthorizedAccessException("Invalid path");
            }

            if (Directory.Exists(absolutePath))
            {
                // Delete folder (including contents)
                Directory.Delete(absolutePath, recursive: true);
            }
            else if (File.Exists(absolutePath))
            {
                // Delete file
                File.Delete(absolutePath);
            }
        }

        public List<FileDomain> GetFiles(string? currentPath)
        {
            string absolutePath = (string.IsNullOrEmpty(currentPath))
                ? Root
                : Path.Combine(Root, currentPath);

            if (!Directory.Exists(absolutePath))
                return new List<FileDomain>();

            try
            {
                DirectoryInfo dir = new DirectoryInfo(absolutePath);

                return dir.GetFileSystemInfos()
                    .Select(entry => new FileDomain(
                        entry.Name,
                        entry.Attributes.HasFlag(FileAttributes.Directory)
                    ))
                    .ToList();
            }
            catch (UnauthorizedAccessException)
            {
                return new List<FileDomain>();
            }
        }

        public (byte[] Content, string ContentType) GetFile(string filePath, int? maxWidth, int? maxHeight)
        {
            string absolutePath = Path.Combine(Root, filePath);

            if (!absolutePath.StartsWith(Root))
                throw new Exception("Invalid path");

            if (!File.Exists(absolutePath))
                throw new Exception("The specified file does not exist");

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(absolutePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var fileBytes = File.ReadAllBytes(absolutePath);


            if (contentType.StartsWith("image/") && (maxWidth.HasValue || maxHeight.HasValue))
            {
                using var image = SixLabors.ImageSharp.Image.Load(fileBytes);

                int width = image.Width;
                int height = image.Height;

                // If no resizing needed, return original
                if ((!maxWidth.HasValue || width <= maxWidth) &&
                    (!maxHeight.HasValue || height <= maxHeight))
                {
                    return (fileBytes, contentType);
                }

                float widthRatio = maxWidth.HasValue ? (float)maxWidth.Value / width : float.PositiveInfinity;
                float heightRatio = maxHeight.HasValue ? (float)maxHeight.Value / height : float.PositiveInfinity;

                // Pick the smaller ratio to ensure both constraints are respected
                float scale = Math.Min(widthRatio, heightRatio);

                int newWidth = (int)(width * scale);
                int newHeight = (int)(height * scale);

                image.Mutate(x => x.Resize(newWidth, newHeight));

                using var ms = new MemoryStream();
                var format = image.Metadata.DecodedImageFormat;

                if (format != null)
                    image.Save(ms, format);
                else
                    image.SaveAsPng(ms);

                return (ms.ToArray(), contentType);
            }


            return (fileBytes, contentType);
        }
    }
}
