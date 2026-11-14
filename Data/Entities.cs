namespace DeluxeDecor.Data;

public class Category
{
    public int Id { get; set; }
    public string Slug { get; set; } = "";
    public int SortOrder { get; set; }

    public List<CategoryTranslation> Translations { get; set; } = new();
    public List<Product> Products { get; set; } = new();
}

public class CategoryTranslation
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public string LanguageCode { get; set; } = "";   // ar / en / fr
    public string Name { get; set; } = "";
}

public class Product
{
    public int Id { get; set; }
    public string Slug { get; set; } = "";
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ProductTranslation> Translations { get; set; } = new();
    public List<ProductImage> Images { get; set; } = new();
    public string? ThumbnailFileName { get; set; }
}

public class ProductTranslation
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public string LanguageCode { get; set; } = "";   // ar / en / fr
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public string FileName { get; set; } = "";   
    public int SortOrder { get; set; }           
}

public class Inquiry
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string Kind { get; set; } = "quote";   // quote / size / custom
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
}