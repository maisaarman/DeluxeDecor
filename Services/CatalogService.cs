using DeluxeDecor.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Forms;
namespace DeluxeDecor.Services;

public record CategoryVm(int Id, string Slug, string Name);
public record ProductCardVm(int Id, string Slug, string Name, string CategoryName, string? Cover);
public record ProductDetailVm(int Id, string Slug, string Name, string? Description,
 string CategoryName, string CategorySlug, List<string> Images);
public record ProductRowVm(int Id, string Name, string CategoryName, string? Cover, bool IsActive);
    

public class CatalogService
{
   private readonly IDbContextFactory<AppDbContext> _factory;
private readonly ImageStorage _images;

public CatalogService(IDbContextFactory<AppDbContext> factory, ImageStorage images)
{
    _factory = factory;
    _images = images;
}
    public async Task<List<CategoryVm>> GetCategoriesAsync(string lang)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var cats = await db.Categories.AsNoTracking()
            .Include(c => c.Translations)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        return cats.Select(c => new CategoryVm(
            c.Id, c.Slug,
            Pick(c.Translations.Select(t => (t.LanguageCode, t.Name)), lang) ?? c.Slug)).ToList();
    }

    public async Task<List<ProductCardVm>> GetProductsAsync(string lang, string? categorySlug = null)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var q = db.Products.AsNoTracking().Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(categorySlug))
            q = q.Where(p => p.Category!.Slug == categorySlug);

        var products = await q
            .Include(p => p.Translations)
            .Include(p => p.Images)
            .Include(p => p.Category).ThenInclude(c => c!.Translations)
            .OrderBy(p => p.SortOrder)
            .ToListAsync();

        return products.Select(p => new ProductCardVm(
            p.Id,
            p.Slug,
            Pick(p.Translations.Select(t => (t.LanguageCode, t.Name)), lang) ?? p.Slug,
            p.Category is null
                ? ""
                : Pick(p.Category.Translations.Select(t => (t.LanguageCode, t.Name)), lang) ?? "",
            p.Images.OrderBy(i => i.SortOrder).Select(i => i.FileName).FirstOrDefault()
        )).ToList();
    }
    public async Task<ProductDetailVm?> GetProductAsync(string lang, string slug)
{
    await using var db = await _factory.CreateDbContextAsync();

    var p = await db.Products.AsNoTracking()
        .Include(x => x.Translations)
        .Include(x => x.Images)
        .Include(x => x.Category).ThenInclude(c => c!.Translations)
        .AsSplitQuery()
        .FirstOrDefaultAsync(x => x.Slug == slug && x.IsActive);

    if (p is null) return null;

    
    var named = p.Translations.Where(t => !string.IsNullOrWhiteSpace(t.Name)).ToList();
    var tr = named.FirstOrDefault(t => t.LanguageCode == lang)
          ?? named.FirstOrDefault(t => t.LanguageCode == "en")
          ?? named.FirstOrDefault();

    return new ProductDetailVm(
        p.Id,
        p.Slug,
        tr?.Name ?? p.Slug,
        tr?.Description,
        p.Category is null
            ? ""
            : Pick(p.Category.Translations.Select(t => (t.LanguageCode, t.Name)), lang) ?? "",
        p.Category?.Slug ?? "",
        p.Images.OrderBy(i => i.SortOrder).Select(i => i.FileName).ToList());
}

    private static string? Pick(IEnumerable<(string Lang, string Name)> items, string lang)
    {
        var list = items.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToList();
        return list.FirstOrDefault(x => x.Lang == lang).Name
            ?? list.FirstOrDefault(x => x.Lang == "en").Name
            ?? list.FirstOrDefault().Name;
    }

private static string Slugify(string s)
{
    var sb = new System.Text.StringBuilder();
    foreach (var ch in s.Trim().ToLowerInvariant())
    {
        if (ch is (>= 'a' and <= 'z') or (>= '0' and <= '9')) sb.Append(ch);
        else if (sb.Length > 0 && sb[^1] != '-') sb.Append('-');
    }
    var r = sb.ToString().Trim('-');
    return r.Length == 0 ? "item-" + Guid.NewGuid().ToString("N")[..6] : r;
}

    public async Task<List<ProductRowVm>> AdminListProductsAsync()
{
    await using var db = await _factory.CreateDbContextAsync();

    var items = await db.Products.AsNoTracking()
        .Include(p => p.Translations)
        .Include(p => p.Images)
        .Include(p => p.Category).ThenInclude(c => c!.Translations)
        .AsSplitQuery()
        .OrderBy(p => p.SortOrder).ThenByDescending(p => p.Id)
        .ToListAsync();

    return items.Select(p => new ProductRowVm(
        p.Id,
        Pick(p.Translations.Select(t => (t.LanguageCode, t.Name)), "ar") ?? p.Slug,
        p.Category is null
            ? ""
            : Pick(p.Category.Translations.Select(t => (t.LanguageCode, t.Name)), "ar") ?? "",
        p.Images.OrderBy(i => i.SortOrder).Select(i => i.FileName).FirstOrDefault(),
        p.IsActive)).ToList();
}

public async Task SetProductActiveAsync(int id, bool active)
{
    await using var db = await _factory.CreateDbContextAsync();
    var p = await db.Products.FindAsync(id);
    if (p is null) return;
    p.IsActive = active;
    await db.SaveChangesAsync();
}

public async Task DeleteProductAsync(int id)
{
    await using var db = await _factory.CreateDbContextAsync();
    var p = await db.Products.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id);
    if (p is null) return;

    var files = p.Images.Select(i => i.FileName).ToList();
    db.Products.Remove(p);
    await db.SaveChangesAsync();
    foreach (var f in files) _images.Delete(f);
}

public async Task DeleteImageAsync(int imageId)
{
    await using var db = await _factory.CreateDbContextAsync();
    var img = await db.ProductImages.FindAsync(imageId);
    if (img is null) return;

    var file = img.FileName;
    db.ProductImages.Remove(img);
    await db.SaveChangesAsync();
    _images.Delete(file);
}

public async Task SetCoverAsync(int productId, int imageId)
{
    await using var db = await _factory.CreateDbContextAsync();
    var imgs = await db.ProductImages
        .Where(i => i.ProductId == productId)
        .OrderBy(i => i.SortOrder)
        .ToListAsync();

    var cover = imgs.FirstOrDefault(i => i.Id == imageId);
    if (cover is null) return;

    imgs.Remove(cover);
    imgs.Insert(0, cover);
    for (var i = 0; i < imgs.Count; i++) imgs[i].SortOrder = i;
    await db.SaveChangesAsync();
}
public async Task<ProductEditModel?> GetProductEditModelAsync(int id)
{
    await using var db = await _factory.CreateDbContextAsync();
    var p = await db.Products.AsNoTracking()
        .Include(x => x.Translations)
        .Include(x => x.Images)
        .AsSplitQuery()
        .FirstOrDefaultAsync(x => x.Id == id);
    if (p is null) return null;

    var m = new ProductEditModel
    {
        Id = p.Id,
        Slug = p.Slug,
        CategoryId = p.CategoryId,
        IsActive = p.IsActive,
        SortOrder = p.SortOrder,
        Images = p.Images.OrderBy(i => i.SortOrder).Select(i => new ImageVm(i.Id, i.FileName)).ToList()
    };

    foreach (var t in m.Translations)
    {
        var e = p.Translations.FirstOrDefault(x => x.LanguageCode == t.LanguageCode);
        if (e is null) continue;
        t.Name = e.Name;
        t.Description = e.Description;
    }
    return m;
}

public async Task<int> SaveProductAsync(ProductEditModel m, IReadOnlyCollection<IBrowserFile> files)
{
    var named = m.Translations.Where(t => !string.IsNullOrWhiteSpace(t.Name)).ToList();
    if (named.Count == 0) throw new InvalidOperationException("أدخل اسم المنتج بلغة واحدة على الأقل.");
    if (m.CategoryId == 0) throw new InvalidOperationException("اختر فئة للمنتج.");

    await using var db = await _factory.CreateDbContextAsync();

    Product p;
    if (m.Id == 0)
    {
        p = new Product { CreatedAt = DateTime.UtcNow };
        db.Products.Add(p);
    }
    else
    {
        p = await db.Products
            .Include(x => x.Translations)
            .Include(x => x.Images)
            .SingleAsync(x => x.Id == m.Id);
    }

    p.CategoryId = m.CategoryId;
    p.IsActive = m.IsActive;
    p.SortOrder = m.SortOrder;

    foreach (var t in m.Translations)
    {
        var name = t.Name?.Trim();
        var desc = string.IsNullOrWhiteSpace(t.Description) ? null : t.Description.Trim();
        var existing = p.Translations.FirstOrDefault(x => x.LanguageCode == t.LanguageCode);

        if (string.IsNullOrEmpty(name))
        {
            if (existing is not null) p.Translations.Remove(existing);
            continue;
        }

        if (existing is null)
            p.Translations.Add(new ProductTranslation { LanguageCode = t.LanguageCode, Name = name, Description = desc });
        else
        {
            existing.Name = name;
            existing.Description = desc;
        }
    }

    var baseName = (named.FirstOrDefault(t => t.LanguageCode == "en") ?? named[0]).Name!;
    var wanted = !string.IsNullOrWhiteSpace(m.Slug) ? Slugify(m.Slug)
        : !string.IsNullOrEmpty(p.Slug) ? p.Slug
        : Slugify(baseName);

    var slug = wanted;
    for (var n = 2; await db.Products.AnyAsync(x => x.Slug == slug && x.Id != p.Id); n++)
        slug = $"{wanted}-{n}";
    p.Slug = slug;

    
    var nextOrder = p.Images.Count == 0 ? 0 : p.Images.Max(i => i.SortOrder) + 1;
    var savedFiles = new List<string>();
    try
    {
        foreach (var f in files)
        {
            var fileName = await _images.SaveAsync(f);
            savedFiles.Add(fileName);
            p.Images.Add(new ProductImage { FileName = fileName, SortOrder = nextOrder++ });
        }
        await db.SaveChangesAsync();
    }
    catch
    {
        foreach (var fn in savedFiles) _images.Delete(fn);
        throw;
    }

    return p.Id;
}
public async Task<List<CategoryEditModel>> GetCategoriesForAdminAsync()
{
    await using var db = await _factory.CreateDbContextAsync();

    var cats = await db.Categories.AsNoTracking()
        .Include(c => c.Translations)
        .OrderBy(c => c.SortOrder).ThenBy(c => c.Id)
        .ToListAsync();

    var counts = await db.Products
        .GroupBy(p => p.CategoryId)
        .Select(g => new { g.Key, Count = g.Count() })
        .ToDictionaryAsync(x => x.Key, x => x.Count);

    return cats.Select(c =>
    {
        var m = new CategoryEditModel
        {
            Id = c.Id,
            Slug = c.Slug,
            SortOrder = c.SortOrder,
            ProductCount = counts.GetValueOrDefault(c.Id)
        };
        foreach (var t in m.Translations)
            t.Name = c.Translations.FirstOrDefault(x => x.LanguageCode == t.LanguageCode)?.Name;
        return m;
    }).ToList();
}

public async Task SaveCategoryAsync(CategoryEditModel m)
{
    var named = m.Translations.Where(t => !string.IsNullOrWhiteSpace(t.Name)).ToList();
    if (named.Count == 0) throw new InvalidOperationException("أدخل اسم الفئة بلغة واحدة على الأقل.");

    await using var db = await _factory.CreateDbContextAsync();

    Category c;
    if (m.Id == 0)
    {
        c = new Category();
        db.Categories.Add(c);
    }
    else
    {
        c = await db.Categories.Include(x => x.Translations).SingleAsync(x => x.Id == m.Id);
    }

    c.SortOrder = m.SortOrder;

    foreach (var t in m.Translations)
    {
        var name = t.Name?.Trim();
        var existing = c.Translations.FirstOrDefault(x => x.LanguageCode == t.LanguageCode);

        if (string.IsNullOrEmpty(name))
        {
            if (existing is not null) c.Translations.Remove(existing);
            continue;
        }

        if (existing is null)
            c.Translations.Add(new CategoryTranslation { LanguageCode = t.LanguageCode, Name = name });
        else
            existing.Name = name;
    }

    var baseName = (named.FirstOrDefault(t => t.LanguageCode == "en") ?? named[0]).Name!;
    var wanted = !string.IsNullOrWhiteSpace(m.Slug) ? Slugify(m.Slug)
        : !string.IsNullOrEmpty(c.Slug) ? c.Slug
        : Slugify(baseName);

    var slug = wanted;
    for (var n = 2; await db.Categories.AnyAsync(x => x.Slug == slug && x.Id != c.Id); n++)
        slug = $"{wanted}-{n}";
    c.Slug = slug;

    await db.SaveChangesAsync();
}


public async Task<string?> DeleteCategoryAsync(int id)
{
    await using var db = await _factory.CreateDbContextAsync();

    if (await db.Products.AnyAsync(p => p.CategoryId == id))
        return "ما بتنحذف فئة فيها منتجات. انقلي المنتجات لفئة ثانية أو احذفيها أولًا.";

    var c = await db.Categories.FindAsync(id);
    if (c is null) return null;

    db.Categories.Remove(c);
    await db.SaveChangesAsync();
    return null;
}
public async Task AddInquiryAsync(InquiryDto d)
{
    var name = d.Name?.Trim() ?? "";
    var email = d.Email?.Trim() ?? "";
    var message = d.Message?.Trim() ?? "";

    if (name.Length is 0 or > 100 || email.Length is 0 or > 200 || !email.Contains('@')
        || message.Length is 0 or > 2000)
        throw new ArgumentException("invalid inquiry");

    var kind = d.Kind is "size" or "custom" ? d.Kind : "quote";

    await using var db = await _factory.CreateDbContextAsync();

    int? productId = null;
    var productName = "";
    if (d.ProductId is int pid)
    {
        var p = await db.Products.AsNoTracking()
            .Include(x => x.Translations)
            .FirstOrDefaultAsync(x => x.Id == pid);
        if (p is not null)
        {
            productId = p.Id;
            productName = Pick(p.Translations.Select(t => (t.LanguageCode, t.Name)), "ar") ?? p.Slug;
        }
    }

    db.Inquiries.Add(new Inquiry
    {
        ProductId = productId,
        ProductName = productName,
        Kind = kind!,
        Name = name,
        Email = email,
        Message = message
    });
    await db.SaveChangesAsync();
}

public async Task<List<Inquiry>> AdminListInquiriesAsync()
{
    await using var db = await _factory.CreateDbContextAsync();
    return await db.Inquiries.AsNoTracking().OrderByDescending(i => i.Id).ToListAsync();
}

public async Task SetInquiryReadAsync(int id, bool read)
{
    await using var db = await _factory.CreateDbContextAsync();
    var i = await db.Inquiries.FindAsync(id);
    if (i is null) return;
    i.IsRead = read;
    await db.SaveChangesAsync();
}

public async Task DeleteInquiryAsync(int id)
{
    await using var db = await _factory.CreateDbContextAsync();
    var i = await db.Inquiries.FindAsync(id);
    if (i is null) return;
    db.Inquiries.Remove(i);
    await db.SaveChangesAsync();
}
}