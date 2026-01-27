using Microsoft.EntityFrameworkCore;
using standupbot_backend.Data;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// Database (Entity Framework Core)
// ===============================
builder.Services.AddDbContext<StandupBotContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

// ===============================
// Controllers
// ===============================
builder.Services.AddControllers();

// ===============================
// Swagger (API Documentation)
// ===============================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "StandupBot API",
        Version = "v1",
        Description = "Backend API for StandupBot application"
    });
});

var app = builder.Build();

// ===============================
// HTTP Pipeline
// ===============================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "StandupBot API v1");
        options.RoutePrefix = "swagger"; // default
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
