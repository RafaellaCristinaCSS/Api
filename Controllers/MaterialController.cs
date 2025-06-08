using Microsoft.AspNetCore.Mvc;
using ScholaAi.Dados;
using ScholaAi.Models;
using Microsoft.EntityFrameworkCore;

namespace ScholaAi.Controllers
{
    [ApiController]
    [Route("api/materiais")]
    public class MaterialController :ControllerBase
    {
        private readonly AppDbContext _context;

        public MaterialController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CriarMaterial([FromBody] MaterialDTO materialDto)
        {
            if(string.IsNullOrEmpty(materialDto.Conteudo))
            {
                return BadRequest("O conteúdo do material é obrigatório.");
            }

            var idEducador = await _context.Educador
            .Where(e => e.IdAgente == materialDto.IdAgente)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

            if(idEducador == 0)
            {
                return BadRequest("O Educador informado não existe.");
            }

            var material = new Material
            {
                Conteudo = materialDto.Conteudo,
                IdEducadorCriador = idEducador,
                IdMateria = materialDto.IdMateria
            };

            _context.Material.Add(material);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CriarMaterial),new { id = material.Id },material);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarMaterial(int id,[FromBody] EditarConteudoDTO dto)
        {
            var material = await _context.Material.FindAsync(id);

            if(material == null || material.Excluido)
            {
                return NotFound("Material não encontrado ou já foi desativado.");
            }

            if(string.IsNullOrEmpty(dto.Conteudo))
            {
                return BadRequest("O conteúdo do material é obrigatório.");
            }

            material.Conteudo = dto.Conteudo;

            _context.Material.Update(material);
            await _context.SaveChangesAsync();

            return Ok(material);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirMaterial(int id)
        {
            var material = await _context.Material.FindAsync(id);

            if(material == null || material.Excluido)
            {
                return NotFound("Material não encontrado ou já está excluído.");
            }

            material.Excluido = true;

            _context.Material.Update(material);
            await _context.SaveChangesAsync();

            return Ok("Material excluído com sucesso.");
        }
        [HttpGet]
        public async Task<IActionResult> BuscarMateriais()
        {
            var materiais = await _context.Material
            .Where(m => m.Excluido == false)
            .Select(m => new
            {
                m.Id,
                m.Conteudo,
                m.IdMateria,
                NomeMateria = _context.Materia
                    .Where(mt => mt.Id == m.IdMateria)
                    .Select(mt => mt.Nome)
                    .FirstOrDefault(),

                m.IdEducadorCriador,
                NomeEducador = (
                    from e in _context.Educador
                    join a in _context.Agente on e.IdAgente equals a.Id
                    where e.Id == m.IdEducadorCriador
                    select a.Nome
                ).FirstOrDefault()
            })
            .ToListAsync();

            return Ok(materiais);
        }

    }
    public class EditarConteudoDTO
    {
        public string Conteudo { get; set; }
    }

    public class MaterialDTO
    {
        public string Conteudo { get; set; }
        public int IdAgente { get; set; }
        public int IdMateria { get; set; }
    }
}
