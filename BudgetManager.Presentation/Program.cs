using BudgetManager.Application.Services;
using BudgetManager.Domain.Interfaces;
using BudgetManager.Infrastructure;
using BudgetManager.Infrastructure.Repositories;
using BudgetManager.Infrastructure.TelegramBot;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddScoped<IUserRepository, UserRepository>();

services.AddScoped<UserService>();

services.AddTelegramBot(configuration["Telegram:Token"]);

builder.Services.AddSwaggerGen();
services.AddControllers();

services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
dbContext.Database.Migrate();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();