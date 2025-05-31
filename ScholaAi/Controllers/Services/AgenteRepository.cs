using ScholaAi.Dados;
using ScholaAi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ScholaAi.Controllers.Services
{
    public class AgenteRepository
    {
        private readonly AppDbContext _context;

        public AgenteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Agente>> GetEducadoresAsync()
        {
            return await _context.Agente
                .Where(a => a.IdTipoAgente == 2)
                .Select(a => new Agente { Id = a.Id,Login = a.Login})
                .ToListAsync();
        }
        public async Task<int> BuscarTipoAgente(int id)
        {
            return await _context.Agente
                .Where(a => a.Id == id)
                .Select(a => a.IdTipoAgente)
                .FirstOrDefaultAsync();
        }
        public async Task<int> BuscarNivelEducador(int id)
        {
            return await _context.Educador
                .Where(a => a.Id == id)
                .Select(a => a.IdAgente)
                .FirstOrDefaultAsync();
        }

    }

}
