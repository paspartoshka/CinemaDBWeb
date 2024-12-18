using CinemaDBWeb.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Регистрация контекста базы данных в DI контейнере
builder.Services.AddDbContext<CinemaDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // Замените на ваш реальный строковый параметр соединения
builder.Services.AddScoped<CinemaDBStorage>();



// Добавление сервисов для MVC
builder.Services.AddControllersWithViews();


var app = builder.Build();

app.UseStaticFiles();

// Настройка маршрутизации
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Movies}/{action=Index}/{id?}");

app.Run();