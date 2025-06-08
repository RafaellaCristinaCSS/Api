using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScholaAi.Dados;
using ScholaAi.Models;

namespace ScholaAi.Controllers
{
    [ApiController]
    [Route("api/blocoNotas")]
    public class BlocoNotasController :ControllerBase
    {
        private readonly AppDbContext _context;

        public BlocoNotasController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("{idAgente}")]
        public async Task<IActionResult> ObterBlocoPorAgente(int idAgente)
        {
            var blocoNotas = await _context.BlocoNotas
                .FirstOrDefaultAsync(b => b.IdAgente == idAgente);

            if(blocoNotas == null)
            {
                BlocoNotas bloco = new BlocoNotas { Anotacao = "" };
                return Ok(bloco);
            }

            return Ok(blocoNotas);
        }
        [HttpPost]
        public async Task<IActionResult> AdicionarOuAtualizarAnotacao([FromBody] BlocoNotasDTO blocoNotasDto)
        {
            if(string.IsNullOrEmpty(blocoNotasDto.Anotacao)) blocoNotasDto.Anotacao = "";
            var blocoExistente = await _context.BlocoNotas
                .FirstOrDefaultAsync(b => b.IdAgente == blocoNotasDto.IdAgente);

            if(blocoExistente != null)
            {
                blocoExistente.Anotacao = blocoNotasDto.Anotacao;
                _context.BlocoNotas.Update(blocoExistente);
                await _context.SaveChangesAsync();

                return Ok(blocoExistente);
            }

            var novoBloco = new BlocoNotas
            {
                IdAgente = blocoNotasDto.IdAgente,
                Anotacao = blocoNotasDto.Anotacao,
            };

            _context.BlocoNotas.Add(novoBloco);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(AdicionarOuAtualizarAnotacao),new { id = novoBloco.Id },novoBloco);
        }
    }
    public class BlocoNotasDTO
    {
        public int IdAgente { get; set; }
        public string Anotacao { get; set; }
    }
}
