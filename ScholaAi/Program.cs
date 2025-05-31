using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ScholaAi.Dados;

namespace ScholaAi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Obt�m a configura��o do arquivo appsettings.json
            var configuration = builder.Configuration;

            // Chave secreta para JWT
            var key = Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"]);

            // Configura��o da autentica��o JWT
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["JwtSettings:Issuer"],
                        ValidAudience = configuration["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });



            // Adiciona controladores
            builder.Services.AddControllers();

            // Configura��o do Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configura��o do banco de dados
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Configura��o do CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
            });

            var app = builder.Build();
            //using(var scope = app.Services.CreateScope())
            //{
            //    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            //    dbContext.Database.EnsureDeleted(); 
            //    dbContext.Database.Migrate();       
            //}

            // Middleware
            if(app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Ativa CORS antes de autentica��o/autoriza��o
            app.UseCors("AllowAllOrigins");

            // **Adiciona autentica��o antes da autoriza��o**
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Aplica migrations automaticamente ao iniciar a aplica��o
            using(var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            app.Run();

        }
    }
}
