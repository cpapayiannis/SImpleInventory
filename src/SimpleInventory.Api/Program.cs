using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SimpleInventory.Common.Classes;           // JwtSettings if you keep it here
using SimpleInventory.DataAccess;
using SimpleInventory.DataAccess.Classes;       // SimpleInventoryRepository
using SimpleInventory.DataAccess.Intefaces;     // ISimpleInventoryRepository

var builder = WebApplication.CreateBuilder(args);

// ---------------- Connection string (SQLite; make relative path absolute) ----------------
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

// ---------------- JWT settings ----------------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt section missing.");
if (string.IsNullOrWhiteSpace(jwt.Key))
    throw new InvalidOperationException("Jwt:Key is empty.");

// ---------------- Services ----------------
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

builder.Services.AddControllers(); // API controllers only

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins("https://localhost:44329", "https://localhost:5173") // UI ports
    .AllowAnyHeader()
    .AllowAnyMethod()));

// ---------------- Auth (JWT Bearer) ----------------
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });

builder.Services.AddAuthorization();

// ---------------- Swagger (with Bearer) ----------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SimpleInventory API", Version = "v1" });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT as: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { scheme, Array.Empty<string>() }
    });
});



var app = builder.Build();

// ---------------- Pipeline ----------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(); 
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); 

// ---------------- DB init (apply migrations) ----------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SimpleInventoryContext>();
    Console.WriteLine($"[DB] Provider: {db.Database.ProviderName}");
    Console.WriteLine($"[DB] ConnStr:  {db.Database.GetDbConnection().ConnectionString}");
    if (!db.Database.GetAppliedMigrations().Any())
        db.Database.EnsureCreated();
    else
        db.Database.Migrate();
}

app.Run();
