using ScholaAi.Models;
using System.ComponentModel.DataAnnotations;

namespace ScholaAi
{
    public class Alternativa
    {

        [Key]
        public int Id { get; set; }
        public string Texto { get; set; }
        public bool Correta { get; set; }
        public int QuestaoId { get; set; }
    }
}
