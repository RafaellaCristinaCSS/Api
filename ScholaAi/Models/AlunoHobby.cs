using System.ComponentModel.DataAnnotations;

namespace ScholaAi.Models
{
    public class AlunoHobby
    {
        [Key]
        public int Id { get; set; }
        public int IdAluno { get; set; }
        public int IdHobby { get; set; }
    }
}
