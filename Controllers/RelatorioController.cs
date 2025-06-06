using Microsoft.AspNetCore.Mvc;
using ScholaAi.Dados;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using PdfSharpCore.Pdf.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PdfSharpCore.Pdf;

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
        [HttpGet("relatorio_completo/{idAluno}")]
        public async Task<IActionResult> BaixarRelatorioCompleto(int idAluno)
        {
            try
            {
                var atividades = await _context.AlunoAtividadeMateria
                    .Include(a => a.Atividade)
                        .ThenInclude(at => at.Questoes)
                            .ThenInclude(q => q.Alternativas)
                    .Where(a => a.IdAluno == idAluno && a.Atividade.Publicada)
                    .ToListAsync();

                if(!atividades.Any())
                    return NotFound("Nenhuma atividade encontrada para o aluno informado.");

                var anexosPdf = new List<byte[]>();

                var pdfRelatorio = QuestPDF.Fluent.Document.Create(container =>
                {
                    foreach(var item in atividades)
                    {
                        var atividade = item.Atividade;
                        var base64 = atividade.ArquivoBase64;

                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(20);
                            page.DefaultTextStyle(x => x.FontSize(14));

                            page.Content().Column(col =>
                            {
                                col.Item().Text($"Atividade: {atividade.Nome}").Bold().FontSize(18);

                                if(!string.IsNullOrWhiteSpace(base64) && base64.Contains("image"))
                                {
                                    try
                                    {
                                        var base64Data = base64.Split(',').Last();
                                        var bytes = Convert.FromBase64String(base64Data);
                                        col.Item().Image(bytes);
                                    }
                                    catch
                                    {
                                        col.Item().Text("[Erro ao carregar imagem]");
                                    }
                                }

                                if(!string.IsNullOrWhiteSpace(base64) && base64.Contains("pdf"))
                                {
                                    try
                                    {
                                        var base64Data = base64.Split(',').Last();
                                        var pdfBytes = Convert.FromBase64String(base64Data);
                                        anexosPdf.Add(pdfBytes);
                                    }
                                    catch
                                    {
                                        // log 
                                    }
                                }

                                if(atividade.Questoes != null && atividade.Questoes.Any())
                                {
                                    foreach(var questao in atividade.Questoes)
                                    {
                                        col.Item().Text($"Questão: {questao.Texto}").Bold();
                                        foreach(var alt in questao.Alternativas)
                                        {
                                            col.Item().Text($"- {alt.Texto} {(alt.Correta ? "(Correta)" : "")}");
                                        }
                                    }
                                }

                                col.Item().Text($"Pontuação Obtida: {item.Pontuacao ?? 0}").Italic();
                            });
                        });
                    }
                }).GeneratePdf();

                var pdfFinal = MesclarPdfComAnexos(pdfRelatorio,anexosPdf);

                return File(pdfFinal,"application/pdf",$"RelatorioCompleto_{idAluno}.pdf");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Erro ao gerar relatório: " + ex.Message);
                return StatusCode(500,$"Erro interno: {ex.Message}");
            }
        }

        private byte[] MesclarPdfComAnexos(byte[] pdfPrincipal,List<byte[]> anexosPdf)
        {
            using var outputDocument = new PdfDocument();

            using(var stream = new MemoryStream(pdfPrincipal))
            {
                var inputDoc = PdfReader.Open(stream,PdfDocumentOpenMode.Import);
                for(int i = 0;i < inputDoc.PageCount;i++)
                {
                    outputDocument.AddPage(inputDoc.Pages[i]);
                }
            }

            foreach(var pdfAnexo in anexosPdf)
            {
                using var stream = new MemoryStream(pdfAnexo);
                var inputDoc = PdfReader.Open(stream,PdfDocumentOpenMode.Import);
                for(int i = 0;i < inputDoc.PageCount;i++)
                {
                    outputDocument.AddPage(inputDoc.Pages[i]);
                }
            }

            using var outputStream = new MemoryStream();
            outputDocument.Save(outputStream);
            return outputStream.ToArray();
        }
    }
    public class PdfMergeRequest
    {
        public byte[] PdfPrincipal { get; set; }
        public List<byte[]> AnexosPdf { get; set; }
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
