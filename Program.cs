using Hairdresser.Data;
using Hairdresser.Models;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// SQLite baðlantýsýný yapýlandýrma
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/User/Login"; // Giriþ yapýlmamýþsa yönlendirme yapýlacak sayfa
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

    // Veritabanýný oluþtur ve baþlangýç verisi ekle
    dbContext.Database.Migrate();

    // Hizmetleri sadece bir kez eklemek için kontrol et
    // Baþlangýç hizmetleri eklenir
    if (!dbContext.Services.Any())
    {
        dbContext.Services.AddRange(
            new Service { Id = 1, Name = "Saç Kesimi", Price = 700, Duration = 1 },
            new Service { Id = 2, Name = "Saç Boyasý", Price = 1500, Duration = 3 },
            new Service { Id = 3, Name = "Saç Bakýmý", Price = 1000, Duration = 2 },
            new Service { Id = 4, Name = "Profesyonel Makyaj", Price = 1000, Duration = 2 },
            new Service { Id = 5, Name = "Nail Art", Price = 1500, Duration = 1 },
            new Service { Id = 6, Name = "Cilt Bakýmý", Price = 1500, Duration = 3 }
        );
        // Veritabanýna kaydet
        await dbContext.SaveChangesAsync();
    }

    // Çalýþanlarý sadece bir kez eklemek için kontrol et
    // Baþlangýç çalýþanlarý eklenir
    if (!dbContext.Employees.Any()) // Eðer çalýþanlar yoksa ekle
    {
        dbContext.Employees.AddRange(
            new Employee { Id = 1, FullName = "Ahmet Yýlmaz", ServiceId = 1, AvailableHours = "09:00-17:00" },
            new Employee { Id = 2, FullName = "Ayþe Kaya", ServiceId = 1, AvailableHours = "10:00-18:00" },
            new Employee { Id = 3, FullName = "Mehmet Demir", ServiceId = 1, AvailableHours = "09:00-15:00" },
            new Employee { Id = 4, FullName = "Fatma Çelik", ServiceId = 2, AvailableHours = "10:00-16:00" },
            new Employee { Id = 5, FullName = "Hasan Yýldýz", ServiceId = 2, AvailableHours = "11:00-19:00" },
            new Employee { Id = 6, FullName = "Zeynep Aydýn", ServiceId = 2, AvailableHours = "08:00-16:00" },
            new Employee { Id = 7, FullName = "Veli Aksoy", ServiceId = 3, AvailableHours = "10:00-18:00" },
            new Employee { Id = 8, FullName = "Selin Yýlmaz", ServiceId = 3, AvailableHours = "09:00-17:00" },
            new Employee { Id = 9, FullName = "Cemal Korkmaz", ServiceId = 3, AvailableHours = "09:30-18:30" },
            new Employee { Id = 10, FullName = "Elif Öztürk", ServiceId = 4, AvailableHours = "08:00-14:00" },
            new Employee { Id = 11, FullName = "Gökhan Büyüker", ServiceId = 4, AvailableHours = "10:30-16:30" },
            new Employee { Id = 12, FullName = "Nurgül Duman", ServiceId = 4, AvailableHours = "07:00-15:00" },
            new Employee { Id = 13, FullName = "Emre Çakýr", ServiceId = 5, AvailableHours = "11:00-19:00" },
            new Employee { Id = 14, FullName = "Ýrem Kýlýç", ServiceId = 5, AvailableHours = "09:00-17:00" },
            new Employee { Id = 15, FullName = "Seda Arslan", ServiceId = 5, AvailableHours = "10:00-18:00" },
            new Employee { Id = 16, FullName = "Serdar Kaya", ServiceId = 6, AvailableHours = "09:00-17:00" },
            new Employee { Id = 17, FullName = "Ayfer Güneþ", ServiceId = 6, AvailableHours = "08:00-16:00" },
            new Employee { Id = 18, FullName = "Orhan Kara", ServiceId = 6, AvailableHours = "09:30-17:30" }
        );

        // Veritabanýna kaydet
        await dbContext.SaveChangesAsync();
        Console.WriteLine("Çalýþanlar baþarýyla eklendi.");
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
