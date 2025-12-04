using Microsoft.EntityFrameworkCore;
using ProductsAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. CONFIGURAÇÃO DOS SERVIÇOS
// =========================================================

builder.Services.AddControllers();

// Configuração do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- MUDANÇA CRUCIAL AQUI (INÍCIO) ---
// 1. Tenta ler a Connection String do Docker (Variável de Ambiente) ou do appsettings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Se a string estiver vazia (caso algo falhe), define um fallback para evitar crash imediato
if (string.IsNullOrEmpty(connectionString))
{
    // Fallback apenas para não dar erro nulo, mas o ideal é vir do Docker
    connectionString = "Server=sql_server;Database=ProductsDB;User Id=sa;Password=GrupoE2526!;TrustServerCertificate=True;";
}

// 3. Regista o DbContext usando explicitamente o SQL Server com as opções configuradas
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// =========================================================
// 2. CRIAÇÃO AUTOMÁTICA DA BASE DE DADOS
// =========================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        
        bool created = context.Database.EnsureCreated();
        
        if (created)
            Console.WriteLine("--> SUCESSO: Base de Dados e Tabelas criadas no Docker!");
        else
            Console.WriteLine("--> INFO: A Base de Dados já existe.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> ERRO CRÍTICO: Não foi possível ligar ao SQL Server.");
        Console.WriteLine($"--> Erro detalhado: {ex.Message}");
    }
}

// =========================================================
// 3. PIPELINE HTTP
// =========================================================

app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductsAPI v1"));

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();