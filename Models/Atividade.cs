using System.ComponentModel.DataAnnotations;

namespace ScholaAi.Models
{
    public class Atividade
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; }
        public int IdMateria { get; set; }
        public float Pontuacao { get; set; }
        public int IdProfessor { get; set; }
        public int IdTipoAtividade { get; set; }
        public bool Publicada { get; set; }

        public string? TextoLeitura { get; set; }
        public string? ArquivoBase64 { get; set; }
        public string? NomeArquivo { get; set; }

        public List<Questao> Questoes { get; set; }
    }

}
