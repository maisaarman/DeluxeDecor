
namespace DeluxeDecor.Services;
public record ImageVm(int Id, string FileName);
public class TranslationEdit
{
    public string LanguageCode { get; set; } = "";
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class ProductEditModel
{
    public int Id { get; set; }
    public string? Slug { get; set; }
    public int CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public List<ImageVm> Images { get; set; } = new();
    public List<TranslationEdit> Translations { get; set; } =
        Languages.All.Select(l => new TranslationEdit { LanguageCode = l.Code }).ToList();
}