
using API_PushtoAzure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Scalar.AspNetCore;
using System;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Trước đây
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Thay bằng
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//if (builder.Environment.IsProduction())
//{
//    // Sử dụng MySQL trong môi trường production (Railway)
//    builder.Services.AddDbContext<AppDbContext>(options =>
//        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
//}
//else
//{
//    // Tiếp tục sử dụng SQL Server trong môi trường development
//    builder.Services.AddDbContext<AppDbContext>(options =>
//        options.UseSqlServer(connectionString));
//}

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/api/auth/login";
    options.LogoutPath = "/api/auth/logout";
    options.AccessDeniedPath = "/api/auth/accessdenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.Expiration = null;
    options.SlidingExpiration = false;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .WithOrigins("http://localhost:4200")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}




if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "/openapi/{documentName}.json";
    });
    app.MapScalarApiReference();
}
app.UseCors("AllowAllOrigins");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (!context.User.Identity.IsAuthenticated) // Nếu API không còn session
    {
        context.Response.Cookies.Delete(".AspNetCore.Identity.Application"); // Xóa cookie đăng nhập
    }
    await next();
});
app.MapControllers();
// Áp dụng migrations khi khởi động
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Đã xảy ra lỗi khi migrate database.");
    }
}
app.Run();
