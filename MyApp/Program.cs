using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyApp.Data;
using MyApp.Helpers;
using MyApp.Repositories.Implementations;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Implementations;
using MyApp.Services.Interfaces;
using System.Security.Claims;
using System.Text;
using CloudinaryDotNet;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// --------------------------- DATABASE ---------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --------------------------- AUTOMAPPER ---------------------------
builder.Services.AddAutoMapper(typeof(MappingProfile));

// --------------------------- DEPENDENCY INJECTION ---------------------------
// Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddHttpContextAccessor();

//rAZORPAY
builder.Services.Configure<RazorpaySettings>(builder.Configuration.GetSection("Razorpay"));
builder.Services.AddScoped<IRazorpayService, RazorpayService>();



// --------------------------- CLOUDINARY ---------------------------
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings")

);


var cloudinarySettings = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
Console.WriteLine($"Loaded CloudinarySettings: CloudName={cloudinarySettings?.CloudName}, ApiKey={cloudinarySettings?.ApiKey}, ApiSecret=***");
Console.WriteLine($"Loaded CloudinarySettings: CloudName={cloudinarySettings?.CloudName}, ApiKey={cloudinarySettings?.ApiKey}, ApiSecret=***");
if (string.IsNullOrWhiteSpace(cloudinarySettings?.CloudName) || string.IsNullOrWhiteSpace(cloudinarySettings?.ApiKey) || string.IsNullOrWhiteSpace(cloudinarySettings?.ApiSecret))
{
    throw new InvalidOperationException("Cloudinary settings are missing or invalid in appsettings.json.");
}

builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
    var account = new Account(cfg.CloudName, cfg.ApiKey, cfg.ApiSecret);
    var cloudinary = new Cloudinary(account)
    {
        Api = { Secure = true }
    };
    return cloudinary;
});

builder.Services.AddScoped<IImageService, CloudinaryImageService>();

// --------------------------- JWT AUTHENTICATION ---------------------------
var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt["Key"] ?? throw new ArgumentNullException("Jwt:Key");

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
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        RoleClaimType = ClaimTypes.Role
    };
});

// --------------------------- CONTROLLERS ---------------------------
builder.Services.AddControllers();

// --------------------------- SWAGGER ---------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyApp API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR5..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// --------------------------- MIDDLEWARE ---------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // /swagger
}

app.UseMiddleware<MyApp.Middleware.ExceptionMiddleware>();

// Seed database if needed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    SeedData.Initialize(context);
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
