namespace ScholaAi.Models
{
    public class Config
    {
        public int Id { get; protected set; }

        protected string Chave { get; set; }

        protected Config() { } 

        public static Config Criar(string chave)
        {
            return new Config { Chave = chave };
        }

        public string ObterToken() => Chave;
    }
}
