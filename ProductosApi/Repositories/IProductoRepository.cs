using ProductosApi.Models;

namespace ProductosApi.Repositories;

// Esta interfaz es como una lista de lo que se puede hacer con los productos.
// Solo dice QUÉ operaciones hay, no CÓMO se hacen (eso lo hace ProductoRepository).
// El controlador solo conoce esta lista. Así, si algún día cambio la base de datos,
// el controlador no se modifica.
public interface IProductoRepository
{
    // Trae todos los productos.
    List<Producto> ObtenerTodos();

    // Trae un producto por su id. Si no existe, regresa null (nada).
    Producto? ObtenerPorId(int id);

    // Guarda un producto nuevo y regresa el id que le puso la base de datos.
    int Insertar(Producto producto);

    // Cambia los datos de un producto. Regresa true si lo encontró, false si no existe.
    bool Actualizar(Producto producto);

    // Borra un producto. Regresa true si lo borró, false si no existe.
    bool Eliminar(int id);

    // Este lo agregué yo para el punto "Buscar" de la actividad.
    // Busca un texto en el nombre, la descripción o la categoría.
    List<Producto> Buscar(string termino);
}
