using System.ComponentModel.DataAnnotations;

namespace ScholaAi.Models
{
    public class Educador
    {
        [Key]
        public int Id { get; set; }
        public int IdAgente { get; set; }
        public int NivelVisibilidade { get; set; }
    }
}
