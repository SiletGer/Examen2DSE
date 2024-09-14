using System.ComponentModel.DataAnnotations;

namespace Examen2DSE.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Nombre { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Contraseña { get; set; }

        [Required]
        public int RolId { get; set; }
        public Rol Rol { get; set; }
    }
}
