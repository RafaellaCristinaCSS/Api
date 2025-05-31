using System.ComponentModel.DataAnnotations;

namespace ScholaAi.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }
        public string Conteudo { get; set; }
        public int IdEducadorCriador { get; set; }
        public int IdMateria { get; set; }
        public bool Excluido { get; set; }

    }
}
