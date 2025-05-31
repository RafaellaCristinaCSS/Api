using Microsoft.AspNetCore.Mvc;
using ScholaAi.Dados;
using ScholaAi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScholaAi.Controllers
{
    [ApiController]
    [Route("api/materia")]
    public class MateriaController :ControllerBase
    {
        private readonly AppDbContext _context;
        public MateriaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CriarMateria([FromBody] MateriaDTO materiaDto)
        {
            if(string.IsNullOrEmpty(materiaDto.Nome))
            {
                return BadRequest("O nome da matéria é obrigatório.");
            }

            var materia = new Materia
            {
                Nome = materiaDto.Nome,
                Imagem = materiaDto.Imagem
            };

            _context.Materia.Add(materia);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CriarMateria),new { id = materia.Id },materia);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MateriaDTOResposta>>> ObterMaterias()
        {
            var materias = await _context.Materia
                .Select(m => new MateriaDTOResposta { Id = m.Id,Nome = m.Nome })
                .ToListAsync();

            return Ok(materias);
        }
    }

    public class MateriaDTO
    {
        public string Nome { get; set; }
        public string Imagem { get; set; }
    }

    public class MateriaDTOResposta
    {
        public int Id { get; set; }
        public string Nome { get; set; }
    }
}
