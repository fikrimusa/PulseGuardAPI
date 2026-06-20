using Microsoft.EntityFrameworkCore;
using PulseGuard.Api.Data;
using PulseGuard.Api.Repositories;
using PulseGuard.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("PulseGuardDatabase")
    ?? throw new InvalidOperationException("Connection string 'PulseGuardDatabase' was not found.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<MonitorRepository>();
builder.Services.AddScoped<MonitorService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
