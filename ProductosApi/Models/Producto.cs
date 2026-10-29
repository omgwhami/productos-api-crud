using System.ComponentModel.DataAnnotations;

namespace ProductosApi.Models;

// Esta clase representa un producto. Cada dato de aquí (nombre, precio, etc.)
// es una columna de la tabla "productos" en MySQL.
// Las reglas entre corchetes, como [Required] o [Range], revisan que los datos
// estén bien antes de guardarlos. Si algo no cumple, la API responde con error 400.
public class Producto
{
    // El número lo crea MySQL solo, yo no lo mando al agregar un producto.
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede pasar de 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    // El signo ? quiere decir que la descripción se puede dejar vacía.
    [StringLength(255, ErrorMessage = "La descripcion no puede pasar de 255 caracteres.")]
    public string? Descripcion { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "La categoria es obligatoria.")]
    [StringLength(50, ErrorMessage = "La categoria no puede pasar de 50 caracteres.")]
    public string Categoria { get; set; } = string.Empty;

    // La fecha la pone MySQL sola cuando se guarda el producto.
    // Solo sirve para mostrarla; si la mando al agregar o editar, se ignora.
    public DateTime FechaCreacion { get; set; }
}
