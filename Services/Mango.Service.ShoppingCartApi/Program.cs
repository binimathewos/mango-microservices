using Mango.Service.ShoppingCartApi;
using Microsoft.EntityFrameworkCore;
using Mango.Service.ShoppingCartApi.Data;
using AutoMapper;
using Microsoft.OpenApi;
using Mango.Service.ShoppingCartApi.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Mango.Service.ShoppingCartApi.Sevice.IService;
using Mango.Service.ShoppingCartApi.Sevice;
using Mango.Service.ShoppingCartApi.Utility;
using Mango.MessageBus;

var builder = WebApplication.CreateBuilder(args);

// Application Insights — reads APPLICATIONINSIGHTS_CONNECTION_STRING from config/env.
builder.Services.AddApplicationInsightsTelemetry();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<BackendApiAuthenticationHttpClientHandler>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IMessageBus>(_ =>
    new MessageBus(builder.Configuration.GetValue<string>("ServiceBusConnectionString")));

builder.Services.AddHttpClient("Product", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ServiceUrls:ProductAPI"]
        ?? throw new InvalidOperationException("ServiceUrls:ProductAPI is not configured."));

}).AddHttpMessageHandler<BackendApiAuthenticationHttpClientHandler>();

builder.Services.AddHttpClient("Coupon", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ServiceUrls:CouponAPI"]
        ?? throw new InvalidOperationException("ServiceUrls:CouponAPI is not configured."));
}).AddHttpMessageHandler<BackendApiAuthenticationHttpClientHandler>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Product API",
        Version = "v1"
    });

    options.AddSecurityDefinition(
        JwtBearerDefaults.AuthenticationScheme,
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = JwtBearerDefaults.AuthenticationScheme
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecuritySchemeReference(
                    JwtBearerDefaults.AuthenticationScheme,
                    document)
            ] = []
        });
});

builder.AddAppAuthentication();

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Swagger UI is enabled in all environments (Production included).
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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