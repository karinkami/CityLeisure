using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CityLeisure.Api.Data;
using CityLeisure.Api.Serialization;
using CityLeisure.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new SafeNullableDoubleJsonConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:5000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=city_leisure_db;Username=postgres;Password=postgres";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IFavoriteEventsService, FavoriteEventsService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IEventRecommendationService, EventRecommendationService>();
builder.Services.AddScoped<IWizardRecommendationService, WizardRecommendationService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IEventCategoryService, EventCategoryService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-secret-key-that-is-at-least-32-characters-long-for-security";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CityLeisure";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CityLeisure";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var adminEmails = builder.Configuration.GetSection("Admin:Emails").Get<string[]>() ?? Array.Empty<string>();

    await db.Database.ExecuteSqlRawAsync("""
        ALTER TABLE "Users"
        ADD COLUMN IF NOT EXISTS role character varying(32) NOT NULL DEFAULT 'User';
        """);

    await db.Database.ExecuteSqlRawAsync("""
        UPDATE "Users" SET role = 'User' WHERE role IS NULL OR trim(role) = '';
        """);

    foreach (var email in adminEmails.Where(e => !string.IsNullOrWhiteSpace(e)))
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE "Users"
            SET role = 'Admin'
            WHERE lower(email) = {email.Trim().ToLowerInvariant()};
            """);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseCors("AllowVueApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();


