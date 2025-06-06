using System.ComponentModel.DataAnnotations;

namespace ScholaAi.Models
{
    public class VariavelGlobal
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Valor { get; set; }
    }
}
