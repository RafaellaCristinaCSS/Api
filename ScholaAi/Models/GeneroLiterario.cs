using System.ComponentModel.DataAnnotations;

namespace ScholaAi.Models
{
    public class GeneroLiterario
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }    // varchar (50)
        public string Categoria { get; set; }    // Varchar(50)
    }
}
