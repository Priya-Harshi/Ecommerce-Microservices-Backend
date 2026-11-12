using AuthService.Data;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration
            .GetConnectionString("AuthDb")));

builder.Services.AddScoped<
    IAuthService,
    AuthService.Services.AuthService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
            .GetRequiredService<AuthDbContext>();

    await context.Database.MigrateAsync();

    await AdminSeeder.SeedAsync(context);
}


app.UseSwagger();
    app.UseSwaggerUI();
//app.UseHttpsRedirection();

app.MapControllers();

app.Run();