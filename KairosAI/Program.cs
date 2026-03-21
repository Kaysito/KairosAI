using KairosAI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// 1. [CRÍTICO] Habilitar HttpClient para las llamadas a CoinGecko y Github Models
builder.Services.AddHttpClient();

// 2. [VITAL] Memoria Caché para el motor de la IA (Rate Limiting y Sesiones Aisladas)
builder.Services.AddMemoryCache();

// ═══ HTTP Clients con Typed Clients ═══
builder.Services.AddHttpClient<ICryptoService, CryptoService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
});

builder.Services.AddHttpClient<IStockService, StockService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
});

// ═══ News Aggregator ═══
builder.Services.AddScoped<INewsAggregatorService, NewsAggregatorService>();

// 3. [VITAL] Configuración para Autenticación (Preparando Google/Facebook)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// 4. [EXTRA] Habilitar Sesiones (Session) para el aislamiento del historial de Chat
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Agregamos servicios para MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configuración del pipeline (Middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 5. [ORDEN CRÍTICO] Sesiones y Seguridad
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Configuración de rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
