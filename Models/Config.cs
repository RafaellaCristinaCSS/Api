using System.ComponentModel.DataAnnotations;

namespace ScholaAi.Models
{
    public class Config
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; private set; }  
        public string Chave { get; private set; }

    }
}
