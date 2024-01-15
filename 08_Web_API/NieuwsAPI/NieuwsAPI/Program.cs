using NieuwsAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    // default policy instellen
    /*options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("null");
    });*/

    /*options.AddPolicy("LocalHostOnly", builder =>
    {
        builder.WithOrigins("null");
    });*/
    options.AddPolicy("AllowAllOrigins",
    builder =>
    {
        builder.AllowAnyOrigin();
    });
});

builder.Services.AddDbContext<NieuwsAPIContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NieuwsAPIContext") ?? throw new InvalidOperationException("Connection string 'NieuwsAPIContext' not found.")));

// Add services to the container.
builder.Services.AddSingleton<NieuwsberichtRepository>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NieuwsAPI", Version = "v1" });

    // Set the comments path for the Swagger JSON and UI.
    // Vergeet niet de xml file generation te enablen in <Project>.csproj
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
}
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");
app.UseAuthorization();

app.MapControllers();

app.Run();
