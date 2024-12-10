using Hairdresser.Data;
using Hairdresser.Models;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// SQLite ba�lant�s�n� yap�land�rma
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/User/Login"; // Giri� yap�lmam��sa y�nlendirme yap�lacak sayfa
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Veritaban�n� olu�tur ve ba�lang�� verisi ekle
    dbContext.Database.Migrate();

    // Hizmetleri sadece bir kez eklemek i�in kontrol et
    // Ba�lang�� hizmetleri eklenir
    if (!dbContext.Services.Any())
    {
        dbContext.Services.AddRange(
            new Service { Id = 1, Name = "Sa� Kesimi", Price = 700, Duration = 1 },
            new Service { Id = 2, Name = "Sa� Boyas�", Price = 1500, Duration = 3 },
            new Service { Id = 3, Name = "Sa� Bak�m�", Price = 1000, Duration = 2 },
            new Service { Id = 4, Name = "Profesyonel Makyaj", Price = 1000, Duration = 2 },
            new Service { Id = 5, Name = "Nail Art", Price = 1500, Duration = 1 },
            new Service { Id = 6, Name = "Cilt Bak�m�", Price = 1500, Duration = 3 }
        );
        // Veritaban�na kaydet
        await dbContext.SaveChangesAsync();
    }

    // �al��anlar� sadece bir kez eklemek i�in kontrol et
    // Ba�lang�� �al��anlar� eklenir
    if (!dbContext.Employees.Any()) // E�er �al��anlar yoksa ekle
    {
        dbContext.Employees.AddRange(
            new Employee { Id = 1, FullName = "Ahmet Y�lmaz", ServiceId = 1, AvailableHours = "09:00-17:00" },
            new Employee { Id = 2, FullName = "Ay�e Kaya", ServiceId = 1, AvailableHours = "10:00-18:00" },
            new Employee { Id = 3, FullName = "Mehmet Demir", ServiceId = 1, AvailableHours = "09:00-15:00" },
            new Employee { Id = 4, FullName = "Fatma �elik", ServiceId = 2, AvailableHours = "10:00-16:00" },
            new Employee { Id = 5, FullName = "Hasan Y�ld�z", ServiceId = 2, AvailableHours = "11:00-19:00" },
            new Employee { Id = 6, FullName = "Zeynep Ayd�n", ServiceId = 2, AvailableHours = "08:00-16:00" },
            new Employee { Id = 7, FullName = "Veli Aksoy", ServiceId = 3, AvailableHours = "10:00-18:00" },
            new Employee { Id = 8, FullName = "Selin Y�lmaz", ServiceId = 3, AvailableHours = "09:00-17:00" },
            new Employee { Id = 9, FullName = "Cemal Korkmaz", ServiceId = 3, AvailableHours = "09:30-18:30" },
            new Employee { Id = 10, FullName = "Elif �zt�rk", ServiceId = 4, AvailableHours = "08:00-14:00" },
            new Employee { Id = 11, FullName = "G�khan B�y�ker", ServiceId = 4, AvailableHours = "10:30-16:30" },
            new Employee { Id = 12, FullName = "Nurg�l Duman", ServiceId = 4, AvailableHours = "07:00-15:00" },
            new Employee { Id = 13, FullName = "Emre �ak�r", ServiceId = 5, AvailableHours = "11:00-19:00" },
            new Employee { Id = 14, FullName = "�rem K�l��", ServiceId = 5, AvailableHours = "09:00-17:00" },
            new Employee { Id = 15, FullName = "Seda Arslan", ServiceId = 5, AvailableHours = "10:00-18:00" },
            new Employee { Id = 16, FullName = "Serdar Kaya", ServiceId = 6, AvailableHours = "09:00-17:00" },
            new Employee { Id = 17, FullName = "Ayfer G�ne�", ServiceId = 6, AvailableHours = "08:00-16:00" },
            new Employee { Id = 18, FullName = "Orhan Kara", ServiceId = 6, AvailableHours = "09:30-17:30" }
        );

        // Veritaban�na kaydet
        await dbContext.SaveChangesAsync();
        Console.WriteLine("�al��anlar ba�ar�yla eklendi.");
    }
    
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
