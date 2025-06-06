using System.ComponentModel.DataAnnotations;

namespace ScholaAi.Models
{
    public class BlocoNotas
    {
        [Key]
        public int Id { get; set; }
        public string Anotacao { get; set; }
        public int IdAgente { get; set; }
    }
}
