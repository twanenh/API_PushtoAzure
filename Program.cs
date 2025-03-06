
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
//Trước 
//builder.Services.AddDbContext<AppDbContext>(options =>
//options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

builder.Services.AddDistributedMemoryCache(); // Hoặc AddDistributedSqlServerCache()
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7); // Giữ session 7 ngày
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    // Thay đổi SameSite từ None sang Lax (hoặc sử dụng None cho production với HTTPS)
    options.Cookie.SameSite = SameSiteMode.Lax;

    // Giữ nguyên tên cookie
    options.Cookie.Name = ".AspNetCore.Identity.Application";

    // Cấu hình sự kiện chuyển hướng tốt hơn cho API
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("https://tuanbeo.vercel.app", "http://localhost:4200")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseCors("AllowAngular");




if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "/openapi/{documentName}.json";
    });
    app.MapScalarApiReference();
}
app.UseSession();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
// Thêm middleware để log request và cookies
// Thêm middleware để log request và cookies
app.Use(async (context, next) =>
{
    // Log request
    var path = context.Request.Path;
    var method = context.Request.Method;
    var identity = context.User.Identity;
    var isAuthenticated = identity?.IsAuthenticated ?? false;
    Console.WriteLine($"Request: {method} {path}, Authenticated: {isAuthenticated}");

    // Log cookies cho các route API
    if (path.StartsWithSegments("/api"))
    {
        var cookies = context.Request.Cookies;
        Console.WriteLine($"Cookies: {string.Join(", ", cookies.Select(c => $"{c.Key}={c.Value}"))}");
    }

    await next();

    // Log response status
    Console.WriteLine($"Response: {context.Response.StatusCode}");
});

app.MapControllers();

app.Run();
