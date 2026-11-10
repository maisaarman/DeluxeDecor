using Microsoft.EntityFrameworkCore;

namespace DeluxeDecor.Data;

public static class DbSeeder
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await factory.CreateDbContextAsync();

        await db.Database.EnsureCreatedAsync();  
        await db.Database.ExecuteSqlRawAsync(@"
      CREATE TABLE IF NOT EXISTS ""Inquiries"" (
        ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Inquiries"" PRIMARY KEY AUTOINCREMENT,
        ""ProductId"" INTEGER NULL,
        ""ProductName"" TEXT NOT NULL,
        ""Kind"" TEXT NOT NULL,
        ""Name"" TEXT NOT NULL,
        ""Email"" TEXT NOT NULL,
        ""Message"" TEXT NOT NULL,
        ""CreatedAt"" TEXT NOT NULL,
        ""IsRead"" INTEGER NOT NULL
);");
       
        if (await db.Categories.AnyAsync()) return;  // إذا في بيانات، لا نعيد التعبئة

        var mirrors = Cat("mirrors", 1, "مرايا", "Mirrors", "Miroirs");
        var wall = Cat("wall-decor", 2, "ديكور جداري", "Wall Decor", "Décoration murale");
        var tables = Cat("tables", 3, "طاولات", "Tables", "Tables");
        db.Categories.AddRange(mirrors, wall, tables);

        db.Products.AddRange(
            Prod("emerald-capsule-mirror", 1, mirrors,
                "مرآة الكبسولة الزمردية", "Emerald Capsule Mirror", "Miroir capsule émeraude",
                "مرآة بشكل كبسولة بإطار من طبقات خشب صلب مقطوعة بالليزر بدرجات الأخضر الزمردي.",
                "A capsule-shaped mirror framed in stacked layers of laser-cut hardwood with emerald-green tones.",
                "Un miroir en forme de capsule, encadré de couches de bois dur découpées au laser dans des tons émeraude."),
            Prod("half-mandala-round-mirror", 2, mirrors,
                "مرآة دائرية نصف ماندالا", "Half Mandala Round Mirror", "Miroir rond demi-mandala",
                "مرآة دائرية بنقش نصف ماندالا مقطوع في عدة طبقات من الخشب.",
                "A round mirror with a half-mandala pattern cut into multiple layers of wood.",
                "Un miroir rond au motif de demi-mandala découpé dans plusieurs couches de bois."),
            Prod("guitar-mandala-wall-art", 3, wall,
                "ديكور جداري ماندالا الغيتار", "Guitar Mandala Wall Art", "Décoration murale mandala guitare",
                "لوحة جدارية تجمع بين شكل الغيتار وماندالا متعددة الطبقات من الخشب المتراص.",
                "Wall art combining a guitar silhouette with a layered mandala, cut from stacked wood.",
                "Une décoration murale alliant une silhouette de guitare et un mandala en couches de bois superposées."));

        await db.SaveChangesAsync();
    }

    private static Category Cat(string slug, int order, string ar, string en, string fr) => new()
    {
        Slug = slug,
        SortOrder = order,
        Translations =
        {
            new CategoryTranslation { LanguageCode = "ar", Name = ar },
            new CategoryTranslation { LanguageCode = "en", Name = en },
            new CategoryTranslation { LanguageCode = "fr", Name = fr },
        }
    };

    private static Product Prod(string slug, int order, Category cat,
        string nameAr, string nameEn, string nameFr,
        string descAr, string descEn, string descFr) => new()
    {
        Slug = slug,
        SortOrder = order,
        Category = cat,
        Translations =
        {
            new ProductTranslation { LanguageCode = "ar", Name = nameAr, Description = descAr },
            new ProductTranslation { LanguageCode = "en", Name = nameEn, Description = descEn },
            new ProductTranslation { LanguageCode = "fr", Name = nameFr, Description = descFr },
        }
    };
}