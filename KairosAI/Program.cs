using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// 1. [CRÍTICO] Habilitar HttpClient para las llamadas a CoinGecko y Github Models
builder.Services.AddHttpClient();

// 2. [VITAL] Memoria Caché para el motor de la IA (Rate Limiting y Sesiones Aisladas)
// Esta línea es crucial para que el KairosController reconozca el IMemoryCache
builder.Services.AddMemoryCache();

// 3. [VITAL] Configuración para Autenticación (Preparando Google/Facebook)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // Aquí es donde en el futuro podrías agregar:
    // options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Dónde mandar al usuario si no ha iniciado sesión
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
app.UseSession(); // Debe ir ANTES de la Autenticación/Autorización en muchos casos, o justo aquí.
app.UseAuthentication(); // Authentication: ¿Quién eres? 
app.UseAuthorization();  // Authorization: ¿Tienes permiso de estar aquí?

// Configuración de rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"); // Inicia en el Login por defecto

app.Run();