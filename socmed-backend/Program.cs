using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using socmed_backend.Data;
using socmed_backend.Models;
using socmed_backend.Services;
using socmed_backend.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 104857600; // 100 MB
});

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddSignalR();
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

    // Support SignalR JWT in query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
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
app.MapHub<NotificationHub>("/hubs/notifications");

// Seed initial test users if none exist
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Hard Reset is done, removing EnsureDeleted() to persist data
    context.Database.Migrate();

    /*
    // Custom seeding: 10 rants for each existing user
    var users = await context.Users.Include(u => u.Rants).ToListAsync();
    foreach (var user in users)
    {
        if (!user.Rants.Any())
        {
            Console.WriteLine($"Seeding 10 rants for user: {user.Username}");
            for (int i = 1; i <= 10; i++)
            {
                context.Rants.Add(new Rant
                {
                    Content = $"Rant #{i} from @{user.Username}! 🚀 Just testing out the new clean slate.",
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i * 10), // Spread them out slightly
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-i * 10)
                });
            }
        }
    }

    await context.SaveChangesAsync();
    */
}

app.Run();
