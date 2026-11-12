using Microsoft.EntityFrameworkCore;
using OrderService.Clients;
using OrderService.Data;
using OrderService.Hubs;
using OrderService.Repositories;
using OrderService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!))
            };
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    const string schemeId = "Bearer";

    options.AddSecurityDefinition(
        schemeId,
        new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Name = "Authorization",
            In = Microsoft.OpenApi.ParameterLocation.Header,
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description =
                "Enter your JWT token. Example: Bearer eyJhbGciOi..."
        });

    options.AddSecurityRequirement(document =>
        new Microsoft.OpenApi.OpenApiSecurityRequirement
        {
            [
                new Microsoft.OpenApi.OpenApiSecuritySchemeReference(
                    schemeId,
                    document)
            ] = []
        });
});

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration
            .GetConnectionString("OrderDb")));

builder.Services.AddScoped<
    IOrderRepository,
    OrderRepository>();

builder.Services.AddScoped<
    IOrderService,
    OrderService.Services.OrderService>();

var productServiceUrl =
    builder.Configuration["ServiceUrls:Product"]
    ?? throw new InvalidOperationException(
        "Missing configuration: ServiceUrls:Product");

var inventoryServiceUrl =
    builder.Configuration["ServiceUrls:Inventory"]
    ?? throw new InvalidOperationException(
        "Missing configuration: ServiceUrls:Inventory");

var paymentServiceUrl =
    builder.Configuration["ServiceUrls:Payment"]
    ?? throw new InvalidOperationException(
        "Missing configuration: ServiceUrls:Payment");

builder.Services.AddHttpClient<ProductServiceClient>(client =>
{
    client.BaseAddress =
        new Uri(productServiceUrl);
});

builder.Services.AddHttpClient<InventoryServiceClient>(client =>
{
    client.BaseAddress =
        new Uri(inventoryServiceUrl);
});

builder.Services.AddHttpClient<PaymentServiceClient>(client =>
{
    client.BaseAddress =
        new Uri(paymentServiceUrl);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<OrderDbContext>();

    dbContext.Database.Migrate();
}

    app.UseSwagger();
    app.UseSwaggerUI();

app.UseCors("Frontend");

//app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<OrderHub>("/hubs/orders");

app.Run();