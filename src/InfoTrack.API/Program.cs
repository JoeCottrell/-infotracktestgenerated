using InfoTrack.API.Data;
using InfoTrack.API.Services;
using InfoTrack.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Services ─────────────────────────────────────────────────────────────

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("InfoTrackDb"));

builder.Services.AddHttpClient<ISolicitorScraperService, SolicitorScraperService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.BaseAddress = new Uri("https://www.solicitors.com");
});

builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<ISearchHistoryService, SearchHistoryService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "InfoTrack Solicitors API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSPA", policy =>
        policy.WithOrigins(
                "http://localhost:5173",   // Vite dev server
                "http://localhost:3000"    // CRA / fallback
              )
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ─── Seed the in-memory database ──────────────────────────────────────────

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// ─── Middleware pipeline ───────────────────────────────────────────────────

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSPA");
app.MapControllers();

app.Run();
