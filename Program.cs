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
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Cookie'nin ge�erlilik s�resi
        options.SlidingExpiration = false;
        options.LoginPath = "/User/Login"; // Giri� yap�lmam��sa y�nlendirme yap�lacak sayfa
    });

builder.Services.AddDistributedMemoryCache(); // Oturum i�in bellek �nbelle�i
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum zaman a��m�
    options.Cookie.HttpOnly = true; // G�venlik i�in sadece HTTP
    options.Cookie.IsEssential = true; // Oturum �erezlerinin �al��mas� i�in gerekli
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
            new Service { ServiceID = 1, Name = "Sa� Kesimi", Price = 700, Duration = 1 },
            new Service { ServiceID = 2, Name = "Sa� Boyas�", Price = 1500, Duration = 3 },
            new Service { ServiceID = 3, Name = "Sa� Bak�m�", Price = 1000, Duration = 2 },
            new Service { ServiceID = 4, Name = "Profesyonel Makyaj", Price = 1000, Duration = 2 },
            new Service { ServiceID = 5, Name = "Nail Art", Price = 1500, Duration = 1 },
            new Service { ServiceID = 6, Name = "Cilt Bak�m�", Price = 1500, Duration = 3 }
        );
        // Veritaban�na kaydet
        await dbContext.SaveChangesAsync();
    }

    // �al��anlar� sadece bir kez eklemek i�in kontrol et
    // Ba�lang�� �al��anlar� eklenir
    if (!dbContext.Employees.Any()) // E�er �al��anlar yoksa ekle
    {
        dbContext.Employees.AddRange(
            new Employee { EmployeeID = "1", FullName = "Ahmet Y�lmaz", EmployeeServiceID = 1, AvailableHours = "09:00-17:00" },
            new Employee { EmployeeID = "2", FullName = "Ay�e Kaya", EmployeeServiceID = 1, AvailableHours = "10:00-18:00" },
            new Employee { EmployeeID = "3", FullName = "Mehmet Demir", EmployeeServiceID = 1, AvailableHours = "09:00-15:00" },
            new Employee { EmployeeID = "4", FullName = "Fatma �elik", EmployeeServiceID = 2, AvailableHours = "10:00-16:00" },
            new Employee { EmployeeID = "5", FullName = "Hasan Y�ld�z", EmployeeServiceID = 2, AvailableHours = "11:00-19:00" },
            new Employee { EmployeeID = "6", FullName = "Zeynep Ayd�n", EmployeeServiceID = 2, AvailableHours = "08:00-16:00" },
            new Employee { EmployeeID = "7", FullName = "Veli Aksoy", EmployeeServiceID = 3, AvailableHours = "10:00-18:00" },
            new Employee { EmployeeID = "8", FullName = "Selin Y�lmaz", EmployeeServiceID = 3, AvailableHours = "09:00-17:00" },
            new Employee { EmployeeID = "9", FullName = "Cemal Korkmaz", EmployeeServiceID = 3, AvailableHours = "09:30-18:30" },
            new Employee { EmployeeID = "10", FullName = "Elif �zt�rk", EmployeeServiceID = 4, AvailableHours = "08:00-14:00" },
            new Employee { EmployeeID = "11", FullName = "G�khan B�y�ker", EmployeeServiceID = 4, AvailableHours = "10:30-16:30" },
            new Employee { EmployeeID = "12", FullName = "Nurg�l Duman", EmployeeServiceID = 4, AvailableHours = "07:00-15:00" },
            new Employee { EmployeeID = "13", FullName = "Emre �ak�r", EmployeeServiceID = 5, AvailableHours = "11:00-19:00" },
            new Employee { EmployeeID = "14", FullName = "�rem K�l��", EmployeeServiceID = 5, AvailableHours = "09:00-17:00" },
            new Employee { EmployeeID = "15", FullName = "Seda Arslan", EmployeeServiceID = 5, AvailableHours = "10:00-18:00" },
            new Employee { EmployeeID = "16", FullName = "Serdar Kaya", EmployeeServiceID = 6, AvailableHours = "09:00-17:00" },
            new Employee { EmployeeID = "17", FullName = "Ayfer G�ne�", EmployeeServiceID = 6, AvailableHours = "08:00-16:00" },
            new Employee { EmployeeID = "18", FullName = "Orhan Kara", EmployeeServiceID = 6, AvailableHours = "09:30-17:30" }
        );

        // Veritaban�na kaydet
        await dbContext.SaveChangesAsync();
        Console.WriteLine("�al��anlar ba�ar�yla eklendi.");
    }

    // Hizmetleri sadece bir kez eklemek i�in kontrol et
    // Ba�lang�� hizmetleri eklenir
    //if (!dbContext.Appointments.Any())
    //{
    //    var service = dbContext.Services.FirstOrDefault(s => s.ServiceID == 1); // ID = 1 olan servisi al
    //    dbContext.Appointments.AddRange( 
    //        new Appointment {
    //            AppointmentID = 1,
    //            AppointmentServiceID = service.ServiceID,
    //            AppointmentUserID = "11159931640",
    //            AppointmentEmployeeID = "11111111111",
    //            AppointmentDate = new DateTime(2024, 12, 15, 14, 30, 0), // Do�ru format
    //            Status = "Beklemede",
    //            TotalPrice = service.Price
    //        }

    //    );
    //    // Veritaban�na kaydet
    //    await dbContext.SaveChangesAsync();
    //}

    if (!dbContext.Roles.Any())
    {
        dbContext.Roles.AddRange(
            new Role { RoleID = 1, RoleName = "user" },
            new Role { RoleID = 2, RoleName = "admin" }
        );
        // Veritaban�na kaydet
        await dbContext.SaveChangesAsync();
    }

    // Kullan�c�lar
    if (!dbContext.User.Any())
    {
        var adminRole = dbContext.Roles.FirstOrDefault(r => r.RoleName == "Admin");
        var userRole = dbContext.Roles.FirstOrDefault(r => r.RoleName == "User");

        dbContext.User.AddRange(
            new User
            {
                UserID = "12345678901",
                FullName = "Admin User",
                Email = "admin@admin.com",
                PhoneNumber = "1234567890",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                UserRoleID = adminRole.RoleID
            },
            new User
            {
                UserID = "11159931640",
                FullName = "Humeyra Ozturk",
                Email = "humeyra.ozturk2@ogr.sakarya.edu.tr",
                PhoneNumber = "05534317361",
                Password = BCrypt.Net.BCrypt.HashPassword("Humeyra123"),
                UserRoleID = userRole.RoleID
            }
        );
        dbContext.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
