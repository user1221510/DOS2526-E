var builder = WebApplication.CreateBuilder(args);

// (Opcional) configurações, serviços

builder.Services.AddControllers();
// Swagger / OpenAPI (recomendado para testes)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware do pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();  // opcional

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();