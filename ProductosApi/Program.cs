using Microsoft.AspNetCore.Diagnostics;
using ProductosApi.Data;
using ProductosApi.Repositories;

// Este archivo es donde arranca todo el programa.
var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------
// 1) SERVICIOS
//    Aquí le digo a ASP.NET qué clases va a necesitar y cómo crearlas.
// ---------------------------------------------------------------

// Activa los controladores (los endpoints de la API).
builder.Services.AddControllers();

// ConexionDb se crea una sola vez y se comparte (solo guarda el texto de conexión).
builder.Services.AddSingleton<ConexionDb>();

// Cuando alguien pida un IProductoRepository, ASP.NET le da un ProductoRepository.
// Así el controlador no tiene que crearlo con "new". Se crea uno nuevo por cada petición.
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();

// Swagger: crea la página web para probar la API.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ---------------------------------------------------------------
// 2) CONFIGURACIÓN DE LA APP
//    Cada petición pasa por estos pasos en orden, de arriba hacia abajo.
// ---------------------------------------------------------------

// Si algo falla (por ejemplo, MySQL apagado) respondo un mensaje claro con error 500
// y dejo el detalle del error en la consola.
app.UseExceptionHandler(errorApp => errorApp.Run(async contexto =>
{
    var error = contexto.Features.Get<IExceptionHandlerFeature>()?.Error;
    var logger = contexto.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogError(error, "Error no controlado en la API");

    contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await contexto.Response.WriteAsJsonAsync(new
    {
        mensaje = "Error en el servidor. Revisa que MySQL este encendido y que la cadena de conexion sea correcta."
    });
}));

// Activa Swagger para poder probar la API desde el navegador.
app.UseSwagger();
app.UseSwaggerUI();

// Si entro a la página principal "/", me manda directo a Swagger.
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// Conecta las rutas de los controladores.
app.MapControllers();

app.Run();
