using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace ScholaAi.Models
{
    public class Agente
    {
        [Key]
        public int Id { get; set; }
        public string Senha { get; set; } //Varchar(20)
        public string Login { get; set; } //Varchar(30)
        public string Nome { get; set; } //Varchar(255)
        public int IdTipoAgente { get; set; }
        //public string Lembrete { get; set; }

        public string DefinirSenha(string senha)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(senha);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
