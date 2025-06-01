using System.ComponentModel.DataAnnotations;

namespace ScholaAi.Models
{
    public class MaterialGeneroLiterario
    {
        [Key]
        public int Id { get; set; }
        public int IdMateria { get; set; }
        public int IdGeneroLiterario { get; set; }
    }
}
