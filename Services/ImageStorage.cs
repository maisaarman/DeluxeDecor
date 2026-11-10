
using Microsoft.AspNetCore.Components.Forms;

namespace DeluxeDecor.Services;

public class ImageStorage
{
    public static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    public const long MaxBytes = 8 * 1024 * 1024; // 8MB

    public string Root { get; }

  public ImageStorage(IConfiguration config, IWebHostEnvironment env)
{
    var configured = config["Uploads:Path"];
    Root = string.IsNullOrWhiteSpace(configured)
        ? Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), "uploads")
        : Path.GetFullPath(configured, env.ContentRootPath);
    Directory.CreateDirectory(Root);
}

    // يرجّع اسم الملف الجديد (GUID + الامتداد)
    public async Task<string> SaveAsync(IBrowserFile file)
    {
        var ext = Path.GetExtension(file.Name).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"نوع الملف غير مدعوم: {file.Name}");

        var name = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(Root, name);
        try
        {
            await using var src = file.OpenReadStream(MaxBytes);
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