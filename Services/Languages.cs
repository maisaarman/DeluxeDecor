using System.Text.Json;
namespace DeluxeDecor.Services;

public record LangInfo(string Code, string Name, string Dir);

public static class Languages
{
    public static readonly LangInfo[] All =
    {
        new("ar", "العربية", "rtl"),
        new("en", "English", "ltr"),
        new("fr", "Français", "ltr"),
    };

    public const string Default = "ar";

    public static bool IsSupported(string? code) => All.Any(l => l.Code == code);

    public static string Dir(string code) => All.FirstOrDefault(l => l.Code == code)?.Dir ?? "rtl";

  
    public static string FromPath(string? path)
    {
        var seg = (path ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return IsSupported(seg) ? seg! : Default;
    }
}
public static class Ui
{
    private static Dictionary<string, Dictionary<string, string>> _text = new();

    
    public static void Load(string folder)
    {
        var all = new Dictionary<string, Dictionary<string, string>>();
        foreach (var l in Languages.All)
        {
            var path = Path.Combine(folder, $"{l.Code}.json");
            if (!File.Exists(path))
                throw new FileNotFoundException($"Missing translation file: {path}");

            all[l.Code] = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path)) ?? new();
        }
        _text = all;
    }

    
    public static string Get(string lang, string key)
    {
        if (_text.TryGetValue(lang, out var d) && d.TryGetValue(key, out var v)) return v;
        if (_text.TryGetValue("en", out var en) && en.TryGetValue(key, out var e)) return e;
        return key;
    }
}