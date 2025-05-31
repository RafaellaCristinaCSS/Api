using System.ComponentModel.DataAnnotations;

namespace ScholaAi.Models
{
    public class TipoAtividade
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; }    // varchar (50)
    }
}
