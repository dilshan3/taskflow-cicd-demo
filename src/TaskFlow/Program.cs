using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using TaskFlow.Data;
using TaskFlow.Services;

var builder = WebApplication.CreateBuilder(args);

// Serialize enums as their names ("High", "InProgress") instead of numbers,
// so the JSON and the Swagger UI read the way the model does.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskFlow API",
        Version = "v1",
        Description = "A tiny in-memory Task Management API used to teach CI/CD.",
    }));

// One shared instance holds the in-memory tasks. The interface is what the
// controller asks for; the concrete type is what we seed on startup.
builder.Services.AddSingleton<TaskService>();
builder.Services.AddSingleton<ITaskService>(sp => sp.GetRequiredService<TaskService>());

var app = builder.Build();

// Load the sample tasks so the demo has data the moment it starts.
app.Services.GetRequiredService<TaskService>().Seed(SampleData.Tasks());

// Swagger is enabled in every environment (including Azure) and served at the
// site root, so opening the URL lands straight on the interactive docs.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API v1");
    options.RoutePrefix = string.Empty;
});

app.MapControllers();

app.Run();
