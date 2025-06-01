using System.ComponentModel.DataAnnotations;

namespace ScholaAi.Models
{
    public class Questao
    {
        [Key]
        public int Id { get; set; }
        public string Texto { get; set; }

        public int AtividadeId { get; set; }
        public float Pontuacao { get; set; }
        public List<Alternativa>? Alternativas { get; set; }
    }
}
