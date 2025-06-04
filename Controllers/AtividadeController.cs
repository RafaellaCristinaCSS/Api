using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ScholaAi.Dados;
using ScholaAi.Models;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace ScholaAi.Controllers
{
    [ApiController]
    [Route("api/atividade")]
    public class AtividadeController :ControllerBase
    {
        private readonly AppDbContext _context;

        public AtividadeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("buscarTiposDeAtividades")]
        public async Task<IActionResult> BuscarTiposDeAtividades()
        {
            var tiposDeAtividades = await _context.TipoAtividade
                                    .Select(ta => new { ta.Id,ta.Nome })
                                    .ToListAsync();
            return Ok(tiposDeAtividades);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarAtividade(int id,[FromBody] AtividadeDTO atividadeDto)
        {
            var atividadeExistente = await _context.Atividade
                .Include(a => a.Questoes)
                    .ThenInclude(q => q.Alternativas)
                .FirstOrDefaultAsync(a => a.Id == id);

            if(atividadeExistente == null)
                return NotFound("Atividade não encontrada.");

            var idProfessor = await _context.Educador
                .Where(e => e.IdAgente == atividadeDto.IdAgente)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();

            if(idProfessor == 0)
                return BadRequest("Professor não encontrado para o agente informado.");

            atividadeExistente.Nome = atividadeDto.Nome;
            atividadeExistente.IdMateria = atividadeDto.IdMateria;
            atividadeExistente.IdProfessor = idProfessor;
            atividadeExistente.IdTipoAtividade = atividadeDto.IdTipoAtividade;
            atividadeExistente.Pontuacao = atividadeDto.Pontuacao;
            atividadeExistente.Publicada = atividadeDto.Publicada;

            atividadeExistente.TextoLeitura = atividadeDto.TextoLeitura;
            atividadeExistente.ArquivoBase64 = atividadeDto.ArquivoBase64;
            atividadeExistente.NomeArquivo = atividadeDto.NomeArquivo;

            if(atividadeDto.Questoes != null && atividadeDto.Questoes.Any())
            {
                foreach(var questao in atividadeExistente.Questoes)
                {
                    _context.Alternativa.RemoveRange(questao.Alternativas);
                }
                _context.Questao.RemoveRange(atividadeExistente.Questoes);
                atividadeExistente.Questoes.Clear();

                foreach(var questaoDto in atividadeDto.Questoes)
                {
                    var novaQuestao = new Questao
                    {
                        Texto = questaoDto.Texto,
                        Pontuacao = questaoDto.Pontuacao,
                        Alternativas = questaoDto.Alternativas.Select(a => new Alternativa
                        {
                            Texto = a.Texto,
                            Correta = a.Correta
                        }).ToList()
                    };

                    atividadeExistente.Questoes.Add(novaQuestao);
                }
            }

            var relacoesAntigas = _context.AlunoAtividadeMateria.Where(r => r.IdAtividade == id);
            _context.AlunoAtividadeMateria.RemoveRange(relacoesAntigas);

            foreach(var idAluno in atividadeDto.ListaIdAlunos)
            {
                var novaRelacao = new AlunoAtividadeMateria
                {
                    IdAluno = idAluno,
                    IdAtividade = id,
                    IdMateria = atividadeDto.IdMateria
                };
                _context.AlunoAtividadeMateria.Add(novaRelacao);
            }

            await _context.SaveChangesAsync();
            return Ok("Atividade atualizada com sucesso.");
        }


        //[Authorize]

        [HttpPost]
        public async Task<IActionResult> AdicionarAtividade([FromBody] AtividadeDTO atividadeDto)
        {
            var idProfessor = await _context.Educador
                .Where(e => e.IdAgente == atividadeDto.IdAgente)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();

            if(idProfessor == 0)
                return BadRequest("Professor não encontrado para o agente informado. " + atividadeDto.IdAgente);

            var atividade = new Atividade
            {
                Nome = atividadeDto.Nome,
                IdMateria = atividadeDto.IdMateria,
                IdProfessor = idProfessor,
                IdTipoAtividade = atividadeDto.IdTipoAtividade,
                Pontuacao = atividadeDto.Pontuacao,
                Publicada = atividadeDto.Publicada
            };

            if(atividadeDto.IdTipoAtividade == 1)
            {
                atividade.Questoes = atividadeDto.Questoes.Select(q => new Questao
                {
                    Texto = q.Texto,
                    Pontuacao = q.Pontuacao,
                    Alternativas = q.Alternativas.Select(a => new Alternativa
                    {
                        Texto = a.Texto,
                        Correta = a.Correta
                    }).ToList()
                }).ToList();
            }
            else if(atividadeDto.IdTipoAtividade == 2 || atividadeDto.IdTipoAtividade == 3)
            {
                atividade.TextoLeitura = atividadeDto.TextoLeitura;
                atividade.ArquivoBase64 = atividadeDto.ArquivoBase64;
                atividade.NomeArquivo = atividadeDto.NomeArquivo;
            }

            _context.Atividade.Add(atividade);
            await _context.SaveChangesAsync();

            foreach(var idAluno in atividadeDto.ListaIdAlunos)
            {
                var relacao = new AlunoAtividadeMateria
                {
                    IdAluno = idAluno,
                    IdAtividade = atividade.Id,
                    IdMateria = atividadeDto.IdMateria
                };

                _context.AlunoAtividadeMateria.Add(relacao);
            }

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(AdicionarAtividade),new { id = atividade.Id },atividade);
        }

        [HttpGet("buscarAtividade/{idAtividade}")]
        public async Task<IActionResult> GetAtividade(int idAtividade)
        {
            var materias = await _context.Materia
                .ToDictionaryAsync(m => m.Id,m => m.Nome);

            var atividade = await _context.Atividade
                .Where(a => a.Id == idAtividade)
                .Include(a => a.Questoes)
                    .ThenInclude(q => q.Alternativas)
                .FirstOrDefaultAsync();

            if(atividade == null)
                return NotFound("Atividade não encontrada");

            var alunosAtividadeMateria = await _context.AlunoAtividadeMateria
                .Where(at => at.IdAtividade == idAtividade)
                .Select(at => new { at.Aluno.Id })
                .ToListAsync();

            var resultado = new
            {
                atividade.Id,
                atividade.Nome,
                atividade.IdMateria,
                atividade.IdTipoAtividade,
                atividade.Publicada,
                atividade.Pontuacao,
                atividade.TextoLeitura,     
                atividade.ArquivoBase64,    
                atividade.NomeArquivo,      
                Alunos = alunosAtividadeMateria,
                Materia = materias.ContainsKey(atividade.IdMateria) ? materias[atividade.IdMateria] : "Desconhecida",
                Questoes = atividade.Questoes.Select(q => new
                {
                    q.Id,
                    q.Texto,
                    q.Pontuacao,
                    Alternativas = q.Alternativas.Select(alt => new
                    {
                        alt.Id,
                        alt.Texto,
                        alt.Correta
                    })
                })
            };

            return Ok(new { TipoAgente = "Aluno",Atividade = resultado });
        }


        [HttpGet("atividadesPorAgente/{idAgente}")]
        public async Task<IActionResult> GetAtividadesPorAgente(int idAgente)
        {
            var idEducador = await _context.Educador
                .Where(e => e.IdAgente == idAgente)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();

            if(idEducador != 0)
            {
                var atividadesEducador = await _context.Atividade
                    .Where(a => a.IdProfessor == idEducador)
                    .Include(a => a.Questoes)
                        .ThenInclude(q => q.Alternativas)
                    .ToListAsync();

                var materias = await _context.Materia.ToListAsync();

                var resultado = atividadesEducador.Select(a =>
                {
                    var materia = materias.FirstOrDefault(m => m.Id == a.IdMateria);

                    return new
                    {
                        a.Id,
                        a.Nome,
                        a.Pontuacao,
                        InformacaoExtra = (a.Publicada ? "Publicada" : "Rascunho"),
                        Materia = materia != null ? new
                        {
                            Id = materia.Id,
                            Nome = materia.Nome,
                            Imagem = materia.Imagem
                        } : new
                        {
                            Id = 0,
                            Nome = "Desconhecida",
                            Imagem = (string?)null
                        },
                        Questoes = a.Questoes.Select(q => new
                        {
                            q.Id,
                            q.Texto,
                            Alternativas = q.Alternativas.Select(alt => new
                            {
                                alt.Id,
                                alt.Texto,
                                alt.Correta
                            })
                        })
                    };
                }).ToList();

                        return Ok(new { TipoAgente = "Educador",Atividades = resultado });
            }

                    var idAluno = await _context.Aluno
             .Where(a => a.IdAgente == idAgente)
             .Select(a => a.Id)
             .FirstOrDefaultAsync();

            if(idAluno != 0)
            {
                var registros = await _context.AlunoAtividadeMateria
                    .Where(aam => aam.IdAluno == idAluno && aam.Atividade.Publicada == true)
                    .Include(aam => aam.Atividade)
                        .ThenInclude(a => a.Questoes)
                            .ThenInclude(q => q.Alternativas)
                    .Include(aam => aam.Materia) 
                    .ToListAsync();

                var resultado = registros.Select(r => new
                {
                    r.Atividade.Id,
                    r.Atividade.Nome,
                    r.Pontuacao,
                    InformacaoExtra = r.Pontuacao.HasValue ? "Nota:" + r.Pontuacao.Value.ToString("0.##") : "Pendente",
                    Materia = new
                    {
                        Id = r.Materia?.Id,
                        Nome = r.Materia.Nome,
                        Imagem = r.Materia?.Imagem
                    },
                    Questoes = r.Atividade.Questoes.Select(q => new
                    {
                        q.Id,
                        q.Texto,
                        Alternativas = q.Alternativas.Select(alt => new
                        {
                            alt.Id,
                            alt.Texto,
                            alt.Correta
                        })
                    })
                }).ToList();

                return Ok(new { TipoAgente = "Aluno",Atividades = resultado });
            }

            return NotFound("Agente não encontrado.");
        }

        [HttpPost("responderAtividade")]
        public async Task<IActionResult> ResponderAtividade([FromBody] RespostaAlunoDto respostaAlunoDto)
        {
            var idAluno = await _context.Aluno
                             .Where(a => a.IdAgente == respostaAlunoDto.AgenteId)
                             .Select(a => a.Id)
                             .FirstOrDefaultAsync();

            if(idAluno == 0)
                return BadRequest("Aluno não encontrado para o agente informado.");

            float pontuacaoTotal = 0;

            foreach(var resposta in respostaAlunoDto.Respostas)
            {
                var alternativa = await _context.Alternativa
                    .FirstOrDefaultAsync(a => a.Id == resposta.AlternativaId && a.QuestaoId == resposta.QuestaoId);

                if(alternativa == null) continue;

                if(alternativa.Correta)
                {
                    var questao = await _context.Questao.FirstOrDefaultAsync(q => q.Id == resposta.QuestaoId);
                    if(questao != null)
                    {
                        pontuacaoTotal += questao.Pontuacao;
                    }
                }
            }

            var alunoAtividade = await _context.AlunoAtividadeMateria
                .FirstOrDefaultAsync(a =>
                    a.IdAluno == idAluno &&
                    a.IdAtividade == respostaAlunoDto.AtividadeId &&
                    a.IdMateria == respostaAlunoDto.MateriaId);

            if(alunoAtividade == null)
            {
                alunoAtividade = new AlunoAtividadeMateria
                {
                    IdAluno = idAluno,
                    IdAtividade = respostaAlunoDto.AtividadeId,
                    IdMateria = respostaAlunoDto.MateriaId,
                    Pontuacao = pontuacaoTotal
                };
                _context.AlunoAtividadeMateria.Add(alunoAtividade);
            }
            else
            {
                alunoAtividade.Pontuacao = pontuacaoTotal;
                _context.AlunoAtividadeMateria.Update(alunoAtividade);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mensagem = "Respostas registradas com sucesso.",
                PontuacaoFinal = pontuacaoTotal
            });
        }
        [HttpPost("gerarQuestionarioAutomatico")]
        public async Task<IActionResult> CriarQuestionarioAutomatico(
    int idAluno,
    int idMateria,
    string nomeMateria,
    string temaAtividade,
    int idAgente,
    string nomeQuestionario,
    string urlApiInterna,
    string openAiKey)
        {
            var aluno = await _context.Aluno.FirstOrDefaultAsync(a => a.Id == idAluno);
            if(aluno == null)
                return NotFound("Aluno não encontrado.");

            var materiais = await _context.Material
                .Where(m => m.IdMateria == idMateria && !m.Excluido)
                .Select(m => m.Conteudo)
                .ToListAsync();

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",openAiKey);

            var prompt = $@"
Crie 3 questões de múltipla escolha com 4 alternativas cada, sendo uma correta.
Matéria: {nomeMateria}
Tema: {temaAtividade}
Aluno: nascido em {aluno.DataNascimento:dd/MM/yyyy}, gosta de {aluno.Hobbies}, gênero literário favorito: {aluno.GeneroLiterarioFavorito}, modelo de ensino: {aluno.ModeloEnsino}, info adicional: {aluno.InformacaoAdicional}
Base de leitura: {string.Join(" ",materiais)}

Formato JSON:
[
  {{
    'texto': 'string',
    'pontuacao': 0,
    'alternativas': [
      {{ 'texto': 'string', 'correta': true }},
      {{ 'texto': 'string', 'correta': false }},
      {{ 'texto': 'string', 'correta': false }},
      {{ 'texto': 'string', 'correta': false }}
    ]
  }}
]
";

            var body = new
            {
                model = "gpt-4o",
                temperature = 0.7,
                messages = new[]
                {
            new { role = "system", content = "Você é um gerador de questionários escolares." },
            new { role = "user", content = prompt }
        }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(body),Encoding.UTF8,"application/json");
            var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions",jsonContent);
            var responseString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseString);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .ToString();

            content = content.Replace("'","\"");

            List<Questao> questoes;
            try
            {
                questoes = JsonSerializer.Deserialize<List<Questao>>(content,new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch(JsonException ex)
            {
                return BadRequest($"Erro ao interpretar as questões retornadas pela IA: {ex.Message}\nConteúdo:\n{content}");
            }

            var questionario = new
            {
                nome = nomeQuestionario,
                idMateria = idMateria,
                idAgente = idAgente,
                idTipoAtividade = 4,
                pontuacao = 0,
                publicada = false,
                listaIdAlunos = new[] { aluno.Id },
                questoes = questoes,
                textoLeitura = string.Join("\n",materiais),
                arquivoBase64 = "",
                nomeArquivo = ""
            };

            var jsonString = JsonSerializer.Serialize(questionario,new JsonSerializerOptions { WriteIndented = true });
            return Ok(jsonString);
        }

    }

    public class TipoAtividadeDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
    }
    public class AtividadeDTO
    {
        public string Nome { get; set; }
        public int IdMateria { get; set; }
        public int IdAgente { get; set; }
        public int IdTipoAtividade { get; set; }
        public int Pontuacao { get; set; }
        public bool Publicada { get; set; }
        public List<int> ListaIdAlunos { get; set; } = new();
        public List<QuestaoDTO> Questoes { get; set; } = new();

        public string? TextoLeitura { get; set; }     
        public string? ArquivoBase64 { get; set; }    
        public string? NomeArquivo { get; set; }      
    }


    public class QuestaoDTO
    {
        public string Texto { get; set; }
        public float Pontuacao { get; set; }
        public List<AlternativaDTO> Alternativas { get; set; }
    }

    public class AlternativaDTO
    {
        public string Texto { get; set; }
        public bool Correta { get; set; }
    }
    public class RespostaAlunoDto
    {
        public int AtividadeId { get; set; }
        public int AgenteId { get; set; }  
        public int MateriaId { get; set; }
        public List<RespostaQuestaoDto> Respostas { get; set; }
    }

    public class RespostaQuestaoDto
    {
        public int QuestaoId { get; set; }
        public int AlternativaId { get; set; }
    }


}
