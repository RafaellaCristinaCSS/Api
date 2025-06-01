using Microsoft.AspNetCore.Mvc;
using ScholaAi.Dados;
using ScholaAi.Models;

namespace ScholaAi.Controllers
{
    [ApiController]
    [Route("api/hobby")]
    public class HobbyController :ControllerBase
    {
        private readonly AppDbContext _context;

        public HobbyController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarHobby([FromBody] HobbyDTO hobbyDto)
        {
            if(string.IsNullOrEmpty(hobbyDto.Nome))
            {
                return BadRequest("O nome do hobby é obrigatório.");
            }

            var hobby = new Hobby
            {
                Name = hobbyDto.Nome,
            };

            _context.Hobby.Add(hobby);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(AdicionarHobby),new { id = hobby.Id },hobby);
        }
    }
    public class HobbyDTO
    {
        public string Nome { get; set; }
    }
}
