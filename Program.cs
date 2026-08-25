using Microsoft.EntityFrameworkCore;
using SessionTrackerApi.Application.Interfaces;
using SessionTrackerApi.Infrastructure.ExternalServices;
using SessionTrackerApi.Infrastructure.Persistence;
using SessionTrackerApi.Application.BackgroundServices;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=sessions.db"));

builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddHostedService<SessionReminderWorker>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Serve wwwroot/index.html for the root URL and all static assets
app.UseDefaultFiles();   // maps "/" → "/index.html"
app.UseStaticFiles();    // serves wwwroot/ files

app.UseAuthorization();
app.MapControllers();

app.Run();