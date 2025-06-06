using Microsoft.AspNetCore.Mvc;
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

        [HttpPost]
        public async Task<IActionResult> AdicionarAnotacao([FromBody] BlocoNotasDTO blocoNotasDto)
        {
            if(string.IsNullOrEmpty(blocoNotasDto.Anotacao)) blocoNotasDto.Anotacao = "";
      
            var blocoNotas = new BlocoNotas
            {
                Anotacao = blocoNotasDto.Anotacao,
            };

            _context.BlocoNotas.Add(blocoNotas);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(AdicionarAnotacao),new { id = blocoNotas.Id },blocoNotas);
        }
    }
    public class BlocoNotasDTO
    {
        public string Anotacao { get; set; }
    }
}
