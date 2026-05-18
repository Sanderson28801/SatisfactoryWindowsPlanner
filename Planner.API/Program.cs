using Planner.Core.Interfaces;
using Planner.Core.Repositories;
using Planner.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 1. Tell the server where the JSON file is
string dataPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "Items&Recipes.json");

// 2. Load the Repository ONCE as a Singleton (it stays in memory forever)
builder.Services.AddSingleton<IDataRepository>(new JsonDataRepository(dataPath));

// 3. Load the Engine. "AddScoped" means it creates a fresh engine for every web request
builder.Services.AddScoped<ProductionEngine>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        // 5173 is the default port for Vite. Update it if your terminal says otherwise!
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowReactApp");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
