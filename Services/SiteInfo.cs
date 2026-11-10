
namespace DeluxeDecor.Services;

public class SiteInfo
{
    public string Name { get; }
    public string WhatsApp { get; }

    public SiteInfo(IConfiguration config)
    {
        Name = config["Site:Name"] ?? "Deluxe Decor";
        WhatsApp = new string((config["Contact:WhatsApp"] ?? "").Where(char.IsDigit).ToArray());
    }

    
    public string AskLink(string lang, string productName, string fallbackUrl)
    {
        if (WhatsApp.Length == 0) return fallbackUrl;
        var msg = $"{Ui.Get(lang, "askMsg")} {productName}";
        return $"https://wa.me/{WhatsApp}?text={Uri.EscapeDataString(msg)}";
    }
}