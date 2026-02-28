using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// 1. [CR�TICO] Habilitar HttpClient para las llamadas a CoinGecko y Github Models
builder.Services.AddHttpClient();

// 2. [VITAL] Memoria Cach� para el motor de la IA (Rate Limiting y Sesiones Aisladas)
// Esta l�nea es crucial para que el KairosController reconozca el IMemoryCache
builder.Services.AddMemoryCache();

// 3. [VITAL] Configuraci�n para Autenticaci�n (Preparando Google/Facebook)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // Aqu� es donde en el futuro podr�as agregar:
    // options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login"; // D�nde mandar al usuario si no ha iniciado sesi�n
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

// Configuraci�n del pipeline (Middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 5. [ORDEN CR�TICO] Sesiones y Seguridad
app.UseSession(); // Debe ir ANTES de la Autenticaci�n/Autorizaci�n en muchos casos, o justo aqu�.
app.UseAuthentication(); // Authentication: �Qui�n eres? 
app.UseAuthorization();  // Authorization: �Tienes permiso de estar aqu�?

// Configuraci�n de rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"); // Inicia en el Login por defecto

app.Run();