using System.ComponentModel.DataAnnotations;

namespace Examen2DSE.Models
{
    public class Permiso
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }
}
