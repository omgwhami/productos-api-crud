using Microsoft.AspNetCore.Mvc;
using ProductosApi.Models;
using ProductosApi.Repositories;

namespace ProductosApi.Controllers;

// El controlador es la "puerta de entrada" de la API. Recibe lo que pide el usuario
// (desde Swagger, por ejemplo), le pide el trabajo al repositorio y responde.
// No tiene nada de SQL, solo coordina.
// Todas las rutas empiezan con: /api/productos
[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    // Aquí uso la interfaz y no la clase directa. ASP.NET me da el repositorio
    // que se configuró en Program.cs.
    private readonly IProductoRepository _repositorio;

    public ProductosController(IProductoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    // GET /api/productos
    // Regresa todos los productos. Código 200 = todo bien.
    [HttpGet]
    public ActionResult<List<Producto>> ObtenerTodos()
    {
        return Ok(_repositorio.ObtenerTodos());
    }

    // GET /api/productos/5
    // Regresa un producto por su id. El ":int" obliga a que sea un número.
    [HttpGet("{id:int}")]
    public ActionResult<Producto> ObtenerPorId(int id)
    {
        var producto = _repositorio.ObtenerPorId(id);

        // Si no existe respondo 404 (no encontrado).
        if (producto is null)
        {
            return NotFound(new { mensaje = $"No existe un producto con id {id}." });
        }

        return Ok(producto);
    }

    // GET /api/productos/buscar?termino=mouse
    // Busca productos que contengan el texto escrito.
    [HttpGet("buscar")]
    public ActionResult<List<Producto>> Buscar([FromQuery] string termino)
    {
        // Si el texto viene vacío respondo 400 (petición incorrecta).
        if (string.IsNullOrWhiteSpace(termino))
        {
            return BadRequest(new { mensaje = "Escribe un texto para buscar." });
        }

        return Ok(_repositorio.Buscar(termino.Trim()));
    }

    // POST /api/productos
    // Agrega un producto nuevo.
    [HttpPost]
    public ActionResult<Producto> Crear([FromBody] Producto producto)
    {
        // Las reglas de la clase Producto ya se revisaron antes de llegar aquí.
        var nuevoId = _repositorio.Insertar(producto);
        var creado = _repositorio.ObtenerPorId(nuevoId);

        // Código 201 = creado. También avisa en qué ruta se puede consultar el nuevo producto.
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoId }, creado);
    }

    // PUT /api/productos/5
    // Edita un producto que ya existe.
    [HttpPut("{id:int}")]
    public IActionResult Actualizar(int id, [FromBody] Producto producto)
    {
        // Uso el id de la ruta, así no hay problema si en el cuerpo viene otro distinto.
        producto.Id = id;

        if (!_repositorio.Actualizar(producto))
        {
            return NotFound(new { mensaje = $"No existe un producto con id {id}." });
        }

        // Código 204 = se hizo bien y no hay nada más que mostrar.
        return NoContent();
    }

    // DELETE /api/productos/5
    // Elimina un producto.
    [HttpDelete("{id:int}")]
    public IActionResult Eliminar(int id)
    {
        if (!_repositorio.Eliminar(id))
        {
            return NotFound(new { mensaje = $"No existe un producto con id {id}." });
        }

        return NoContent();
    }
}
