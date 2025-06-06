using Microsoft.EntityFrameworkCore;
using ScholaAi.Models;
namespace ScholaAi.Dados
{
    public class AppDbContext :DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Agente> Agente { get; set; }
        public DbSet<Aluno> Aluno { get; set; }
        public DbSet<AlunoAtividadeMateria> AlunoAtividadeMateria { get; set; }
        public DbSet<AlunoGeneroLiterario> AlunoGeneroLiterario { get; set; }
        public DbSet<AlunoMateria> AlunoMateria { get; set; }
        public DbSet<AlunoHobby> AlunoHobby { get; set; }
        public DbSet<Atividade> Atividade { get; set; }
        public DbSet<Educador> Educador { get; set; }
        public DbSet<GeneroLiterario> GeneroLiterario { get; set; }
        public DbSet<BlocoNotas> BlocoNotas { get; set; }
        public DbSet<Hobby> Hobby { get; set; }
        public DbSet<Materia> Materia { get; set; }
        public DbSet<Material> Material { get; set; }
        public DbSet<MaterialGeneroLiterario> MaterialGeneroLiterario { get; set; }
        public DbSet<Questao> Questao { get; set; }
        public DbSet<Alternativa> Alternativa { get; set; }  
        public DbSet<TipoAtividade> TipoAtividade { get; set; }
    }


}
