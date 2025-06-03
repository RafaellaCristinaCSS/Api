using Microsoft.AspNetCore.Mvc;
using ScholaAi.Dados;
using ScholaAi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace ScholaAi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgenteController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AgenteController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> CriarAgente([FromBody] AgenteDTO agenteDto)
        {
            if (string.IsNullOrEmpty(agenteDto.Login) || string.IsNullOrEmpty(agenteDto.Senha))
            {
                return BadRequest("Login e senha são obrigatórios.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var agente = new Agente
                {
                    Login = agenteDto.Login,
                    Nome = agenteDto.Nome,
                    IdTipoAgente = agenteDto.IdTipoAgente,
                    Senha = BCrypt.Net.BCrypt.HashPassword(agenteDto.Senha) 
                };

                _context.Agente.Add(agente);
                await _context.SaveChangesAsync(); 

                if (agenteDto.IdTipoAgente == 1) 
                {
                    if (agenteDto.DataNascimento == null || agenteDto.IdEducador == null)
                    {
                        return BadRequest("Data de Nascimento e Id do Educador são obrigatórios para Aluno.");
                    }

                    var aluno = new Aluno
                    {
                        IdAgente = agente.Id,
                        IdEducador = agenteDto.IdEducador.Value,
                        DataNascimento = agenteDto.DataNascimento.Value
                    };
                    _context.Aluno.Add(aluno);
                }
                else if (agenteDto.IdTipoAgente == 2) 
                {
                    var educador = new Educador
                    {
                        IdAgente = agente.Id,
                        NivelVisibilidade = 1 
                    };
                    _context.Educador.Add(educador);
                }
                else
                {
                    return BadRequest("Tipo de Agente inválido. Use 1 para Aluno ou 2 para Educador.");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(CriarAgente), new { id = agente.Id }, new { agente.Id, agente.Login });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Erro ao criar agente: {ex.Message}");
            }
        }
        [HttpPost("complementar-informacoes")]
        public async Task<IActionResult> ComplementarInformacoesAluno([FromBody] AlunoInformacoesDTO dto)
        {
            var aluno = await _context.Aluno.FirstOrDefaultAsync(a => a.IdAgente == dto.IdAgente);

            if(aluno == null)
                return NotFound("Aluno não encontrado");

            aluno.EstiloAprendizagem = dto.EstiloAprendizagem;
            aluno.GeneroLiterarioFavorito = dto.GeneroLiterarioFavorito;
            aluno.DataNascimento = dto.DataNascimento;
            aluno.ModeloEnsino = dto.ModeloEnsino;
            aluno.HorasEstudo = dto.HorasEstudo;
            aluno.Hobbies = dto.Hobbies;
            aluno.InformacaoAdicional = dto.InformacaoAdicional;

            await _context.SaveChangesAsync();

            return Ok("Informações complementares salvas com sucesso");
        }
        [HttpGet("aluno/{id}")]
        public async Task<IActionResult> ObterDadosAlun(int id)
        {
            var aluno = await _context.Aluno.FindAsync(id);

            if(aluno == null)
                return NotFound();

            var alunoDto = new AlunoInformacoesDTO
            {
                DataNascimento = aluno.DataNascimento,
                EstiloAprendizagem = aluno.EstiloAprendizagem,
                GeneroLiterarioFavorito = aluno.GeneroLiterarioFavorito,
                ModeloEnsino = aluno.ModeloEnsino,
                HorasEstudo = aluno.HorasEstudo,
                Hobbies = aluno.Hobbies,
                InformacaoAdicional = aluno.InformacaoAdicional
            };

            return Ok(alunoDto);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            var agente = await _context.Agente.FirstOrDefaultAsync(a => a.Login == loginDto.Login);

            if(agente == null || !BCrypt.Net.BCrypt.Verify(loginDto.Senha,agente.Senha))
            {
                return Unauthorized("Login ou senha inválidos.");
            }

            int? nivelEducador = null;
            int? idAluno = null;
            int? idEducador = null;

            if(agente.IdTipoAgente == 1)
            {
                idAluno = await _context.Aluno
                    .Where(a => a.IdAgente == agente.Id)
                    .Select(a => a.Id)
                    .FirstOrDefaultAsync();
            }
            else
            {
                var educador = await _context.Educador
                    .Where(e => e.IdAgente == agente.Id)
                    .Select(e => new { e.Id,e.NivelVisibilidade })
                    .FirstOrDefaultAsync();

                if(educador != null)
                {
                    nivelEducador = educador.NivelVisibilidade;
                    idEducador = educador.Id;
                }
            }

            var secretKey = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]);
            var securityKey = new SymmetricSecurityKey(secretKey);
            var credentials = new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, agente.Login),
                new Claim("Id", agente.Id.ToString()),
                new Claim("Tipo", agente.IdTipoAgente.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if(agente.IdTipoAgente == 1)
            {
                claims.Add(new Claim("Nivel","aluno"));
                claims.Add(new Claim(ClaimTypes.Role,"aluno"));
            }
            else if(nivelEducador != null)
            {
                string nivelTexto = nivelEducador switch
                {
                    1 => "basico",
                    2 => "contribuidor",
                    3 => "admin",
                    _ => "desconhecido"
                };

                claims.Add(new Claim("Nivel",nivelTexto));
                claims.Add(new Claim(ClaimTypes.Role,nivelTexto));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                Token = tokenString,
                IdAluno = idAluno,
                IdEducador = idEducador
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterAgente(int id)
        {
            var agente = await _context.Agente.FindAsync(id);
            if (agente == null) return NotFound();

            return Ok(agente);
        }

        [HttpGet]
        public async Task<IActionResult> ListarAgentes()
        {
            var agentes = await _context.Agente.ToListAsync();
            return Ok(agentes);
        }

        [HttpGet("buscarLoginEducadores")]
        public async Task<IActionResult> GetEducadores()
        {
            var educadores = await _context.Agente
                .Where(a => a.IdTipoAgente == 2)
                .Select(a => new { a.Id, a.Login })
                .ToListAsync();

            return Ok(educadores);
        }

        [HttpGet("buscarDadosEducadores")]
        public async Task<ActionResult<IEnumerable<AgenteEducadorDTO>>> GetEducadoresAsync()
        {
            var educadores = await _context.Agente
                .Where(a => a.IdTipoAgente == 2)
                .Join(
                    _context.Educador,
                    agente => agente.Id,
                    educador => educador.IdAgente,
                    (agente, educador) => new AgenteEducadorDTO
                    {
                        IdAgente = agente.Id,
                        Login = agente.Login,
                        NomeEducador = agente.Nome,
                        IdEducador = educador.Id,
                        NivelVisibilidade = educador.NivelVisibilidade
                    })
                .Where(dto => dto.NivelVisibilidade != 3)
                .ToListAsync();

            return Ok(educadores);
        }
        [HttpGet("buscarAlunosPorEducador/{idAgenteEducador}")]
        public async Task<ActionResult<IEnumerable<AgenteAlunoDTO>>> GetAlunosPorEducadorAsync(int idAgenteEducador)
        {
            var alunos = await _context.Aluno
                .Where(aluno => aluno.IdEducador == idAgenteEducador)
                .Join(
                    _context.Agente.Where(agente => agente.IdTipoAgente == 1),
                    aluno => aluno.IdAgente,
                    agente => agente.Id,
                    (aluno,agente) => new AgenteAlunoDTO
                    {
                        Nome = agente.Nome,
                        IdAluno = aluno.Id
                    })
                .ToListAsync();

            return Ok(alunos);
        }
        //[Authorize(Roles = "admin")]
        [HttpPut("educador/{id}/editarNivelVisibilidade")]
        public async Task<IActionResult> AtualizarNivelVisibilidade(int id,[FromBody] AtualizarVisibilidadeDTO dto)
        {
            try
            {
                if(dto.NivelVisibilidade < 1 || dto.NivelVisibilidade > 3)
                {
                    return BadRequest("O nível de visibilidade deve ser 1, 2 ou 3.");
                }

                var educador = await _context.Educador.FindAsync(id);
                if(educador == null)
                {
                    return NotFound("Educador não encontrado.");
                }

                educador.NivelVisibilidade = dto.NivelVisibilidade;
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Nível de visibilidade atualizado com sucesso!",educador });
            }
            catch(Exception ex)
            {
                throw new Exception($"Erro :: AgenteController - AtualizarNivelVisibilidade - Erro interno ao atualizar visibilidade {ex.Message}");
            }
        }
    }
    public class AlunoInformacoesDTO
    {
        public int IdAgente { get; set; }
        public string? EstiloAprendizagem { get; set; }
        public DateTime DataNascimento { get; set; }
        public string? GeneroLiterarioFavorito { get; set; }
        public string? ModeloEnsino { get; set; }
        public string? HorasEstudo { get; set; }
        public string? Hobbies { get; set; }
        public string? InformacaoAdicional { get; set; }
    }

    public class AtualizarVisibilidadeDTO
    {
        public int NivelVisibilidade { get; set; }
    }
    public class AgenteDTO
    {
        public string Login { get; set; }
        public string Senha { get; set; }
        public string Nome { get; set; }
        public int IdTipoAgente { get; set; }
        public DateTime? DataNascimento { get; set; }
        public int? IdEducador { get; set; }
    }
    public class AgenteEducadorDTO
    {
        public int IdAgente { get; set; }
        public int IdEducador { get; set; }
        public string Login { get; set; }
        public string NomeEducador { get; set; }
        public int NivelVisibilidade { get; set; }
    }
    
    public class AgenteAlunoDTO
    {
        public string Nome { get; set; }
        public int IdAluno { get; set; }
        public int IdEducadorVinculado { get; set; }
    }
    public class LoginDTO
    {
        public string Login { get; set; }
        public string Senha { get; set; }
    }
}
