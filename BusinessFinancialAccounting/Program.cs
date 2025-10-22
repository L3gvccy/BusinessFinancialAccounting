using BusinessFinancialAccounting.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var provider = builder.Configuration.GetValue<string>("DatabaseProvider")?.ToLowerInvariant() ?? "sqlserver";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    switch (provider)
    {
        case "sqlserver":
            var sqlConn = builder.Configuration.GetConnectionString("SqlServer");
            options.UseSqlServer(sqlConn);
            break;

        case "postgres":
            var pg = builder.Configuration.GetConnectionString("Postgres");
            options.UseNpgsql(pg, b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            break;

        case "sqlite":
            var sqlite = builder.Configuration.GetConnectionString("Sqlite");
            options.UseSqlite(sqlite);
            break;

        case "inmemory":
            options.UseInMemoryDatabase("BusinessFinancialAccounting_InMemory");
            break;

        default:
            throw new InvalidOperationException($"Unknown DatabaseProvider: {provider}");
    }
});


builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google";
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (provider == "inmemory")
    {
        // Ensure created (InMemory)
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }
    else if (provider == "sqlite" || provider == "sqlserver" || provider == "postgres")
    {
        // Apply migrations (make sure you created migrations)
        // db.Database.Migrate();
    }
}

app.Run();
