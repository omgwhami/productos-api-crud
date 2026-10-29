using MySqlConnector;

namespace ProductosApi.Data;

// PUNTO 1: CONEXIÓN
// Esta clase se encarga de conectarse a la base de datos MySQL.
// Los datos de la conexión (servidor, usuario, contraseña) no están aquí escritos,
// los lee del archivo appsettings.json. Así, si cambio de base de datos,
// solo cambio ese archivo y no el código.
public class ConexionDb
{
    // Aquí guardo el texto con los datos para conectarme.
    private readonly string _cadenaConexion;

    // Cuando se crea la clase, ASP.NET le da la configuración (appsettings.json)
    // y de ahí saco el texto de conexión que se llama "MySql".
    public ConexionDb(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("MySql")
            ?? throw new InvalidOperationException(
                "Falta la cadena de conexion 'MySql' en appsettings.json.");
    }

    // Crea una conexión nueva a MySQL. Todavía está cerrada:
    // quien la use tiene que abrirla con Open().
    // En el repositorio la uso con "using" para que se cierre sola al terminar.
    public MySqlConnection CrearConexion()
    {
        return new MySqlConnection(_cadenaConexion);
    }
}
