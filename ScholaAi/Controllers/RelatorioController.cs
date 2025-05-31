using Microsoft.AspNetCore.Mvc;
using ScholaAi.Dados;
using Microsoft.EntityFrameworkCore;

namespace ScholaAi.Controllers
{
    [ApiController]
    [Route("api/relatorio")]
    public class RelatorioController :ControllerBase
    {
        private readonly AppDbContext _context;

        public RelatorioController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("relatorio-desempenho/{idAluno}")]
        public async Task<IActionResult> RelatorioDesempenho(int idAluno)
        {
            var dados = await _context.AlunoAtividadeMateria
                .Include(a => a.Atividade)
                    .ThenInclude(at => at.Questoes)
                .Include(a => a.Materia)
                .Where(a => a.IdAluno == idAluno)
                .Where(a => a.Atividade.Publicada == true)
                .ToListAsync();

            var relatorio = dados
                .GroupBy(d => new { d.IdMateria,d.Materia.Nome})
                .Select(grupoMateria =>
                {
                    var relatorioMateria = new RelatorioMateriaDto
                    {
                        IdMateria = grupoMateria.Key.IdMateria,
                        NomeMateria = grupoMateria.Key.Nome,
                        Atividades = grupoMateria.Select(a =>
                        {
                            float pontuacaoTotalAtividade = a.Atividade.Questoes.Sum(q => q.Pontuacao);

                            return new RelatorioAtividadeDto
                            {
                                IdAtividade = a.IdAtividade,
                                NomeAtividade = a.Atividade.Nome,
                                PontuacaoPossivel = pontuacaoTotalAtividade,
                                PontuacaoObtida = a.Pontuacao ?? 0
                            };
                        }).ToList()
                    };

                    relatorioMateria.TotalPontuacaoPossivel = relatorioMateria.Atividades.Sum(a => a.PontuacaoPossivel);
                    relatorioMateria.TotalPontuacaoObtida = relatorioMateria.Atividades.Sum(a => a.PontuacaoObtida);

                    return relatorioMateria;
                })
                .ToList();

            return Ok(relatorio);
        }

        [HttpGet("relatorio-desempenho/educador/{idEducador}")]
        public async Task<IActionResult> RelatorioDesempenhoPorEducador(int idEducador)
        {
            var alunos = await _context.Aluno
                .Where(a => a.IdEducador == idEducador)
                .ToListAsync();

            var listaRelatorios = new List<object>();

            foreach(var aluno in alunos)
            {
                var dados = await _context.AlunoAtividadeMateria
                    .Include(a => a.Atividade)
                        .ThenInclude(at => at.Questoes)
                    .Include(a => a.Materia)
                    .Where(a => a.IdAluno == aluno.Id)
                    .Where(a => a.Atividade.Publicada == true)
                    .ToListAsync();

                var relatorio = dados
                    .GroupBy(d => new { d.IdMateria,d.Materia.Nome })
                    .Select(grupoMateria =>
                    {
                        var relatorioMateria = new RelatorioMateriaDto
                        {
                            IdMateria = grupoMateria.Key.IdMateria,
                            NomeMateria = grupoMateria.Key.Nome,
                            Atividades = grupoMateria.Select(a =>
                            {
                                float pontuacaoTotalAtividade = a.Atividade.Questoes.Sum(q => q.Pontuacao);

                                return new RelatorioAtividadeDto
                                {
                                    IdAtividade = a.IdAtividade,
                                    NomeAtividade = a.Atividade.Nome,
                                    PontuacaoPossivel = pontuacaoTotalAtividade,
                                    PontuacaoObtida = a.Pontuacao ?? 0
                                };
                            }).ToList()
                        };

                        relatorioMateria.TotalPontuacaoPossivel = relatorioMateria.Atividades.Sum(a => a.PontuacaoPossivel);
                        relatorioMateria.TotalPontuacaoObtida = relatorioMateria.Atividades.Sum(a => a.PontuacaoObtida);

                        return relatorioMateria;
                    })
                    .ToList();

                var nomeAluno = await _context.Agente
                 .Where(a => a.Id  == aluno.IdAgente)
                 .Select(a => a.Nome)
                 .FirstOrDefaultAsync();

                listaRelatorios.Add(new
                {
                    IdAluno = aluno.Id,
                    NomeAluno = nomeAluno,
                    Relatorio = relatorio
                });
            }

            return Ok(listaRelatorios);
        }


    }
    public class RelatorioMateriaDto
    {
        public int IdMateria { get; set; }
        public string NomeMateria { get; set; }
        public List<RelatorioAtividadeDto> Atividades { get; set; } = new();
        public float TotalPontuacaoPossivel { get; set; }
        public float TotalPontuacaoObtida { get; set; }
    }
    public class RelatorioAtividadeDto
    {
        public int IdAtividade { get; set; }
        public string NomeAtividade { get; set; }
        public float PontuacaoPossivel { get; set; }
        public float PontuacaoObtida { get; set; }
    }

}
