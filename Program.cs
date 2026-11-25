using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");


// Demo login accounts
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    var demoUsers = new List<User>
    {
        new User
        {
            Name = "Library Admin",
            Email = "admin@library.com",
            Password = "admin123",
            Role = "Admin"
        },

        new User
        {
            Name = "Demo Borrower",
            Email = "user@library.com",
            Password = "user123",
            Role = "User"
        },

        new User
        {
            Name = "meet",
            Email = "user123@gmail.com",
            Password = "123456789",
            Role = "User"
        }
    };

    foreach (var user in demoUsers)
    {
        var existingUser = context.Users
            .FirstOrDefault(u => u.Email == user.Email);

        if (existingUser == null)
        {
            context.Users.Add(user);
        }
        else
        {
            existingUser.Name = user.Name;
            existingUser.Role = user.Role;
        }
    }

    context.SaveChanges();

    context.SaveChanges();
}

app.Run();