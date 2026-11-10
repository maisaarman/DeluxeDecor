
namespace DeluxeDecor.Services;

public class CategoryEditModel
{
    public int Id { get; set; }
    public string? Slug { get; set; }
    public int SortOrder { get; set; }
    public int ProductCount { get; set; }

    public List<TranslationEdit> Translations { get; set; } =
        Languages.All.Select(l => new TranslationEdit { LanguageCode = l.Code }).ToList();

   
    public CategoryEditModel Clone() => new()
    {
        Id = Id,
        Slug = Slug,
        SortOrder = SortOrder,
        ProductCount = ProductCount,
        Translations = Translations
            .Select(t => new TranslationEdit { LanguageCode = t.LanguageCode, Name = t.Name })
            .ToList()
    };
}