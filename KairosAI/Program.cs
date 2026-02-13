var builder = WebApplication.CreateBuilder(args);

// Agregamos servicios para MVC (Model-View-Controller)
// Esto es lo único necesario para que funcionen las vistas.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configuración del pipeline de solicitudes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // HSTS por defecto para producción
    app.UseHsts();
}

app.UseHttpsRedirection();

// Habilita el uso de archivos estáticos (CSS, JS, Imágenes en wwwroot)
app.UseStaticFiles();

app.UseRouting();

// Mantenemos la autorización básica para que MVC funcione correctamente
app.UseAuthorization();

// Configuración de rutas
// Aquí definimos que la app arranque en el Login del AccountController
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();