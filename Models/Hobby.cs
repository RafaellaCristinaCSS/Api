using System.ComponentModel.DataAnnotations;

namespace ScholaAi.Models
{
    public class Hobby
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }    // varchar (50)
    }
}
