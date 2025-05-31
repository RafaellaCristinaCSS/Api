using System.ComponentModel.DataAnnotations;

namespace ScholaAi.Models
{
    public class AlunoMateria
    {
        [Key]
        public int Id { get; set; }
        public int IdAluno { get; set; }
        public int IdMateria { get; set; }
    }
}
