using System.ComponentModel.DataAnnotations;

namespace ScholaAi.Models
{
    public class Materia
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; }  
        public string Imagem {  get; set; }
    }
}
