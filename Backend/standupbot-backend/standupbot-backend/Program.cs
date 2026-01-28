using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using standupbot_backend.Data;
using standupbot_backend.Services;

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
// Services (Dependency Injection)
// ===============================
builder.Services.AddScoped<IStandupService, StandupService>();

// ===============================
// CORS
// ===============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ===============================
// Controllers
// ===============================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

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
        options.RoutePrefix = "swagger";
    });
}

// CORS must be before authorization and HTTPS redirection
app.UseCors("AllowAngularApp");

// Only redirect to HTTPS in production (causes CORS preflight issues in development)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
