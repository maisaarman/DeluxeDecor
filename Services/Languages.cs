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
    private static readonly Dictionary<string, Dictionary<string, string>> Text = new()
    {
        ["en"] = new()
        {
            ["heroTitle1"] = "Depth, cut",
            ["heroTitle2"] = "layer by layer.",
            ["heroSub"] = "Multi-layered laser-cut wood mirrors, wall decorations and tables — each piece built from stacked hardwoods.",
            ["all"] = "All",
            ["ask"] = "Ask about this piece",
            ["madeToOrder"] = "Every piece is made to order — sizes, colors and pricing are custom.",
            ["location"] = "Nablus, Palestine",
            ["noProducts"] = "No products yet.",
            ["back"] = "Back to catalog",
            ["notFound"] = "Product not found.",
            ["description"] = "About this piece",
            ["askMsg"] = "Hello, I'm interested in:",
            ["eyebrow"] = "Handcrafted in Nablus",
            ["inqSub"] = "Request a custom quote, size or design changes.",
            ["inqQuote"] = "Custom quote",
            ["inqSize"] = "Size",
            ["inqCustom"] = "Customization",
            ["inqName"] = "Name",
            ["inqEmail"] = "Email",
            ["inqMessage"] = "Message",
            ["inqSend"] = "Send question",
            ["inqOk"] = "Thank you! We received your message and will reply soon.",
            ["inqErr"] = "Something went wrong. Please try again.",
            ["inqWa"] = "Chat on WhatsApp",

        },
        ["ar"] = new()
        {
            ["heroTitle1"] = "عمق، يُقطع",
            ["heroTitle2"] = "طبقة تلو الأخرى.",
            ["heroSub"] = "مرايا وديكورات جدارية وطاولات من الخشب المقطّع بالليزر متعدد الطبقات — كل قطعة مبنية من طبقات خشب صلب متراصّة.",
            ["all"] = "الكل",
            ["ask"] = "اسأل عن هذه القطعة",
            ["madeToOrder"] = "كل قطعة تُصنع حسب الطلب — المقاسات والألوان والأسعار بحسب رغبتك.",
            ["location"] = "نابلس، فلسطين",
            ["noProducts"] = "لا توجد منتجات بعد.",
            ["back"] = "العودة إلى المعرض",
            ["notFound"] = "المنتج غير موجود.",
            ["description"] = "عن هذه القطعة",
            ["askMsg"] = "مرحبًا، أنا مهتم بـ:",
            ["eyebrow"] = "صناعة يدوية من نابلس",
            ["inqSub"] = "اطلب عرض سعر خاص، أو مقاسًا، أو تعديلات على التصميم.",
            ["inqQuote"] = "عرض سعر خاص",
            ["inqSize"] = "المقاس",
            ["inqCustom"] = "تخصيص",
            ["inqName"] = "الاسم",
            ["inqEmail"] = "البريد الإلكتروني",
            ["inqMessage"] = "رسالتك",
            ["inqSend"] = "إرسال السؤال",
            ["inqOk"] = "شكرًا لك! وصلتنا رسالتك وسنرد عليك قريبًا.",
            ["inqErr"] = "حدث خطأ. حاول مرة أخرى.",
            ["inqWa"] = "تواصل عبر واتساب",

        },
        ["fr"] = new()
        {
            ["heroTitle1"] = "De la profondeur, découpée",
            ["heroTitle2"] = "couche après couche.",
            ["heroSub"] = "Miroirs, décorations murales et tables en bois multicouche découpé au laser — chaque pièce est composée de bois durs superposés.",
            ["all"] = "Tout",
            ["ask"] = "Se renseigner sur cette pièce",
            ["madeToOrder"] = "Chaque pièce est réalisée sur commande — dimensions, couleurs et prix sur mesure.",
            ["location"] = "Naplouse, Palestine",
            ["noProducts"] = "Aucun produit pour le moment.",
            ["back"] = "Retour au catalogue",
            ["notFound"] = "Produit introuvable.",
            ["description"] = "À propos de cette pièce",
            ["askMsg"] = "Bonjour, je suis intéressé(e) par :",
            ["eyebrow"] = "Fait main à Naplouse",
            ["inqSub"] = "Demandez un devis sur mesure, une taille ou des modifications de design.",
            ["inqQuote"] = "Devis sur mesure",
            ["inqSize"] = "Taille",
            ["inqCustom"] = "Personnalisation",
            ["inqName"] = "Nom",
            ["inqEmail"] = "E-mail",
            ["inqMessage"] = "Message",
            ["inqSend"] = "Envoyer la question",
            ["inqOk"] = "Merci ! Nous avons bien reçu votre message et vous répondrons bientôt.",
            ["inqErr"] = "Une erreur est survenue. Veuillez réessayer.",
            ["inqWa"] = "Discuter sur WhatsApp",
        },
    };

    public static string Get(string lang, string key)
    {
        if (Text.TryGetValue(lang, out var d) && d.TryGetValue(key, out var v)) return v;
        return Text["en"].GetValueOrDefault(key, key);
    }
}