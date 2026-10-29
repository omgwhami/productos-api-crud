using MySqlConnector;
using ProductosApi.Data;
using ProductosApi.Models;

namespace ProductosApi.Repositories;

// Esta clase hace el trabajo real con la base de datos. Aquí está todo el SQL.
// En cada método hago lo mismo:
//   1) Pido una conexión y la abro.
//   2) Preparo la consulta SQL. Los datos van como @parametros (por ejemplo @id)
//      y no pegados al texto, para que nadie pueda meter código malicioso.
//   3) La ejecuto y regreso el resultado.
// El "using" cierra la conexión solo cuando termina el método.
public class ProductoRepository : IProductoRepository
{
    private readonly ConexionDb _conexionDb;

    // Las columnas que pido en los SELECT. Las guardo aquí para no escribirlas
    // varias veces, y siempre en el mismo orden.
    private const string Columnas =
        "id, nombre, descripcion, precio, stock, categoria, fecha_creacion";

    // ASP.NET me da la ConexionDb cuando crea esta clase (se configura en Program.cs).
    public ProductoRepository(ConexionDb conexionDb)
    {
        _conexionDb = conexionDb;
    }

    // ---------------------------------------------------------------
    // PUNTO 2: CONSULTAS
    // ---------------------------------------------------------------

    // Trae todos los productos de la tabla.
    public List<Producto> ObtenerTodos()
    {
        var productos = new List<Producto>();

        using var conexion = _conexionDb.CrearConexion();
        conexion.Open();

        const string sql = "SELECT " + Columnas + " FROM productos ORDER BY id;";
        using var comando = new MySqlCommand(sql, conexion);

        // El lector va leyendo los resultados fila por fila.
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            // Cada fila la convierto en un Producto y la agrego a la lista.
            productos.Add(Mapear(lector));
        }

        return productos;
    }

    // Busca un solo producto usando su id.
    public Producto? ObtenerPorId(int id)
    {
        using var conexion = _conexionDb.CrearConexion();
        conexion.Open();

        const string sql = "SELECT " + Columnas + " FROM productos WHERE id = @id;";
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@id", id);

        using var lector = comando.ExecuteReader();

        // Si Read() da true es que encontró el producto; si no, regreso null.
        return lector.Read() ? Mapear(lector) : null;
    }

    // ---------------------------------------------------------------
    // PUNTO 3: INSERT (agregar)
    // ---------------------------------------------------------------

    // Guarda un producto nuevo.
    public int Insertar(Producto producto)
    {
        using var conexion = _conexionDb.CrearConexion();
        conexion.Open();

        // No mando el id ni la fecha porque MySQL los crea solo.
        const string sql =
            "INSERT INTO productos (nombre, descripcion, precio, stock, categoria) " +
            "VALUES (@nombre, @descripcion, @precio, @stock, @categoria);";

        using var comando = new MySqlCommand(sql, conexion);
        AgregarParametros(comando, producto);

        comando.ExecuteNonQuery();

        // Regreso el id que MySQL le puso al producto nuevo.
        return (int)comando.LastInsertedId;
    }

    // ---------------------------------------------------------------
    // PUNTO 4: ACTUALIZAR (editar)
    // ---------------------------------------------------------------

    // Cambia los datos de un producto que ya existe.
    public bool Actualizar(Producto producto)
    {
        using var conexion = _conexionDb.CrearConexion();
        conexion.Open();

        // El WHERE es importante: sin él se cambiarían TODOS los productos.
        const string sql =
            "UPDATE productos SET nombre = @nombre, descripcion = @descripcion, " +
            "precio = @precio, stock = @stock, categoria = @categoria " +
            "WHERE id = @id;";

        using var comando = new MySqlCommand(sql, conexion);
        AgregarParametros(comando, producto);
        comando.Parameters.AddWithValue("@id", producto.Id);

        // ExecuteNonQuery dice cuántas filas se afectaron.
        // Si es más de 0, sí se encontró el producto; si es 0, el id no existe.
        return comando.ExecuteNonQuery() > 0;
    }

    // ---------------------------------------------------------------
    // PUNTO 5: ELIMINAR
    // ---------------------------------------------------------------

    // Borra un producto por su id.
    public bool Eliminar(int id)
    {
        using var conexion = _conexionDb.CrearConexion();
        conexion.Open();

        const string sql = "DELETE FROM productos WHERE id = @id;";
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@id", id);

        // Igual que en Actualizar: true si borró algo, false si no existía.
        return comando.ExecuteNonQuery() > 0;
    }

    // ---------------------------------------------------------------
    // PUNTO 6: BUSCAR
    // ---------------------------------------------------------------

    // Busca productos que contengan el texto en el nombre, la descripción o la categoría.
    public List<Producto> Buscar(string termino)
    {
        var productos = new List<Producto>();

        using var conexion = _conexionDb.CrearConexion();
        conexion.Open();

        // LIKE sirve para buscar "que contenga" un texto. El signo % significa
        // "cualquier cosa", así que %mouse% encuentra todo lo que tenga "mouse".
        const string sql =
            "SELECT " + Columnas + " FROM productos " +
            "WHERE nombre LIKE @termino OR descripcion LIKE @termino OR categoria LIKE @termino " +
            "ORDER BY nombre;";

        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@termino", $"%{termino}%");

        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            productos.Add(Mapear(lector));
        }

        return productos;
    }

    // ---------------------------------------------------------------
    // Métodos de ayuda (solo se usan dentro de esta clase)
    // ---------------------------------------------------------------

    // Toma una fila que leyó de la base de datos y la convierte en un objeto Producto.
    private static Producto Mapear(MySqlDataReader lector)
    {
        return new Producto
        {
            Id = lector.GetInt32("id"),
            Nombre = lector.GetString("nombre"),
            // La descripción puede venir vacía (NULL), por eso primero reviso.
            Descripcion = lector.IsDBNull(lector.GetOrdinal("descripcion"))
                ? null
                : lector.GetString("descripcion"),
            Precio = lector.GetDecimal("precio"),
            Stock = lector.GetInt32("stock"),
            Categoria = lector.GetString("categoria"),
            FechaCreacion = lector.GetDateTime("fecha_creacion")
        };
    }

    // Hace lo contrario de Mapear: pasa los datos del Producto a los @parametros
    // de la consulta. Lo usan Insertar y Actualizar para no repetir código.
    private static void AgregarParametros(MySqlCommand comando, Producto producto)
    {
        comando.Parameters.AddWithValue("@nombre", producto.Nombre);
        // Si la descripción está vacía, se manda DBNull.Value, que es el "vacío" de SQL.
        comando.Parameters.AddWithValue("@descripcion", (object?)producto.Descripcion ?? DBNull.Value);
        comando.Parameters.AddWithValue("@precio", producto.Precio);
        comando.Parameters.AddWithValue("@stock", producto.Stock);
        comando.Parameters.AddWithValue("@categoria", producto.Categoria);
    }
}
