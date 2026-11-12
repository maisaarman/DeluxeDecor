using DeluxeDecor.Components;
using DeluxeDecor.Data;
using Microsoft.EntityFrameworkCore;
using DeluxeDecor.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

var dataDir = builder.Configuration["DataDir"] is { Length: > 0 } configuredDir
    ? Path.GetFullPath(configuredDir, builder.Environment.ContentRootPath)
    : Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDir);
builder.Services.AddDbContextFactory<AppDbContext>(o =>
    o.UseSqlite($"Data Source={Path.Combine(dataDir, "shop.db")}"));
builder.Services.AddScoped<CatalogService>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

    builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/admin/login";
        o.Cookie.Name = "dd_admin";
        o.Cookie.HttpOnly = true;
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<ImageStorage>();
builder.Services.AddSingleton<SiteInfo>();
var app = builder.Build();
Ui.Load(Path.Combine(app.Environment.ContentRootPath, "Localization"));
var adminPassword = app.Configuration["Admin:Password"];
if (string.IsNullOrWhiteSpace(adminPassword) ||
    (!app.Environment.IsDevelopment() && adminPassword.Length < 10))
{
    throw new InvalidOperationException(
        "Set Admin:Password (10+ characters in production) using user-secrets or the Admin__Password environment variable.");
}
await DbSeeder.InitializeAsync(app.Services);
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
var storage = app.Services.GetRequiredService<ImageStorage>();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(storage.Root),
    RequestPath = "/uploads"
});
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapGet("/", () => Results.Redirect("/ar"));
app.MapPost("/admin/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync();
    return Results.Redirect("/admin/login");
});

app.MapPost("/api/inquiry", async (InquiryDto dto, CatalogService catalog) =>
{
    if (!string.IsNullOrEmpty(dto.Website)) return Results.Ok();   // حقل مخفي: البوتات بس بتعبّيه
    try
    {
        await catalog.AddInquiryAsync(dto);
        return Results.Ok();
    }
    catch (ArgumentException)
    {
        return Results.BadRequest();
    }
});
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
