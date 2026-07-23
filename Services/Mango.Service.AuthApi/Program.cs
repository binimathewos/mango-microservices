using Mango.Service.AuthApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Mango.Service.AuthApi.Models;
using Mango.Service.AuthApi.Services;
using Mango.Service.AuthApi.Service.IServices;
using Mango.MessageBus;

var builder = WebApplication.CreateBuilder(args);

// Application Insights — reads APPLICATIONINSIGHTS_CONNECTION_STRING from config/env.
builder.Services.AddApplicationInsightsTelemetry();

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("ApiSettings:JwtOptions"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();

builder.Services.AddScoped<IMessageBus>(_ =>
    new MessageBus(builder.Configuration.GetValue<string>("ServiceBusConnectionString")));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.MapHealthChecks("/health");

ApplyMigration();

app.Run();

void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (dbContext.Database.GetPendingMigrations().Any())
        {
            dbContext.Database.Migrate();
        }
    }
}
