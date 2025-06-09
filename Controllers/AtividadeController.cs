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
    public class AtividadeController : ControllerBase
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
                                    .Select(ta => new { ta.Id, ta.Nome })
                                    .ToListAsync();
            return Ok(tiposDeAtividades);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarAtividade(int id, [FromBody] AtividadeDTO atividadeDto)
        {
            var atividadeExistente = await _context.Atividade
                .Include(a => a.Questoes)
                    .ThenInclude(q => q.Alternativas)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (atividadeExistente == null)
                return NotFound("Atividade não encontrada.");

            var idProfessor = await _context.Educador
                .Where(e => e.IdAgente == atividadeDto.IdAgente)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();

            if (idProfessor == 0)
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

            if (atividadeDto.Questoes != null && atividadeDto.Questoes.Any())
            {
                foreach (var questao in atividadeExistente.Questoes)
                {
                    _context.Alternativa.RemoveRange(questao.Alternativas);
                }
                _context.Questao.RemoveRange(atividadeExistente.Questoes);
                atividadeExistente.Questoes.Clear();

                foreach (var questaoDto in atividadeDto.Questoes)
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

            foreach (var idAluno in atividadeDto.ListaIdAlunos)
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
        [HttpPost]
        public async Task<IActionResult> AdicionarAtividade([FromBody] AtividadeDTO atividadeDto)
        {
            var idProfessor = await _context.Educador
                .Where(e => e.IdAgente == atividadeDto.IdAgente)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();

            if (idProfessor == 0)
                return BadRequest("Professor não encontrado para o agente informado. " + atividadeDto.IdAgente);

            Atividade atividade;

            try
            {
                atividade = await MontarAtividade(atividadeDto, idProfessor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro ao montar atividade: " + ex.Message);
            }

            await SalvarAtividadeNoBanco(atividade, atividadeDto);
            return CreatedAtAction(nameof(AdicionarAtividade), new { id = atividade.Id }, atividade);
        }
        private async Task<Atividade> MontarAtividade(AtividadeDTO dto, int idProfessor)
        {
            var atividade = new Atividade
            {
                Nome = dto.Nome,
                IdMateria = dto.IdMateria,
                IdProfessor = idProfessor,
                IdTipoAtividade = dto.IdTipoAtividade,
                Pontuacao = dto.Pontuacao,
                Publicada = dto.Publicada
            };

            switch (dto.IdTipoAtividade)
            {
                case 1:
                    if (dto.Questoes?.Any() == true)
                    {
                        atividade.Questoes = dto.Questoes.Select(q => new Questao
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
                    break;

                case 2:
                case 3:
                    atividade.TextoLeitura = dto.TextoLeitura;
                    atividade.ArquivoBase64 = dto.ArquivoBase64;
                    atividade.NomeArquivo = dto.NomeArquivo;
                    break;
            }

            return atividade;
        }
        private async Task SalvarAtividadeNoBanco(Atividade atividade, AtividadeDTO dto)
        {
            _context.Atividade.Add(atividade);
            await _context.SaveChangesAsync();

            foreach (var idAluno in dto.ListaIdAlunos)
            {
                var relacao = new AlunoAtividadeMateria
                {
                    IdAluno = idAluno,
                    IdAtividade = atividade.Id,
                    IdMateria = dto.IdMateria,
                    Pontuacao = (dto.IdTipoAtividade == 2 || dto.IdTipoAtividade == 3)
                        ? dto.Pontuacao : 0,
                    Data = (dto.IdTipoAtividade == 2 || dto.IdTipoAtividade == 3)
                        ? DateOnly.FromDateTime(DateTime.Now) : null
                };

                _context.AlunoAtividadeMateria.Add(relacao);
            }

            await _context.SaveChangesAsync();
        }

        [HttpPost("gerarQuestoesAutomaticas")]
        public async Task<IActionResult> GerarQuestoesAutomatico(AtividadeDTO dto)
        {
            try
            {
                var questoesGeradas = await GerarQuestoesAutomaticamente(dto);

                if (questoesGeradas == null)
                    throw new Exception("Erro ao gerar questões automaticamente.");

                return questoesGeradas;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar questões automaticamente: " + ex.Message);
            }
        }


        [HttpGet("buscarAtividade/{idAtividade}")]
        public async Task<IActionResult> GetAtividade(int idAtividade)
        {
            var materias = await _context.Materia
                .ToDictionaryAsync(m => m.Id, m => m.Nome);

            var atividade = await _context.Atividade
                .Where(a => a.Id == idAtividade)
                .Include(a => a.Questoes)
                    .ThenInclude(q => q.Alternativas)
                .FirstOrDefaultAsync();

            if (atividade == null)
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

            return Ok(new { TipoAgente = "Aluno", Atividade = resultado });
        }


        [HttpGet("atividadesPorAgente/{idAgente}")]
        public async Task<IActionResult> GetAtividadesPorAgente(int idAgente)
        {
            var idEducador = await _context.Educador
                .Where(e => e.IdAgente == idAgente)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();

            if (idEducador != 0)
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

                return Ok(new { TipoAgente = "Educador", Atividades = resultado });
            }

            var idAluno = await _context.Aluno
     .Where(a => a.IdAgente == idAgente)
     .Select(a => a.Id)
     .FirstOrDefaultAsync();

            if (idAluno != 0)
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

                return Ok(new { TipoAgente = "Aluno", Atividades = resultado });
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

            if (idAluno == 0)
                return BadRequest("Aluno não encontrado para o agente informado.");

            float pontuacaoTotal = 0;

            foreach (var resposta in respostaAlunoDto.Respostas)
            {
                var alternativa = await _context.Alternativa
                    .FirstOrDefaultAsync(a => a.Id == resposta.AlternativaId && a.QuestaoId == resposta.QuestaoId);

                if (alternativa == null) continue;

                if (alternativa.Correta)
                {
                    var questao = await _context.Questao.FirstOrDefaultAsync(q => q.Id == resposta.QuestaoId);
                    if (questao != null)
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

            if (alunoAtividade == null)
            {
                alunoAtividade = new AlunoAtividadeMateria
                {
                    IdAluno = idAluno,
                    IdAtividade = respostaAlunoDto.AtividadeId,
                    IdMateria = respostaAlunoDto.MateriaId,
                    Pontuacao = pontuacaoTotal,
                    Data = DateOnly.FromDateTime(DateTime.Now)
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
        private async Task<IActionResult> GerarQuestoesAutomaticamente(AtividadeDTO dto)
        {
            try
            {
                bool existeMaterial = true;
                var aluno = await _context.Aluno.FirstOrDefaultAsync(a => a.Id == dto.ListaIdAlunos.First());
                if (aluno == null)
                {
                    return NotFound(new
                    {
                        mensagem = "Não foi possivel encontrar o aluno informado",
                        idMateria = dto.IdMateria
                    });
                }

                var materiais = await _context.Material
                    .Where(m => m.IdMateria == dto.IdMateria && !m.Excluido)
                    .Select(m => m.Conteudo)
                    .ToListAsync();

                if (materiais == null || !materiais.Any()) existeMaterial = false;

                var prompt = $@"
                    Crie 3 questões de múltipla escolha com 4 alternativas cada, sendo uma correta.
                    Matéria: {dto.NomeMateria}
                    Tema: {dto.TemaAtividade}
                    Aluno: nascido em {aluno.DataNascimento}, tem como gênero literário favorito {aluno.GeneroLiterarioFavorito} e {aluno.InformacaoAdicional}
                    {(existeMaterial ? "Base de leitura: " + string.Join(" ",materiais) : "Não há material disponível. Use seus conhecimentos sobre o tema para criar as questões.")}

                    Formato JSON:
                    [
                      {{
                        'texto': 'string',
                        'pontuacao': 0,
                        'alternativas': [
                          {{ 'texto': 'string', 'correta': true/false }},
                          ...
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

                using var httpClient = new HttpClient();
                string OpenIa = _context.Configuracoes
                    .FirstOrDefault(c => c.Nome == "OpenIa")?.Chave;

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", OpenIa);

                var jsonContent = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                try
                {
                    response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", jsonContent);
                }
                catch (Exception ex)
                {
                    throw new Exception("Erro na requisição HTTP para a OpenAI: " + ex.Message);
                }

                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Erro da API OpenAI: " + response.StatusCode + " - " + responseString);

                string content;
                try
                {
                    using var doc = JsonDocument.Parse(responseString);
                    content = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .ToString();
                }
                catch (Exception ex)
                {
                    throw new Exception("Erro ao extrair o conteúdo da resposta JSON da OpenAI: " + ex.Message + "\nResposta bruta:\n" + responseString);
                }

                try
                {
                    content = content.Trim();

                    if (content.StartsWith("```"))
                    {
                        var firstLineEnd = content.IndexOf('\n');
                        if (firstLineEnd != -1)
                        {
                            content = content.Substring(firstLineEnd + 1);
                        }

                        if (content.EndsWith("```"))
                        {
                            content = content.Substring(0, content.Length - 3);
                        }

                        content = content.Trim();
                    }

                    var questoes = JsonSerializer.Deserialize<List<Questao>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return Ok(questoes) ?? throw new Exception("A lista de questões retornada está vazia.");
                }
                catch (Exception ex)
                {
                    throw new Exception("Erro ao interpretar as questões retornadas pela IA: " + ex.Message + "\nConteúdo retornado:\n" + content);
                }
            }
            catch (Exception geral)
            {
                throw new Exception("Erro ao gerar questões automaticamente: " + geral.Message, geral);
            }
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
        public List<QuestaoDTO>? Questoes { get; set; } = new();

        public string? TextoLeitura { get; set; }
        public string? ArquivoBase64 { get; set; }
        public string? NomeArquivo { get; set; }

        public string? NomeMateria { get; set; }
        public string? TemaAtividade { get; set; }
    }

    public class QuestaoDTO
    {
        public string Texto { get; set; }
        public float Pontuacao { get; set; }
        public List<AlternativaDTO>? Alternativas { get; set; }
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
