using Gamestore.WebApi.Logging;
using Gamestore.WebApi.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Gamestore.Services.Services.Auth;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Text.Json;
using Gamestore.Services.Services.Business;
using Gamestore.Services.Services.Community;
using Gamestore.Services.Services.Filters;
using Gamestore.Services.Services.Auth.Management;
using Gamestore.Services.Interfaces;
using Gamestore.Data.Interfaces;
using Gamestore.Data.Repositories;
using Gamestore.Data.Data;
using Gamestore.Services.Services.Orders;
using Gamestore.Services.Services.Payment;

var builder = WebApplication.CreateBuilder(args);

// Configure all services
ConfigureLogging(builder);
ConfigureBasicServices(builder);
ConfigureDatabase(builder);
ConfigureSwagger(builder);
ConfigureAuthentication(builder);
ConfigureAuthorization(builder);
ConfigureCors(builder);
ConfigureBusinessServices(builder);
ConfigureExternalAuthService(builder);

var app = builder.Build();

// Configure middleware pipeline
await ConfigureMiddlewarePipeline(app);

app.Run();

// Configuration Methods

static void ConfigureLogging(WebApplicationBuilder builder)
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();

    builder.Logging.AddFile(options =>
    {
        options.LogDirectory = "Logs";
        options.FileSizeLimit = 20 * 1024 * 1024;
        options.RetainedFileCountLimit = 31;
        options.MinimumLogLevel = LogLevel.Information;
        options.UseUtcTimestamp = false;
        options.FileNamePrefix = "app_";
    });
}

static void ConfigureBasicServices(WebApplicationBuilder builder)
{
    builder.Services.AddControllersWithViews()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new EmptyStringToNullGuidConverter());
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });

    builder.Services.AddOutputCache();
}

static void ConfigureDatabase(WebApplicationBuilder builder)
{
    builder.Services.AddDbContext<GameCatalogDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

static void ConfigureSwagger(WebApplicationBuilder builder)
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Game Store API",
            Version = "v1",
            Description = "API for Game Store with JWT Authentication and Database User Management"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
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
}

static void ConfigureAuthentication(WebApplicationBuilder builder)
{
    const string JWT_SECRET_KEY = "GamestoreSecretKeyForJWTTokenGeneration2024!ThisMustBeLongEnoughForSecurity";
    var jwtKey = Encoding.UTF8.GetBytes(JWT_SECRET_KEY);

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true,
                RequireSignedTokens = true
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("🚫 JWT Authentication failed: {Error}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    var email = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                    var role = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                    logger.LogInformation("✅ JWT Token validated for user: {Email}, Role: {Role}", email, role);
                    return Task.CompletedTask;
                }
            };
        });
}

static void ConfigureAuthorization(WebApplicationBuilder builder)
{
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("CanManageGames", policy =>
            policy.RequireAssertion(context =>
                GetUserRoleFromContext(context) is Roles.Administrator or Roles.Manager))

        .AddPolicy("CanManageBusinessEntities", policy =>
            policy.RequireAssertion(context =>
                GetUserRoleFromContext(context) is Roles.Administrator or Roles.Manager))

        .AddPolicy("CanViewDeletedGames", policy =>
            policy.RequireAssertion(context =>
                GetUserRoleFromContext(context) == Roles.Administrator))

        .AddPolicy("CanEditDeletedGames", policy =>
            policy.RequireAssertion(context =>
                GetUserRoleFromContext(context) == Roles.Administrator))

        .AddPolicy("CanManageUsers", policy =>
            policy.RequireAssertion(context =>
                GetUserRoleFromContext(context) == Roles.Administrator))

        .AddPolicy("CanManageRoles", policy =>
            policy.RequireAssertion(context =>
                GetUserRoleFromContext(context) == Roles.Administrator))

        .AddPolicy("CanModerateComments", policy =>
            policy.RequireAssertion(context =>
                GetUserRoleFromContext(context) is Roles.Administrator or Roles.Manager or Roles.Moderator))

        .AddPolicy("CanBanUsers", policy =>
            policy.RequireAssertion(context =>
                GetUserRoleFromContext(context) is Roles.Administrator or Roles.Manager or Roles.Moderator))

        .AddPolicy("CanManageOrders", policy =>
            policy.RequireAssertion(context =>
                GetUserRoleFromContext(context) is Roles.Administrator or Roles.Manager))

        .AddPolicy("CanAddComments", policy =>
            policy.RequireAssertion(context =>
            {
                if (context.User.Identity?.IsAuthenticated != true)
                {
                    return false;
                }

                var userRole = GetUserRoleFromContext(context);
                return userRole != Roles.Guest;
            }))

        .AddPolicy("CanBuyGames", policy =>
            policy.RequireAssertion(context =>
            {
                if (context.User.Identity?.IsAuthenticated != true)
                {
                    return false;
                }

                var userRole = GetUserRoleFromContext(context);
                return userRole != Roles.Guest;
            }));
}

static void ConfigureCors(WebApplicationBuilder builder)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("x-total-numbers-of-games", "Authorization"));
    });
}

static void ConfigureBusinessServices(WebApplicationBuilder builder)
{
    // Business Services
    builder.Services.AddScoped<IGameService, GameService>();
    builder.Services.AddScoped<IGenreService, GenreService>();
    builder.Services.AddScoped<IPlatformService, PlatformService>();
    builder.Services.AddScoped<IPublisherService, PublisherService>();
    builder.Services.AddScoped<ICommentService, CommentService>();
    builder.Services.AddScoped<IGameFilterService, GameFilterService>();

    // Data Access
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Utility Services
    builder.Services.AddScoped<ErrorLoggingService>();

    // Authentication & Authorization Services
    builder.Services.AddScoped<IDatabaseRoleService, DatabaseRoleService>();
    builder.Services.AddScoped<IRoleManagementService, RoleManagementService>();
    builder.Services.AddScoped<IUserManagementService, UserManagementService>();

    // Orders and Payment Services
    builder.Services.AddScoped<ICartService, CartService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<IPaymentService, PaymentService>();
    builder.Services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
    builder.Services.AddHttpClient<IPaymentMicroserviceClient, PaymentMicroserviceClient>(client =>
    {
        var config = builder.Configuration;
        var baseUrl = config["PaymentMicroservice:BaseUrl"] ?? "https://localhost:5001";
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(config.GetValue("PaymentMicroservice:Timeout", 30));

        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });
}

static void ConfigureExternalAuthService(WebApplicationBuilder builder)
{
    var useExternalAuth = builder.Configuration.GetValue("UseExternalAuthService", false);
    if (!useExternalAuth)
    {
        return;
    }

    builder.Services.AddHttpClient<AuthService>(client =>
    {
        var config = builder.Configuration;
        var baseUrl = config["AuthService:BaseUrl"];
        if (string.IsNullOrEmpty(baseUrl))
        {
            throw new InvalidOperationException(
                "AuthService:BaseUrl configuration is required when UseExternalAuthService is enabled. " +
                "Please add 'AuthService:BaseUrl' to your appsettings.json file.");
        }

        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(config.GetValue("AuthService:Timeout", 30));
    });

    builder.Services.AddScoped<AuthService>();
}

static async Task ConfigureMiddlewarePipeline(WebApplication app)
{
    // Database Initialization
    await InitializeDatabaseAsync(app);

    // Middleware Pipeline
    app.UseMiddleware<ExceptionMiddleware>();
    app.UseMiddleware<TotalGamesMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseAuthHeaderFix();

    ConfigureDevelopmentMiddleware(app);
    ConfigureProductionMiddleware(app);

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseCors("AllowAll");
    app.UseRouting();

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAuthorizationLogging();

    // Endpoints
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapFallbackToFile("index.html");

    LogStartupInformation(app);
}

static void ConfigureDevelopmentMiddleware(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
    {
        return;
    }

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Game Store API V1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Game Store API";
    });
}

static void ConfigureProductionMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        return;
    }

    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

static void LogStartupInformation(WebApplication app)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("🚀 Game Store API starting...");
    logger.LogInformation("🔐 JWT Authentication enabled");
    logger.LogInformation("🛡️ Authorization policies configured");
    logger.LogInformation("🗄️ Database authentication enabled");
    logger.LogInformation("📚 Swagger available at: /swagger");
    logger.LogInformation("🌐 Environment: {Environment}", app.Environment.EnvironmentName);
}

// Helper Methods

static string GetUserRoleFromContext(AuthorizationHandlerContext context)
{
    return context.User.FindFirst("role")?.Value ??
           context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ??
           Roles.Guest;
}

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("🗄️ Initializing database...");

        var context = services.GetRequiredService<GameCatalogDbContext>();
        await context.Database.EnsureCreatedAsync();

        var databaseRoleService = services.GetRequiredService<IDatabaseRoleService>();
        await databaseRoleService.SeedDefaultRolesAndPermissionsAsync();
        await databaseRoleService.MigrateHardcodedUsersAsync();

        logger.LogInformation("✅ Database initialization completed");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error during database initialization");
        throw;
    }
}