using Microsoft.EntityFrameworkCore;
using ProductsAPI.Data; // Importante para encontrar o AppDbContext

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. CONFIGURAÇÃO DOS SERVIÇOS
// =========================================================

builder.Services.AddControllers();

// Configuração do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- NOVO: REGISTAR O CONTEXTO DA BASE DE DADOS ---
// Isto diz à aplicação que o AppDbContext existe e pode ser injetado nos Controllers
builder.Services.AddDbContext<AppDbContext>();

var app = builder.Build();

// =========================================================
// 2. CRIAÇÃO AUTOMÁTICA DA BASE DE DADOS (SOLUÇÃO SEM COMANDOS)
// =========================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 1. Pede uma instância do contexto
        var context = services.GetRequiredService<AppDbContext>();
        
        // 2. Comando mágico: Cria a BD e Tabelas no Docker se não existirem
        bool created = context.Database.EnsureCreated();
        
        if (created)
            Console.WriteLine("--> SUCESSO: Base de Dados e Tabelas criadas no Docker!");
        else
            Console.WriteLine("--> INFO: A Base de Dados já existe. A saltar criação.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> ERRO CRÍTICO: Não foi possível ligar ao SQL Server. Verifique se o Docker está a correr. Erro: {ex.Message}");
    }
}

// =========================================================
// 3. PIPELINE HTTP (IGUAL AO TEU ANTIGO)
// =========================================================

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Mantive do teu original
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Mantive comentado como tinhas

app.UseRouting(); // Mantive do teu original

app.UseAuthorization();

app.MapControllers();

app.Run();