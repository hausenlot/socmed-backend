using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using socmed_backend.Data;
using socmed_backend.Models;
using socmed_backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 104857600; // 100 MB
});

// --- Services ---
builder.Services.AddControllers();
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// App Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<RantService>(); // concrete registration for internal mapper usage
builder.Services.AddScoped<IRantService>(sp => sp.GetRequiredService<RantService>());
builder.Services.AddScoped<IRantInteractionService, RantInteractionService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReplyService, ReplyService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<ITimelineService, TimelineService>();
builder.Services.AddHttpClient<IMultimediaService, MultimediaService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

// CORS — allow frontend dev origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",   // React / CRA
                "http://localhost:5173"    // Vite
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// --- Middleware Pipeline ---
app.UseCors("FrontendPolicy");

app.UseStaticFiles(); // serves wwwroot/uploads/

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed initial test users if none exist
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Automatically apply migrations on startup
    context.Database.Migrate();

    if (!context.Users.Any(u => u.Id == "test-user-id"))
    {
        context.Users.Add(new User
        {
            Id = "test-user-id",
            Username = "test-user",
            DisplayName = "Test User",
            Bio = "I am a test user seed!"
        });
    }

    if (!context.Users.Any(u => u.Id == "test-user-2"))
    {
        context.Users.Add(new User
        {
            Id = "test-user-2",
            Username = "test-user-2",
            DisplayName = "Second Test User",
            Bio = "I am another test user seed!"
        });
    }

    context.SaveChanges();
}

app.Run();
