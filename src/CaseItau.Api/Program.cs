using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Adiciona suporte a controllers
builder.Services.AddControllers();

// Habilita CORS para permitir requisi��es do front (localhost ou arquivo local)
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
        Description = "API para classifica��o autom�tica de reclama��es"
    });
});

var app = builder.Build();

// Aplica a pol�tica CORS para permite receber requisi��es do meu front local
app.UseCors("AllowAll");

// Configura o pipeline de requisi��es
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