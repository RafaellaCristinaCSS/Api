using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScholaAi.Models
{
    public class AlunoAtividadeMateria
    {
        [Key]
        public int Id { get; set; }

        public int IdAluno { get; set; }
        public int IdMateria { get; set; }
        public int IdAtividade { get; set; }
        public float? Pontuacao { get; set; }
        public DateOnly? Data { get; set; }

        [ForeignKey(nameof(IdAluno))]
        public Aluno Aluno { get; set; }

        [ForeignKey(nameof(IdAtividade))]
        public Atividade Atividade { get; set; }

        [ForeignKey(nameof(IdMateria))]
        public Materia Materia { get; set; }
    }
}
