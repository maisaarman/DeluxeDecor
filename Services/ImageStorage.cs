using Microsoft.AspNetCore.Components.Forms;

namespace DeluxeDecor.Services;

public class ImageStorage
{
    public static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    public static readonly string[] VideoExtensions = { ".mp4", ".webm" };
    public static readonly string[] AllowedExtensions = ImageExtensions.Concat(VideoExtensions).ToArray();

    public const long MaxImageBytes = 8 * 1024 * 1024;    // 8MB
    public const long MaxVideoBytes = 50 * 1024 * 1024;   // 50MB
    public const long MaxBytes = MaxImageBytes;

    public string Root { get; }

    public ImageStorage(IConfiguration config, IWebHostEnvironment env)
    {
        var configured = config["Uploads:Path"];
        Root = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), "uploads")
            : Path.GetFullPath(configured, env.ContentRootPath);
        Directory.CreateDirectory(Root);
    }

    public static bool IsVideo(string fileName) =>
        VideoExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());

    public static long LimitFor(string fileName) => IsVideo(fileName) ? MaxVideoBytes : MaxImageBytes;

    public async Task<string> SaveAsync(IBrowserFile file)
    {
        var ext = Path.GetExtension(file.Name).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"نوع الملف غير مدعوم: {file.Name}");

        var name = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(Root, name);
        try
        {
            await using var src = file.OpenReadStream(LimitFor(file.Name));
            await using var dst = File.Create(path);
            await src.CopyToAsync(dst);
        }
        catch
        {
            if (File.Exists(path)) File.Delete(path);
            throw;
        }
        return name;
    }

    public void Delete(string fileName)
    {
        var path = Path.Combine(Root, Path.GetFileName(fileName));
        if (File.Exists(path)) File.Delete(path);
    }
}