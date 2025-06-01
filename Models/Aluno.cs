using System;
using System.ComponentModel.DataAnnotations;

namespace ScholaAi.Models
{
    public class Aluno
    {
        [Key]
        public int Id { get; set; }
        public int IdEducador { get; set; }
        public int IdAgente { get; set; }
        public DateTime DataNascimento { get; set; }
        public string? EstiloAprendizagem { get; set; }
        public string? GeneroLiterarioFavorito { get; set; }
        public string? ModeloEnsino { get; set; }
        public string? HorasEstudo { get; set; }
        public string? Hobbies { get; set; }
        public string? InformacaoAdicional { get; set; }
    }
}
