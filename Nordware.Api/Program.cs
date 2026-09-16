var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Gera o documento OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Gera /openapi/v1.json
    app.MapOpenApi();

    // Interface Swagger
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Minha API v1"
        );
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();