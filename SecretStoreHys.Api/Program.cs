using System.Reflection;
using Microsoft.OpenApi.Models;
using SecretStoreHys.Api;
using SecretStoreHys.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SecretStore.Api", 
        Version = "v1", 
        Contact = new OpenApiContact()
        {
            Name = "Nikita Reznikov",
            Email = "nreznikov@gmail.com"
        },
        Description = "A simple secret store API."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddTransient<ISecretService, SecretService>();
builder.Services.AddHostedService<SecretsCleanerJob>();

builder.Services.AddCors(o => o.AddPolicy("MyPolicy", corsPolicyBuilder =>
{
    corsPolicyBuilder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

var app = builder.Build();

app.UseCors("MyPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();

app.MapControllers();

app.Run();