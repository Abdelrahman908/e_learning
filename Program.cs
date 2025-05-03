using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using e_learning.Data;
using e_learning.Hubs;
using e_learning.Models;
using e_learning.DTOs.Responses;
using e_learning.Service.Interfaces;
using e_learning.Service.Implementations;
using e_learning.Repositories.Interfaces;
using e_learning.Repositories;
using e_learning.Service;
using e_learning.Services;
using e_learning.Mappings;
using AutoMapper;


var builder = WebApplication.CreateBuilder(args);
// 1. Configure Serilog Logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// 2. Database Configuration
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null));

    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
        options.LogTo(Console.WriteLine);
    }
});

// 3. JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/notificationHub") || path.StartsWithSegments("/chatHub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Log.Error(context.Exception, "JWT Authentication Failed");
            return Task.CompletedTask;
        }
    };
});

// 4. Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromSeconds(10);
        limiterOptions.PermitLimit = 5;
        limiterOptions.QueueLimit = 2;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// 5. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000", "https://yourapp.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// 6. Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "E-Learning API",
        Version = "v1",
        Description = "API لنظام التعلم الإلكتروني",
        Contact = new OpenApiContact
        {
            Name = "فريق التطوير",
            Email = "dev@elearning.com",
            Url = new Uri("https://elearning.com/support")
        },
        License = new OpenApiLicense { Name = "MIT" }
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Description = "أدخل التوكن بهذا الشكل: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
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

// 7. SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

// 8. Controllers + JSON Settings
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// 9. Register All Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(QuizProfile).Assembly);

// Register services
builder.Services.AddScoped<IQuizService, QuizService>();

builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<ILessonFileService, LessonFileService>();
// تسجيل خدمات الملفات
builder.Services.AddScoped<IFileService, LessonFileService>();
builder.Services.AddScoped<ILessonFileService, LessonFileService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IEmailConfirmationService, EmailConfirmationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPasswordValidator, PasswordValidator>();
builder.Services.AddSingleton<RateLimiterService>();
builder.Services.AddHttpClient<RecommendationService>();

// 10. Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("Database")
    .AddCheck<FileStorageHealthCheck>("File Storage");

// Build App
var app = builder.Build();

// Exception Handler early to catch startup errors
app.UseExceptionHandler("/error");

// Migrate & Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        await SeedData.Initialize(services);
        Log.Information("Database updated successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database migration failed");
    }
}

// Development Tools
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Learning API v1");
        c.ConfigObject.DisplayRequestDuration = true;
        c.InjectStylesheet("/swagger-ui/custom.css");
        c.DisplayOperationId();
        c.EnableDeepLinking();
    });
    app.UseDeveloperExceptionPage();
}

// Middleware Pipeline
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=604800")
});
app.UseRouting();
app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHub<NotificationHub>("/notificationHub").RequireCors("AllowAll").RequireAuthorization();
app.MapHub<ChatHub>("/chatHub").RequireCors("AllowAll").RequireAuthorization();

// Global Error Endpoint
app.Map("/error", (HttpContext context) =>
{
    var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    Log.Error(exception, "Unexpected Error Occurred");
    return Results.Json(new ApiResponse(false, "حدث خطأ غير متوقع"), statusCode: StatusCodes.Status500InternalServerError);
});

// Run App
try
{
    Log.Information("Starting application...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Data Seeder
public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!await context.Users.AnyAsync())
        {
            var users = new List<User>
            {
                new User
                {
                    FullName = "مدير النظام",
                    Email = "admin@elearning.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "Admin",
                    IsEmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    FullName = "مدرس نموذجي",
                    Email = "instructor@elearning.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Instructor@123"),
                    Role = "Instructor",
                    IsEmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    FullName = "طالب نموذجي",
                    Email = "student@elearning.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                    Role = "Student",
                    IsEmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        // Seed initial courses if none exist
        if (!await context.Courses.AnyAsync() && await context.Users.AnyAsync(u => u.Role == "Instructor"))
        {
            var instructor = await context.Users.FirstAsync(u => u.Role == "Instructor");

            var courses = new List<Course>
            {
                new Course
                {
                    Title = "دورة البرمجة الأساسية",
                    Description = "تعلم أساسيات البرمجة للمبتدئين",
                    InstructorId = instructor.Id,
                    Price = 0,
                    CreatedAt = DateTime.UtcNow
                },
                new Course
                {
                    Title = "تعلم ASP.NET Core",
                    Description = "دورة متقدمة لتعلم تطوير الويب باستخدام ASP.NET Core",
                    InstructorId = instructor.Id,
                    Price = 100,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Courses.AddRangeAsync(courses);
            await context.SaveChangesAsync();
        }
    }
}

// Health Check for File Storage
public class FileStorageHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tempPath = Path.GetTempPath();
            var testFile = Path.Combine(tempPath, $"healthcheck_{Guid.NewGuid()}.tmp");

            File.WriteAllText(testFile, "Health check test");
            File.Delete(testFile);

            return Task.FromResult(HealthCheckResult.Healthy("File storage is working properly"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy("File storage is not accessible", ex));
        }
    }
}