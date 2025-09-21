using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SimpleInventory.DataAccess;
using SimpleInventory.DataAccess.Classes;
using SimpleInventory.DataAccess.Intefaces;

var builder = WebApplication.CreateBuilder(args);

// ---- Connection string (SQLite; make relative paths absolute) ----
var rawCs = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default missing.");

var csb = new SqliteConnectionStringBuilder(rawCs);
if (!Path.IsPathRooted(csb.DataSource))
{
    var abs = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, csb.DataSource));
    Directory.CreateDirectory(Path.GetDirectoryName(abs)!);
    csb.DataSource = abs;
}
var connectionString = csb.ToString();

// ---- Services ----
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<SimpleInventoryContext>(opt =>
{
    opt.UseSqlite(connectionString);
    if (builder.Environment.IsDevelopment())
    {
        opt.EnableDetailedErrors();
        opt.EnableSensitiveDataLogging();
    }
});

builder.Services.AddScoped<ISimpleInventoryRepository, SimpleInventoryRepository>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.LogoutPath = "/Account/Logout";
        o.AccessDeniedPath = "/Account/Denied";
    });
var app = builder.Build();

// ---- Pipeline ----
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// If you add auth later, keep these (otherwise omit):
// app.UseAuthentication();
app.UseAuthorization();

// ---- DB init (apply migrations) ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SimpleInventoryContext>();
    Console.WriteLine($"[DB] Provider: {db.Database.ProviderName}");
    Console.WriteLine($"[DB] File:     {db.Database.GetDbConnection().DataSource}");
    Console.WriteLine($"[DB] Exists:   {System.IO.File.Exists(db.Database.GetDbConnection().DataSource)}");
    db.Database.Migrate(); // or EnsureCreated() for dev-only
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
