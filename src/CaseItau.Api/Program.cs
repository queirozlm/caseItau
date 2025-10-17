using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Adiciona suporte a controllers
builder.Services.AddControllers();

// Habilita CORS para permitir requisições do front (localhost ou arquivo local)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Adiciona o Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Case Itau API",
        Version = "v1",
        Description = "API para classificação automática de reclamações"
    });
});

var app = builder.Build();

// Aplica a política CORS para permite receber requisições do meu front local
app.UseCors("AllowAll");

// Configura o pipeline de requisições
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Case Itau API v1");
        c.RoutePrefix = string.Empty;
    });
}

//app.UseHttpsRedirection();

app.MapControllers();

app.Run();